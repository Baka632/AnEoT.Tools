using System.Diagnostics.CodeAnalysis;
using System.Text;
using SixLabors.ImageSharp;
using Windows.Storage;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace AnEoT.Tools.VolumeCreator.Models.Resources;

internal sealed partial class ProjectPackageResourcesHelper(ProjectPackage projectPackage) : IVolumeResourcesHelper, IDisposable, IAsyncDisposable
{
    public bool ConvertWebP
    {
        get => ProjectPackage.Info.ImageConvertToWebp;
        set => ProjectPackage.Info = ProjectPackage.Info with { ImageConvertToWebp = value };
    }
    public bool HasCover { get => ProjectPackage.HasCoverImage; }
    public ProjectPackage ProjectPackage { get; } = projectPackage;

    public async Task SaveProjectAsync()
    {
        await ProjectPackage.SaveAsync();
    }

    public async Task<Stream?> GetCoverAsync()
    {
        return await ProjectPackage.GetCoverFileAsync();
    }

    public async Task SetCoverAsync(string coverPath)
    {
        await ProjectPackage.SetCoverFileAsync(coverPath);
    }

    public async Task ExportAssetsAsync(IEnumerable<AssetNode> files, StorageFolder outputFolder)
    {
        await Parallel.ForEachAsync(files, async (node, ct) =>
        {
            await ExportAssetsCore(node, outputFolder);
        });
    }

    private async Task ExportAssetsCore(AssetNode node, StorageFolder outputFolder)
    {
        if (node.Type == AssetNodeType.Folder)
        {
            foreach (AssetNode item in node.Children)
            {
                if (item.Type == AssetNodeType.Folder)
                {
                    if (item.Children.Count > 0)
                    {
                        StorageFolder folder = await outputFolder.CreateFolderAsync(item.DisplayName, CreationCollisionOption.OpenIfExists);

                        await Parallel.ForEachAsync(item.Children, async (subItem, ct) =>
                        {
                            await ExportAssetsCore(subItem, folder);
                        });
                    }
                }
                else if (item.Type == AssetNodeType.File && item is FileNode fileNode)
                {
                    await SaveFileNode(fileNode, outputFolder);
                }
            }
        }
        else if (node.Type == AssetNodeType.File && node is FileNode fileNode)
        {
            await SaveFileNode(fileNode, outputFolder);
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debugger.Break();
        }
#endif

        async Task SaveFileNode(FileNode fileNode, StorageFolder rootFolder)
        {
            if (fileNode.IsRelativePath)
            {
                using Stream? stream = await ProjectPackage.GetAssetAsync(fileNode);
                if (stream == null)
                {
                    return;
                }
                stream.Seek(0, SeekOrigin.Begin);

                if (ConvertWebP && !Path.GetExtension(fileNode.FilePath).Equals(".webp", StringComparison.OrdinalIgnoreCase))
                {
                    using ImageSharpImage image = await ImageSharpImage.LoadAsync(stream);
                    StorageFile target = await rootFolder.CreateFileAsync(Path.ChangeExtension(fileNode.DisplayName, ".webp"));

                    using Stream targetStream = await target.OpenStreamForWriteAsync();
                    await Task.Run(() => image.SaveAsWebp(targetStream)); // 防止卡主线程，ImageSharp 库自带的异步方法有点问题
                }
                else
                {
                    StorageFile target = await rootFolder.CreateFileAsync(fileNode.DisplayName);
                    using Stream targetStream = await target.OpenStreamForWriteAsync();
                    targetStream.SetLength(0);

                    await stream.CopyToAsync(targetStream);
                }
            }
            else
            {
                if (!fileNode.IsFileExist)
                {
                    return;
                }

                if (ConvertWebP && !Path.GetExtension(fileNode.FilePath).Equals(".webp", StringComparison.OrdinalIgnoreCase))
                {
                    using ImageSharpImage image = await ImageSharpImage.LoadAsync(fileNode.FilePath);
                    StorageFile target = await rootFolder.CreateFileAsync(Path.ChangeExtension(fileNode.DisplayName, ".webp"));

                    using Stream targetStream = await target.OpenStreamForWriteAsync();
                    await Task.Run(() => image.SaveAsWebp(targetStream)); // 防止卡主线程，ImageSharp 库自带的异步方法有点问题
                }
                else
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(fileNode.FilePath);
                    await file.CopyAsync(rootFolder);
                }
            }
        }
    }

    public bool ValidateAssets(IEnumerable<AssetNode> files, [NotNullWhen(false)] out string? errorMessage)
    {
        bool success = ValidateAssetsCore(files, out string? msg);
        errorMessage = msg;
        return success;
    }

    public async Task<Stream?> GetAssetsAsync(FileNode fileNode)
    {
        if (fileNode.IsRelativePath)
        {
            return await ProjectPackage.GetAssetAsync(fileNode);
        }
        else if (fileNode.IsFileExist)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(fileNode.FilePath);
            return await file.OpenStreamForReadAsync();
        }
        else
        {
            return null;
        }
    }

    private bool ValidateAssetsCore(IEnumerable<AssetNode> assets, [NotNullWhen(false)] out string? errorMessage)
    {
        bool isSuccess = true;
        StringBuilder builder = new();

        foreach (AssetNode node in assets)
        {
            if (node is FolderNode folderNode)
            {
                if (folderNode.Children.Count <= 0)
                {
                    continue;
                }

                bool success = ValidateAssetsCore(folderNode.Children, out string? msg);

                if (!success)
                {
                    builder.AppendLine(msg);
                    isSuccess = false;
                }
            }
            else if (node is FileNode fileNode)
            {
                if (fileNode.IsRelativePath)
                {
                    if (!ProjectPackage.ContainsAsset(fileNode))
                    {
                        builder.AppendLine(fileNode.FilePath);
                        isSuccess = false;
                    }
                }
            }
#if DEBUG
            else
            {
                System.Diagnostics.Debugger.Break();
            }
#endif
        }

        errorMessage = builder.ToString();
        return isSuccess;
    }

    public void Dispose()
    {
        ProjectPackage.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await ProjectPackage.DisposeAsync();
    }
}

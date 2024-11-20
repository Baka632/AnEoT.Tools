using System.Diagnostics.CodeAnalysis;
using System.Text;
using SixLabors.ImageSharp;
using Windows.Storage;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace AnEoT.Tools.VolumeCreator.Models.Resources;

internal sealed class MemoryResourcesHelper : IVoulmeResourcesHelper
{
    private StorageFile? coverFile;

    public bool ConvertWebP { get; set; }
    [MemberNotNullWhen(true, nameof(coverFile))]
    public bool HasCover { get => coverFile is not null; }

    public async Task<Stream?> GetCoverAsync()
    {
        if (HasCover)
        {
            Stream coverStream = await coverFile.OpenStreamForReadAsync();
            coverStream.Seek(0, SeekOrigin.Begin);
            return coverStream;
        }
        else
        {
            return null;
        }
    }

    public async Task SetCoverAsync(string coverPath)
    {
        if (string.IsNullOrWhiteSpace(coverPath))
        {
            throw new ArgumentException($"“{nameof(coverPath)}”不能为 null 或空白。", nameof(coverPath));
        }

        StorageFile file = await StorageFile.GetFileFromPathAsync(coverPath);
        coverFile = file;
    }

    public async Task ExportAssetsAsync(IEnumerable<AssetNode> files, StorageFolder outputFolder)
    {
        foreach (AssetNode node in files)
        {
            await CopyContentRecursively(node, outputFolder);
        }
    }

    public async Task<Stream?> GetAssetsAsync(FileNode fileNode)
    {
        if (fileNode.IsRelativePath || !fileNode.IsFileExist)
        {
            return null;
        }

        StorageFile file = await StorageFile.GetFileFromPathAsync(fileNode.FilePath);
        return await file.OpenStreamForReadAsync();
    }

    public bool ValidateAssets(IEnumerable<AssetNode> files, [NotNullWhen(false)] out string? errorMessage)
    {
        bool success = CheckImageFilesPathAllExist(files, out string? msg);
        errorMessage = msg;
        return success;
    }

    private async Task CopyContentRecursively(AssetNode node, StorageFolder rootFolder)
    {
        if (node.Type == ImageListNodeType.Folder)
        {
            foreach (AssetNode item in node.Children)
            {
                if (item.Type == ImageListNodeType.Folder)
                {
                    if (item.Children.Count > 0)
                    {
                        StorageFolder folder = await rootFolder.CreateFolderAsync(item.DisplayName, CreationCollisionOption.OpenIfExists);

                        foreach (AssetNode subItem in item.Children)
                        {
                            await CopyContentRecursively(subItem, folder);
                        }
                    }
                }
                else if (item.Type == ImageListNodeType.File && item is FileNode fileNode && File.Exists(fileNode.FilePath))
                {
                    await SaveFileNode(fileNode, rootFolder);
                }
            }
        }
        else if (node.Type == ImageListNodeType.File && node is FileNode fileNode && File.Exists(fileNode.FilePath))
        {
            await SaveFileNode(fileNode, rootFolder);
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debugger.Break();
        }
#endif

        async Task SaveFileNode(FileNode fileNode, StorageFolder rootFolder)
        {
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

    internal async Task<ProjectPackageResourcesHelper> ToProjectBasedResourcesHelperAsync(ProjectPackage projectPackage)
    {
        ProjectPackageResourcesHelper helper = new(projectPackage)
        {
            ConvertWebP = this.ConvertWebP
        };

        if (HasCover)
        {
            await helper.SetCoverAsync(coverFile.Path);
        }

        return helper;
    }

    private static bool CheckImageFilesPathAllExist(IEnumerable<AssetNode> nodes, [NotNullWhen(false)] out string? errorMessage)
    {
        bool isSuccess = true;
        StringBuilder builder = new();

        foreach (AssetNode node in nodes)
        {
            if (node is FolderNode folderNode)
            {
                if (folderNode.Children.Count <= 0)
                {
                    continue;
                }

                bool success = CheckImageFilesPathAllExist(folderNode.Children, out string? msg);

                if (!success)
                {
                    builder.AppendLine(msg);
                    isSuccess = false;
                }
            }
            else if (node is FileNode fileNode)
            {
                if (!fileNode.IsRelativePath)
                {
                    fileNode.EnsurePathExist();

                    if (!fileNode.IsFileExist)
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
}

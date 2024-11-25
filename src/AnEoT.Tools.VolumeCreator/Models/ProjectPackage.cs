using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;

namespace AnEoT.Tools.VolumeCreator.Models;

/// <summary>
/// 表示项目包的类
/// </summary>
public sealed partial class ProjectPackage : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 项目信息文件名。
    /// </summary>
    private const string ProjectInfoFileName = "project.json";
    /// <summary>
    /// 期刊目录文件名。
    /// </summary>
    private const string IndexMarkdownFileName = "volume/index.json";
    /// <summary>
    /// 期刊文章文件名。
    /// </summary>
    private const string ArticlesFileName = "volume/articles.json";
    /// <summary>
    /// 资源文件清单文件名。
    /// </summary>
    private const string AssetsFileName = "assets/manifest.json";
    /// <summary>
    /// 资源文件文件夹名称。
    /// </summary>
    private const string AssetsFolderName = "assets";

    private ZipArchive zipArchive;
    private string path;

    /// <summary>
    /// 通过路径打开或者创建一个 <see cref="ProjectPackage"/>，但不读取其中的信息。
    /// </summary>
    /// <param name="path">目标文件路径。</param>
    /// <param name="isOverride">指示是否覆盖原文件的值。</param>
    private ProjectPackage(string path, bool isOverride = false)
    {
        CreateZipArchive(path, isOverride);
    }

    [MemberNotNull(nameof(zipArchive), nameof(this.path))]
    private void CreateZipArchive(string path, bool isOverride)
    {
        this.path = path;
        FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        if (isOverride)
        {
            stream.SetLength(0);
        }
        else
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        try
        {
            zipArchive = new ZipArchive(stream, ZipArchiveMode.Update);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 项目信息。
    /// </summary>
    public ProjectInfo Info { get; set; } = new ProjectInfo();

    /// <summary>
    /// 文章 Markdown 列表。
    /// </summary>
    public IEnumerable<MarkdownWrapper> Articles { get; set; } = [];

    /// <summary>
    /// 资源列表。
    /// </summary>
    public IEnumerable<AssetNode> Assets { get; set; } = [];

    /// <summary>
    /// 目录页 Markdown。
    /// </summary>
    public MarkdownWrapper? IndexMarkdown { get; set; }

    /// <summary>
    /// 确定项目包内是否包括封面图像。
    /// </summary>
    public bool HasCoverImage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Info.CoverImageName))
            {
                return false;
            }

            ZipArchiveEntry? coverEntry = zipArchive.GetEntry(Info.CoverImageName);
            return coverEntry is not null;
        }
    }

    /// <summary>
    /// 加载指定的项目包。若路径指向的文件不存在或无效，则创建一个新的项目包。
    /// </summary>
    /// <param name="path">项目包文件路径。</param>
    /// <returns>表示项目包的 <see cref="ProjectPackage"/>。</returns>
    public static async Task<ProjectPackage> LoadOrCreateAsync(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return await LoadAsync(path);
            }
        }
        catch (InvalidDataException)
        {
        }

        return Create(path);
    }

    /// <summary>
    /// 通过指定路径创建一个空的 <see cref="ProjectPackage"/>。
    /// </summary>
    /// <remarks>
    /// 如果 <paramref name="path"/> 指向的文件已存在，则这个文件将被覆盖。
    /// </remarks>
    /// <param name="path">目标路径。</param>
    /// <returns>表示项目包的 <see cref="ProjectPackage"/>。</returns>
    public static ProjectPackage Create(string path)
    {
        ProjectPackage package = new(path, true);
        return package;
    }

    /// <summary>
    /// 加载指定的项目包。
    /// </summary>
    /// <param name="path">项目包文件路径。</param>
    /// <returns>表示项目包的 <see cref="ProjectPackage"/>。</returns>
    /// <exception cref="InvalidDataException">项目包的格式无效。</exception>
    public static async Task<ProjectPackage> LoadAsync(string path)
    {
        ProjectPackage? package = null;
        try
        {
            package = new(path);
            await InitializePackageFor<ProjectInfo>(package, ProjectInfoFileName, info => package.Info = info,
                                                    $"无法加载工程文件中的项目信息（{ProjectInfoFileName}）。",
                                                    true,
                                                    $"工程文件中缺少项目信息（{ProjectInfoFileName}）。");
            await InitializePackageFor<MarkdownWrapper>(package, IndexMarkdownFileName,
                                                        index => package.IndexMarkdown = index,
                                                        $"无法加载工程文件中的目录信息（{ProjectInfoFileName}）。");
            await InitializePackageFor<IEnumerable<MarkdownWrapper>>(package, ArticlesFileName,
                                                        articles => package.Articles = articles ?? [],
                                                        $"无法加载工程文件中的文章列表。");
            await InitializePackageFor<IEnumerable<AssetNode>>(package, AssetsFileName,
                                                        nodes => package.Assets = nodes ?? [],
                                                        $"无法加载工程文件中的资源列表。");

            return package;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("工程文件无效，请查阅内部异常以获取更多信息。", ex);
        }
    }

    /// <summary>
    /// 获取封面图像流。
    /// </summary>
    /// <returns>表示封面图像流的 <see cref="Stream"/>，若包内没有封面图像，则返回 <see langword="null"/>。</returns>
    public async Task<Stream?> GetCoverFileAsync()
    {
        if (string.IsNullOrWhiteSpace(Info.CoverImageName))
        {
            return null;
        }

        ZipArchiveEntry? coverEntry = zipArchive.GetEntry(Info.CoverImageName);
        if (coverEntry is null)
        {
            return null;
        }

        using Stream coverStream = coverEntry.Open();
        coverStream.Seek(0, SeekOrigin.Begin);

        MemoryStream result = new();
        await coverStream.CopyToAsync(result);
        result.Seek(0, SeekOrigin.Begin);
        return result;
    }

    /// <summary>
    /// 设置封面图像。
    /// </summary>
    /// <param name="coverImagePath">封面图像的文件路径。</param>
    public async Task SetCoverFileAsync(string coverImagePath)
    {
        using FileStream coverStream = File.OpenRead(coverImagePath);
        coverStream.Seek(0, SeekOrigin.Begin);

        if (!string.IsNullOrWhiteSpace(Info.CoverImageName))
        {
            ZipArchiveEntry? coverEntry = zipArchive.GetEntry(Info.CoverImageName);
            coverEntry?.Delete();
        }
        string coverName = $"cover{Path.GetExtension(coverImagePath)}";
        Info = Info with { CoverImageName = coverName };

        using Stream target = CreateEntryStream(coverName);
        await coverStream.CopyToAsync(target);
    }

    /// <summary>
    /// 移除封面图像。
    /// </summary>
    public void RemoveCoverFile()
    {
        if (!string.IsNullOrWhiteSpace(Info.CoverImageName))
        {
            ZipArchiveEntry? coverEntry = zipArchive.GetEntry(Info.CoverImageName);
            coverEntry?.Delete();
        }
    }

    /// <summary>
    /// 获取项目包中的资源流。
    /// </summary>
    /// <param name="fileNode">用于获取资源流的 <see cref="FileNode"/>。</param>
    /// <returns>表示资源流的 <see cref="Stream"/>，若为 <see langword="null"/> 则表示未找到指定的资源。</returns>
    /// <exception cref="ArgumentException"><paramref name="fileNode"/> 的文件路径不为相对路径。</exception>
    public async Task<Stream?> GetAssetAsync(FileNode fileNode)
    {
        if (!fileNode.IsRelativePath)
        {
            throw new ArgumentException($"只有文件路径为相对路径的 {nameof(FileNode)} 才能用于获取项目包内的资源文件。");
        }

        if (TryGetEntryStream(fileNode.FilePath, out Stream? entryStream))
        {
            using (entryStream)
            {
                MemoryStream memoryStream = new();
                await entryStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 确定项目包内是否包含 <see cref="FileNode"/> 所表示的资源。
    /// </summary>
    /// <param name="fileNode">指定的 <see cref="FileNode"/>。</param>
    /// <returns>指示资源是否存在的值。</returns>
    /// <exception cref="ArgumentException"><paramref name="fileNode"/> 的文件路径不为相对路径。</exception>
    public bool ContainsAsset(FileNode fileNode)
    {
        if (!fileNode.IsRelativePath)
        {
            throw new ArgumentException($"只有文件路径为相对路径的 {nameof(FileNode)} 才能用于获取项目包内的资源文件。");
        }

        ZipArchiveEntry? entry = zipArchive.GetEntry(fileNode.FilePath);
        return entry is not null;
    }

    /// <summary>
    /// 保存对当前 <see cref="ProjectPackage"/> 的更改。
    /// </summary>
    /// <param name="reopen">指示是否重新打开文件的值，一般应当设为 <see langword="true"/>。</param>
    public async Task SaveAsync(bool reopen = true)
    {
        zipArchive.Comment = $"\u6B64\u5DE5\u7A0B\u6587\u4EF6\u7531\u0020\u0041\u006E\u0045\u006F\u0054\u0020\u0056\u006F\u006C\u0075\u006D\u0065\u0020\u0043\u0072\u0065\u0061\u0074\u006F\u0072\u0020\u751F\u6210\uFF0C\u751F\u6210\u4E8E\uFF1A{DateTimeOffset.Now}\u3002{SelectCandidate()}";
        await SavePackageFor(ProjectInfoFileName, Info);
        await SavePackageFor(IndexMarkdownFileName, IndexMarkdown);
        await SavePackageFor(ArticlesFileName, Articles);
        await SavePackageForAssets(Assets);

        if (reopen)
        {
            zipArchive.Dispose();
            CreateZipArchive(path, false);
        }
    }

    private async Task SavePackageForAssets(IEnumerable<AssetNode> nodes)
    {
        await SaveAssets(nodes);
        await SavePackageFor(AssetsFileName, nodes);

        FileNode[] fileNodes = Descendants<FileNode>(nodes).ToArray();
        ZipArchiveEntry[] assetsEntries = zipArchive.Entries.Where(entry => entry.FullName.StartsWith(AssetsFolderName) && entry.FullName != AssetsFileName).ToArray();
        foreach (ZipArchiveEntry assetsEntry in assetsEntries)
        {
            bool shouldDelete = !fileNodes.Any(node => IsNodeMatchesEntry(node, assetsEntry));

            if (shouldDelete)
            {
                assetsEntry.Delete();
            }
        }

        static IEnumerable<T> Descendants<T>(IEnumerable<AssetNode> nodes)
        {
            foreach (AssetNode node in nodes)
            {
                if (node is T target)
                {
                    yield return target;
                }

                foreach (T? subitem in Descendants<T>(node.Children))
                {
                    yield return subitem;
                }
            }
        }

        static bool IsNodeMatchesEntry(FileNode node, ZipArchiveEntry entry)
        {
            string nodePath = node.FilePath.Replace('\\', '/');
            string entryPath = entry.FullName.Replace("\\", "/");

            if (!node.IsRelativePath)
            {
                return true;
            }
            else
            {
                return nodePath.Equals(entryPath, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private async Task SaveAssets(IEnumerable<AssetNode> nodes)
    {
        foreach (AssetNode node in nodes)
        {
            if (node is FolderNode folderNode)
            {
                if (folderNode.Children.Count <= 0)
                {
                    continue;
                }

                await SaveAssets(folderNode.Children);
            }
            else if (node is FileNode fileNode)
            {
                if (!fileNode.IsRelativePath && fileNode.IsFileExist)
                {
                    using FileStream fileStream = File.OpenRead(fileNode.FilePath);
                    fileStream.Seek(0, SeekOrigin.Begin);

                    string fileName = $"{AssetsFolderName}/{GetNodePath(fileNode)}";
                    using Stream zipFileStream = CreateEntryStream(fileName);
                    await fileStream.CopyToAsync(zipFileStream);
                    fileNode.IsRelativePath = true;
                    fileNode.FilePath = fileName;
                }
            }
#if DEBUG
            else
            {
                System.Diagnostics.Debugger.Break();
            }
#endif
        }
    }

    private static string GetNodePath(AssetNode node)
    {
        if (node.Parent is null)
        {
            return node.DisplayName;
        }
        else
        {
            return $"{GetNodePath(node.Parent)}/{node.DisplayName}";
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "已经保留了必需的类型信息")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "已经保留了必需的类型信息")]
    private static async Task InitializePackageFor<T>(ProjectPackage package, string packageFileName, Action<T?> operationForPackage, string parseErrorMessage, bool required = false, string fileNotFoundErrorMessage = "")
    {
        if (package.TryGetEntryStream(packageFileName, out Stream? entryStream))
        {
            using (entryStream)
            {
                try
                {
                    T? value = await JsonSerializer.DeserializeAsync<T>(entryStream, CommonValues.DefaultJsonSerializerOption);
                    operationForPackage(value);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException(parseErrorMessage, ex);
                }
            }
        }
        else if (required)
        {
            throw new InvalidDataException(fileNotFoundErrorMessage);
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "已经保留了必需的类型信息")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "已经保留了必需的类型信息")]
    private async Task SavePackageFor<T>(string entryName, T value)
    {
        if (value is null || value.Equals(default))
        {
            return;
        }

        using Stream entryStream = CreateEntryStream(entryName);
        await JsonSerializer.SerializeAsync(entryStream, value, CommonValues.DefaultJsonSerializerOption);
    }

    private bool TryGetEntryStream(string fileRelativePath, [NotNullWhen(true)] out Stream? entryStream)
    {
        ZipArchiveEntry? entry = zipArchive.GetEntry(fileRelativePath);
        if (entry is null)
        {
            entryStream = null;
            return false;
        }
        else
        {
            entryStream = entry.Open();
            entryStream.Seek(0, SeekOrigin.Begin);
            return true;
        }
    }

    private Stream CreateEntryStream(string fileRelativePath)
    {
        ZipArchiveEntry entry = GetOrCreateEntry(fileRelativePath);
        Stream stream = entry.Open();

        stream.SetLength(0);

        return stream;
    }

    private ZipArchiveEntry GetOrCreateEntry(string fileRelativePath)
    {
        ZipArchiveEntry entry = zipArchive.GetEntry(fileRelativePath) ?? zipArchive.CreateEntry(fileRelativePath);
        return entry;
    }

    public async void Dispose()
    {
        await SaveAsync(false);
        zipArchive.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await SaveAsync(false);
        zipArchive.Dispose();
    }

    private static readonly string[] candidates =
    [
        "\u300C\u0050\u0065\u0072\u0020\u0041\u0073\u0070\u0065\u0072\u0061\u0020\u0041\u0064\u0020\u0041\u0073\u0074\u0072\u0061\u0020\u007C\u0020\u5FAA\u6B64\u82E6\u65C5\uFF0C\u76F4\u62B5\u7FA4\u661F\u300D",
        "\u300C\u83B2\u6B65\u7CBC\u7CBC\u4E00\u70B9\u6C34\u0020\u5706\u6708\u4E0E\u706F\u4E24\u76F8\u660E\u300D",
        "\u300C\u8BA9\u6211\u4EEC\u4E00\u8D77\u5EFA\u8BBE\u6587\u5B57\u4E8C\u521B\u300D",
        "\u300C\u6765\u81EA\u0020\u0042\u0061\u006B\u0061\u0036\u0033\u0032\u0020\u7684\u95EE\u5019\u300D",
        "\u300C\u0043\u006F\u006E\u0073\u006F\u006C\u0065\u002E\u0057\u0072\u0069\u0074\u0065\u004C\u0069\u006E\u0065\u0028\u0022\u0048\u0065\u006C\u006C\u006F\u0020\u0066\u0072\u006F\u006D\u0020\u0042\u0061\u006B\u0061\u0036\u0033\u0032\u0020\u0062\u0079\u0020\u0043\u0023\u0022\u0029\u300D",
        "\u300C\u56DE\u5F52\u7EBF\uFF01\u54E6\uFF0C\u56DE\u5F52\u7EBF\uFF01\u300D",
        "\u300C\u6211\u4EEC\u65E2\u8981\u505A\u5B5C\u5B5C\u4E0D\u5026\u7684\u7A0B\u5E8F\u5458\uFF0C\u4E5F\u8981\u505A\u7B14\u8015\u4E0D\u8F8D\u7684\u5F00\u53D1\u8005\u3002\u2014\u2014\u0042\u0061\u006B\u0061\u0036\u0033\u0032\u300D",
        "\u300C\u5F80\u6614\u662F\u4E2A\u6F2B\u957F\u7684\u591C\u0020\u6211\u672C\u5C06\u6E29\u9A6F\u7684\u6C89\u6EBA\u5176\u4E2D\u0020\u4F46\u90A3\u53E4\u8001\u661F\u5C18\u4E00\u95EA\u800C\u8FC7\u7684\u5149\u8292\u0020\u5728\u6211\u7684\u7075\u9B42\u70B9\u71C3\u4E86\u53DB\u9006\u4E4B\u706B\u300D",
        "\u300C\u6545\u4E61\u4F9D\u4E8E\u4EBA\u751F\uFF0C\u5374\u65E0\u9700\u5BBF\u4E3B\uFF1B\u4E61\u6101\u5F25\u4E8E\u65E0\u5F62\uFF0C\u5374\u81EA\u6709\u5B9E\u4F53\u300D",
    ];

    private static string SelectCandidate() => candidates[Random.Shared.Next(candidates.Length)];
}

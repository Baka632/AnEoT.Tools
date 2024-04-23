using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Logging;
using AnEoT.Tools.MarkdownChecker.Models;
using System.Globalization;
using Markdig.Extensions.Yaml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace AnEoT.Tools.MarkdownChecker.Checkers;

partial class FileChecker
{
    private static readonly IReadOnlyDictionary<string, string> WrongCorrectOperatorNamePairs = new Dictionary<string, string>()
    {
        ["缪尔赛斯"] = "缪尔赛思",
        ["克洛斯"] = "克洛丝",
        ["凯尔西"] = "凯尔希",
        ["塞雷亚"] = "塞雷娅",
        ["饲夜"] = "伺夜",
        ["魁影"] = "傀影",
        ["桑椹"] = "桑葚",
        ["赫墨"] = "赫默",
        ["特雷西娅"] = "特蕾西娅",
        ["罗得岛"] = "罗德岛",
        ["广英与荣耀"] = "广英和荣耀",
    };

    public static async Task<CheckResult> CheckMarkdown(string filePath, string? rootPath)
    {
        CheckResult checkResult = new();
        string? parentFolderPath = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileName(filePath);

        string markdown = await File.ReadAllTextAsync(filePath);
        MarkdownDocument document = Markdown.Parse(markdown, MarkdownPipeline);

        int imageLinkCount = 0;
        foreach (MarkdownObject obj in document.Descendants())
        {
            switch (obj)
            {
                case LinkInline link when string.IsNullOrWhiteSpace(link.Url) != true:
                    checkResult += await CheckLinkInline(link, filePath, rootPath);

                    if (link.IsImage)
                    {
                        imageLinkCount++;
                    }
                    break;
                case YamlFrontMatterBlock yamlFrontMatter:
                    checkResult += CheckYamlFrontMatter(markdown, yamlFrontMatter, filePath, parentFolderPath);
                    break;
            }
        }

        if (fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase) && parentFolderPath is not null)
        {
            DirectoryInfo volumeFolder = new(parentFolderPath);
            if (volumeFolder.Exists && DateOnly.TryParse(volumeFolder.Name, CultureInfo.InvariantCulture, out _))
            {
                // 在期刊文件夹中
                IEnumerable<FileInfo> files = volumeFolder.EnumerateFiles("*.md")
                                                      .Where(file => file.Name != fileName);
                IEnumerable<LinkInline> links = document.Descendants<LinkInline>()
                                                        .Where(link => link.IsImage != true);

                foreach (FileInfo file in files)
                {
                    bool isArticleInContents = links.Where(link => link.Url is not null)
                                          .Any(link => link.Url!.Replace(".html", ".md").Equals(file.Name, StringComparison.OrdinalIgnoreCase));
                    if (isArticleInContents != true)
                    {
                        LogNotAllFilesInContents(Logger, filePath, file.Name);
                        checkResult.ErrorCount++;
                    }
                }
            }
        }

        return checkResult;
    }

    private static CheckResult CheckYamlFrontMatter(string markdown, YamlFrontMatterBlock yamlFrontMatter, string filePath, string? parentFolderPath)
    {
        CheckResult checkResult = new();

        string yaml = markdown.Substring(yamlFrontMatter.Span.Start, yamlFrontMatter.Span.Length);
        ArticleInfo articleInfo = GetArticleInfo(yaml);

        if (parentFolderPath is not null && DateTimeOffset.TryParse(articleInfo.Date, out DateTimeOffset publishDate))
        {
            DirectoryInfo volumeFolder = new(parentFolderPath);
            if (volumeFolder.Exists && DateOnly.TryParse(volumeFolder.Name, CultureInfo.InvariantCulture, out DateOnly volumePublishDate))
            {
                if (publishDate.Year != volumePublishDate.Year || publishDate.Month != volumePublishDate.Month)
                {
                    LogArticleDateAndVolumeDateNotMatch(Logger, filePath, publishDate.ToString("yyyy-MM"), volumePublishDate.ToString("yyyy-MM"));
                    checkResult.WarningCount++;
                }
            }
        }

        return checkResult;

        static ArticleInfo GetArticleInfo(string yaml)
        {
            StringReader input = new(yaml);
            Parser yamlParser = new(input);
            yamlParser.Consume<StreamStart>();
            yamlParser.Consume<DocumentStart>();

            IDeserializer yamlDes = new StaticDeserializerBuilder(new ArticleInfoStaticContext())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            ArticleInfo info = yamlDes.Deserialize<ArticleInfo>(yamlParser);
            yamlParser.Consume<DocumentEnd>();
            return info;
        }
    }

    private static async Task<CheckResult> CheckLinkInline(LinkInline link, string path, string? rootPath)
    {
        CheckResult checkResult = new();
        string fileName = Path.GetFileName(path);
        string urlString = link.Url!;
        int line = link.Line + 1;

        Uri uri = new(urlString, UriKind.RelativeOrAbsolute);

        if (uri.IsAbsoluteUri)
        {
            if (uri.Scheme is "http" or "https")
            {
                try
                {
                    using HttpResponseMessage httpMessage = await HttpClient.GetAsync(uri);
                    if (httpMessage.IsSuccessStatusCode != true)
                    {
                        LogCannotAccessLink(Logger, path, line, urlString);
                        checkResult.ErrorCount++;
                    }
                }
                catch (HttpRequestException)
                {
                    LogCannotAccessLink(Logger, path, line, urlString);
                    checkResult.ErrorCount++;
                }
            }
            else if (uri.IsFile)
            {
                if (Path.Exists(uri.AbsolutePath) != true)
                {
                    LogCannotFindFile(Logger, path, line, urlString);
                    checkResult.ErrorCount++;
                }
            }
        }
        else
        {
                string targetPath;
                if (urlString.StartsWith('/'))
                {
                    if (rootPath is not null)
                    {
                        if (rootPath.EndsWith('/') || rootPath.EndsWith('\\'))
                        {
                            rootPath = rootPath[..^1];
                        }
                        targetPath = $"{rootPath}{urlString}";
                    }
                    else
                    {
                        string dirName = Path.GetDirectoryName(path) ?? string.Empty;
                        if (dirName.EndsWith('/') || dirName.EndsWith('\\'))
                        {
                            dirName = dirName[..^1];
                        }
                        targetPath = $"{dirName}{urlString}";
                    }
                }
                else
                {
                    string dirName = Path.GetDirectoryName(path) ?? string.Empty;
                    targetPath = Path.Combine(dirName, urlString);
                }

                if (Path.Exists(targetPath) != true && fileName != "description.md" && fileName != "subscription.md")
                {
                    string fileExt = Path.GetExtension(targetPath);
                    if (!fileExt.Equals(".html", StringComparison.OrdinalIgnoreCase) || !Path.Exists(targetPath.Replace(".html", ".md")))
                    {
                        LogCannotFindFile(Logger, path, line, urlString, targetPath);
                        checkResult.ErrorCount++;
                    }
                }
        }

        return checkResult;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "{FilePath}(第 {TargetLine} 行): 无法访问链接：{Link}")]
    public static partial void LogCannotAccessLink(ILogger logger, string filePath, int targetLine, string link);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "{FilePath}(第 {TargetLine} 行): 找不到文件：{Link}。")]
    public static partial void LogCannotFindFile(ILogger logger, string filePath, int targetLine, string link);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "{FilePath}(第 {TargetLine} 行): 找不到文件：{Link}。已尝试在以下路径中寻找：{TriedFilePath}")]
    public static partial void LogCannotFindFile(ILogger logger, string filePath, int targetLine, string link, string triedFilePath);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "{FilePath}：{FileNotIncluded} 位于当前期刊的文件夹中，但未包含在此期的目录页。")]
    public static partial void LogNotAllFilesInContents(ILogger logger, string filePath, string fileNotIncluded);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "{FilePath}：此文章的发布日期（{articleReleaseDate}）与当前期刊的发布日期（{volumeReleaseDate}）不符。")]
    public static partial void LogArticleDateAndVolumeDateNotMatch(ILogger logger, string filePath, string articleReleaseDate, string volumeReleaseDate);
}

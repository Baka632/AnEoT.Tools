using Markdig;
using Microsoft.Extensions.Logging;
using AnEoT.Tools.MarkdownChecker.Models;

namespace AnEoT.Tools.MarkdownChecker.Checkers;

public static partial class FileChecker
{
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Default)
        .UseAdvancedExtensions()
        .UseListExtras()
        .UseYamlFrontMatter()
        .Build();
    private static readonly ILogger Logger;
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(20),
    };

    static FileChecker()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Markdown 分析器");
        Logger = logger;
    }

    public static async Task<CheckResult> CheckSingleFile(string filePath, string? rootPath)
    {
        CheckResult result = await CheckMarkdown(filePath, rootPath);
        return result;
    }
}

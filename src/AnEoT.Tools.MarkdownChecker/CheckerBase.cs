using Markdig;
using Microsoft.Extensions.Logging;

namespace AnEoT.Tools.MarkdownChecker;

internal partial class CheckerBase
{
    protected static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Default)
        .UseAdvancedExtensions()
        .UseListExtras()
        .UseYamlFrontMatter()
        .Build();
    protected static readonly ILogger Logger;
    protected static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(20),
    };

    static CheckerBase()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger("Markdown 分析器");
        Logger = logger;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "{FilePath}(第 {TargetLine} 行): 无法访问链接：{Link}")]
    public static partial void LogCannotAccessLink(ILogger logger, string filePath, int targetLine, string link);
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "{FilePath}(第 {TargetLine} 行): 找不到文件：{Link}。已尝试在以下路径中寻找：{TriedFilePath}")]
    public static partial void LogCannotFindFile(ILogger logger, string filePath, int targetLine, string link, string triedFilePath);
}

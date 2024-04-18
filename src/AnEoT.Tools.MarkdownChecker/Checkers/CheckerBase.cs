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
}

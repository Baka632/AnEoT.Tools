using Markdig;

namespace AnEoT.Tools.VolumeCreator;

public class CommonValues
{
    public static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseListExtras()
            .UseEmojiAndSmiley(true)
            .UseYamlFrontMatter()
            .Build();
}

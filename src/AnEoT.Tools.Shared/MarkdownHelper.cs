using System.Text;
using AnEoT.Tools.Shared.Models;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace AnEoT.Tools.Shared;

public static class MarkdownHelper
{
    public static string InsertFrontMatter(string markdown, FrontMatter frontMatter)
    {
        StringBuilder stringBuilder = new(markdown);
        string yamlHeader = GetYamlFrontMatterString(frontMatter);

        stringBuilder.Insert(0, yamlHeader);
        return stringBuilder.ToString();
    }
    
    public static StringBuilder InsertFrontMatter(StringBuilder markdownStringBuilder, FrontMatter frontMatter)
    {
        string yamlHeader = GetYamlFrontMatterString(frontMatter);

        markdownStringBuilder.Insert(0, yamlHeader);
        return markdownStringBuilder;
    }

    private static string GetYamlFrontMatterString(FrontMatter frontMatter)
    {
        ISerializer serializer = new SerializerBuilder()
                        .WithIndentedSequences()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
        string yamlString = serializer.Serialize(frontMatter).Trim();
        string yamlHeader = $"""
                ---
                {yamlString}
                ---


                """;
        return yamlHeader;
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Text;
using AnEoT.Tools.Shared.Models;

namespace AnEoT.Tools.Shared;

public static class MarkdownHelper
{
    [RequiresDynamicCode("此方法调用了不支持 IL 裁剪的 YamlDotNet.Serialization.SerializerBuilder.SerializerBuilder()")]
    public static string InsertFrontMatter(string markdown, FrontMatter frontMatter)
    {
        StringBuilder stringBuilder = new(markdown);
        string yamlHeader = GetYamlHeaderString(frontMatter);

        stringBuilder.Insert(0, yamlHeader);
        return stringBuilder.ToString();
    }

    [RequiresDynamicCode("此方法调用了不支持 IL 裁剪的 YamlDotNet.Serialization.SerializerBuilder.SerializerBuilder()")]
    public static StringBuilder InsertFrontMatter(StringBuilder markdownStringBuilder, FrontMatter frontMatter)
    {
        string yamlHeader = GetYamlHeaderString(frontMatter);

        markdownStringBuilder.Insert(0, yamlHeader);
        return markdownStringBuilder;
    }

    [RequiresDynamicCode("此方法调用了不支持 IL 裁剪的 YamlDotNet.Serialization.SerializerBuilder.SerializerBuilder()")]
    public static string GetYamlHeaderString(FrontMatter frontMatter)
    {
        string yamlString = frontMatter.GetYamlString();

        StringBuilder stringBuilder = new(200);
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine(yamlString);
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }
}

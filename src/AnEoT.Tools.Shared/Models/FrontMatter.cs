using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AnEoT.Tools.Shared.StaticContexts;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Diagnostics.CodeAnalysis;

namespace AnEoT.Tools.Shared.Models;

public readonly record struct FrontMatter(
    [property: YamlMember(Order = 2)] string Title,
    [property: YamlMember(Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? ShortTitle,
    [property: YamlMember(Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string Icon,
    [property: YamlMember(Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? Author,
    [property: YamlMember(Order = 5, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? Date,
    [property: YamlMember(Order = 6, DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)] IEnumerable<string>? Category,
    [property: YamlMember(Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)] IEnumerable<string>? Tag,
    [property: YamlMember(Order = 9)] int Order,
    [property: YamlMember(Order = 8, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? Description)
{
    [RequiresDynamicCode("此方法调用了不支持 IL 裁剪的 YamlDotNet.Serialization.SerializerBuilder.SerializerBuilder()")]
    public string GetYamlString()
    {
        ISerializer serializer = new SerializerBuilder()
                        .WithIndentedSequences()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
        //ISerializer serializer = new StaticSerializerBuilder(new FrontMatterStaticContext())
        //                .WithIndentedSequences()
        //                .WithNamingConvention(CamelCaseNamingConvention.Instance)
        //                .Build();
        string yamlString = serializer.Serialize(this).Trim();
        return yamlString;
    }

    [RequiresDynamicCode("此方法调用了不支持 IL 裁剪的 YamlDotNet.Serialization.DeserializerBuilder.DeserializerBuilder()")]
    public static bool TryParse(string yaml, out FrontMatter result)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            result = default;
            return false;
        }

        try
        {
            StringReader input = new(yaml);
            Parser yamlParser = new(input);
            yamlParser.Consume<StreamStart>();
            yamlParser.Consume<DocumentStart>();

            IDeserializer yamlDes = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            //IDeserializer yamlDes = new StaticDeserializerBuilder(new FrontMatterStaticContext())
            //    .WithNamingConvention(CamelCaseNamingConvention.Instance)
            //    .Build();
            result = yamlDes.Deserialize<FrontMatter>(yamlParser);
            yamlParser.Consume<DocumentEnd>();
        }
        catch
        {
            result = default;
            return false;
        }

        return true;
    }
}

internal class ClassFrontMatter
{
    [YamlMember(Order = 2)]
    public string Title { get; set; } = string.Empty;

    [YamlMember(Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? ShortTitle { get; set; }

    [YamlMember(Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string Icon { get; set; } = string.Empty;

    [YamlMember(Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Author { get; set; }

    [YamlMember(Order = 5, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Date { get; set; }

    [YamlMember(Order = 6, DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)]
    public IEnumerable<string>? Category { get; set; }

    [YamlMember(Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)]
    public IEnumerable<string>? Tag { get; set; }

    [YamlMember(Order = 9)]
    public int Order { get; set; }

    [YamlMember(Order = 8, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    public string? Description { get; set; }

    public string GetYamlString()
    {
        ISerializer serializer = new StaticSerializerBuilder(new FrontMatterStaticContext())
                        .WithIndentedSequences()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
        string yamlString = serializer.Serialize(this).Trim();
        return yamlString;
    }

    public static bool TryParse(string yaml, out FrontMatter? result)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            result = default;
            return false;
        }

        try
        {
            StringReader input = new(yaml);
            Parser yamlParser = new(input);
            yamlParser.Consume<StreamStart>();
            yamlParser.Consume<DocumentStart>();

            IDeserializer yamlDes = new StaticDeserializerBuilder(new FrontMatterStaticContext())
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            result = yamlDes.Deserialize<FrontMatter>(yamlParser);
            yamlParser.Consume<DocumentEnd>();
        }
        catch
        {
            result = default;
            return false;
        }

        return true;
    }
}
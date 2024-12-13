using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AnEoT.Tools.Shared.StaticContexts;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Collections;
using YamlDotNet.Serialization.ObjectFactories;

namespace AnEoT.Tools.Shared.Models;

public readonly record struct FrontMatter(
    [property: YamlMember(Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string Icon,
    [property: YamlMember(Order = 2, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults), DefaultValue(true)] bool Article,
    [property: YamlMember(Order = 3)] string Title,
    [property: YamlMember(Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? ShortTitle,
    [property: YamlMember(Order = 5, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? Author,
    [property: YamlMember(Order = 6, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? Date,
    [property: YamlMember(Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)] IEnumerable<string>? Category,
    [property: YamlMember(Order = 8, DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)] IEnumerable<string>? Tag,
    [property: YamlMember(Order = 9, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? Description,
    [property: YamlMember(Order = 10)] int Order)
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
                .WithObjectFactory(new FrontMatterFactory())
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

file sealed class FrontMatterFactory : IObjectFactory
{
    private readonly DefaultObjectFactory defaultFactory = new();

    public object Create(Type type)
    {
        if (type == typeof(FrontMatter))
        {
            return new FrontMatter() { Article = true };
        }
        else
        {
            return defaultFactory.Create(type);
        }
    }

    public object? CreatePrimitive(Type type)
    {
        return defaultFactory.CreatePrimitive(type);
    }

    public void ExecuteOnDeserialized(object value)
    {
        defaultFactory.ExecuteOnDeserialized(value);
    }

    public void ExecuteOnDeserializing(object value)
    {
        defaultFactory.ExecuteOnDeserializing(value);
    }

    public void ExecuteOnSerialized(object value)
    {
        defaultFactory.ExecuteOnSerialized(value);
    }

    public void ExecuteOnSerializing(object value)
    {
        defaultFactory.ExecuteOnSerializing(value);
    }

    public bool GetDictionary(IObjectDescriptor descriptor, out IDictionary? dictionary, out Type[]? genericArguments)
    {
        return defaultFactory.GetDictionary(descriptor, out dictionary, out genericArguments);
    }

    public Type GetValueType(Type type)
    {
        return defaultFactory.GetValueType(type);
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
        ISerializer serializer = new StaticSerializerBuilder(new ClassFrontMatterStaticContext())
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

            IDeserializer yamlDes = new StaticDeserializerBuilder(new ClassFrontMatterStaticContext())
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
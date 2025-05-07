using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using AnEoT.Tools.VolumeCreator.Models;
using Markdig;

namespace AnEoT.Tools.VolumeCreator;

public partial class CommonValues
{
    public static bool IsProjectSaved { get; set; } = true;

    public static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseListExtras()
            .UseEmojiAndSmiley(true)
            .UseYamlFrontMatter()
            .Build();

    public static readonly JsonSerializerOptions DefaultJsonSerializerOption = CreateJsonSerializerOption(null);

    public static JsonSerializerOptions CreateJsonSerializerOption(IJsonTypeInfoResolver? typeInfoResolver)
    {
        return new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter<PredefinedCategory>(JsonNamingPolicy.CamelCase),
                new JsonStringEnumConverter<MarkdownWrapperType>(JsonNamingPolicy.CamelCase),
                new JsonStringEnumConverter<AssetNodeType>(JsonNamingPolicy.CamelCase),
            },
            TypeInfoResolver = typeInfoResolver,
            ReferenceHandler = ReferenceHandler.Preserve,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };
    }

    [GeneratedRegex(@"^([a-z0-9-]+\.)*lofter\.com\.*$")]
    public static partial Regex GetLofterDomainVerifyRegex();

    public static List<FileNode> DescendantsFileNode(AssetNode node)
    {
        List<FileNode> fileNodes = new(10);

        foreach (AssetNode item in node.Children)
        {
            if (item is FileNode fileNode)
            {
                fileNodes.Add(fileNode);
            }
            else if (item is FolderNode folderNode)
            {
                fileNodes.AddRange(DescendantsFileNode(folderNode));
            }
        }

        return fileNodes;
    }

    public static bool ContainsFileNode(AssetNode node)
    {
        if (node is FileNode)
        {
            return true;
        }

        bool hasFileNode = false;

        foreach (AssetNode item in node.Children)
        {
            if (item is FileNode)
            {
                hasFileNode = true;
            }
            else if (item is FolderNode)
            {
                hasFileNode = ContainsFileNode(item);
            }

            if (hasFileNode)
            {
                break;
            }
        }

        return hasFileNode;
    }
}

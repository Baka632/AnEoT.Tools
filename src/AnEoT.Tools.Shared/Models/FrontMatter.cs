using YamlDotNet.Serialization;

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
    [property: YamlMember(Order = 8, DefaultValuesHandling = DefaultValuesHandling.OmitNull)] string? Description);

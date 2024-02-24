using YamlDotNet.Serialization;

namespace AnEoT.Tools.WordToMarkdown.Models;

/// <summary>
/// 表示文章信息的结构
/// </summary>
public readonly record struct ArticleInfo
{
    [YamlMember(Order = 2)]
    /// <summary>
    /// 文章标题
    /// </summary>
    public string Title { get; init; }

    [YamlMember(Order = 3, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    /// <summary>
    /// 文章短标题
    /// </summary>
    public string? ShortTitle { get; init; }

    [YamlMember(Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    /// <summary>
    /// 文章类型图标
    /// </summary>
    public string Icon { get; init; }

    [YamlMember(Order = 4, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    /// <summary>
    /// 文章作者
    /// </summary>
    public string? Author { get; init; }

    [YamlMember(Order = 5, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    /// <summary>
    /// 文章创建日期的字符串
    /// </summary>
    public string? Date { get; init; }

    [YamlMember(Order = 6,DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)]
    /// <summary>
    /// 文章类别
    /// </summary>
    public IEnumerable<string>? Category { get; init; }

    [YamlMember(Order = 7, DefaultValuesHandling = DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)]
    /// <summary>
    /// 文章标签
    /// </summary>
    public IEnumerable<string>? Tag { get; init; }

    [YamlMember(Order = 9)]
    /// <summary>
    /// 文章在本期期刊的顺序
    /// </summary>
    public int Order { get; init; }

    [YamlMember(Order = 8, DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
    /// <summary>
    /// 页面描述
    /// </summary>
    public string? Description { get; init; }
}

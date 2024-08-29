using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AnEoT.Tools.VolumeCreator.Models;

public sealed class MarkdownWrapper(string displayName, string markdown, MarkdownWrapperType type = default, string outputTitle = "", PredefinedCategory? categoryInIndexPage = null) : INotifyPropertyChanged
{
    [JsonIgnore]
    public static readonly List<MarkdownWrapperType> AvailableTypes =
    [
        MarkdownWrapperType.Intro,
        MarkdownWrapperType.Article,
        MarkdownWrapperType.Interview,
        MarkdownWrapperType.Comic,
        MarkdownWrapperType.OperatorSecret,
        MarkdownWrapperType.Paintings,
        MarkdownWrapperType.Others,
    ];

    public string Markdown { get; set; } = markdown;

    public MarkdownWrapperType Type { get; set; } = type;

    public PredefinedCategory? CategoryInIndexPage { get; set; } = categoryInIndexPage;

    [JsonIgnore]
    public int TypeIndex
    {
        get => AvailableTypes.IndexOf(Type);
        set
        {
            Type = value + 1 > AvailableTypes.Count || value < 0 ? default : AvailableTypes[value];
            CommonValues.IsProjectSaved = false;
            OnPropertiesChanged();
        }
    }

    public string OutputTitle
    {
        get => outputTitle;
        set
        {
            outputTitle = value;
            CommonValues.IsProjectSaved = false;
            OnPropertiesChanged();
        }
    }

    public string DisplayName
    {
        get => displayName;
        set
        {
            displayName = value;
            CommonValues.IsProjectSaved = false;
            OnPropertiesChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 通知运行时属性已经发生更改
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// 表示 <see cref="MarkdownWrapper"/> 类型的枚举
/// </summary>
public enum MarkdownWrapperType
{
    /// <summary>
    /// 卷首语
    /// </summary>
    Intro,
    /// <summary>
    /// 文章
    /// </summary>
    Article,
    /// <summary>
    /// 专访
    /// </summary>
    Interview,
    /// <summary>
    /// 漫画
    /// </summary>
    Comic,
    /// <summary>
    /// 干员秘闻
    /// </summary>
    OperatorSecret,
    /// <summary>
    /// 画中秘境
    /// </summary>
    Paintings,
    /// <summary>
    /// 其他
    /// </summary>
    Others,
}
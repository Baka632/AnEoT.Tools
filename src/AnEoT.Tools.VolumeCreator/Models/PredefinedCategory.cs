namespace AnEoT.Tools.VolumeCreator.Models;

public enum PredefinedCategory
{
    /// <summary>
    /// 泰拉观察
    /// </summary>
    News,
    /// <summary>
    /// 见字如面·罗德岛日志
    /// </summary>
    RhodesIslandJournal,
    /// <summary>
    /// 见字如面·此地之外
    /// </summary>
    BeyondHere,
    /// <summary>
    /// 见字如面·午间逸话
    /// </summary>
    StoriesOfAfternoon,
    /// <summary>
    /// 见字如面·前航远歌
    /// </summary>
    QianHangYuanGe,
    /// <summary>
    /// 画中秘境
    /// </summary>
    Paintings,
    /// <summary>
    /// 莱茵实验室
    /// </summary>
    RhineLaboratory,
    /// <summary>
    /// 情报处理室
    /// </summary>
    Intelligence,
}

public static class PredefinedCategoryExtensions
{
    public static string AsCategoryString(this PredefinedCategory category, bool appendCategoryNameForSubcategory = false)
    {
        return category switch
        {
            PredefinedCategory.News => "泰拉观察",
            PredefinedCategory.RhodesIslandJournal => appendCategoryNameForSubcategory ? "见字如面 · 罗德岛日志" : "罗德岛日志",
            PredefinedCategory.BeyondHere => appendCategoryNameForSubcategory ? "见字如面 · 此地之外" : "此地之外",
            PredefinedCategory.StoriesOfAfternoon => appendCategoryNameForSubcategory ? "见字如面 · 午间逸话" : "午间逸话",
            PredefinedCategory.QianHangYuanGe => appendCategoryNameForSubcategory ? "见字如面 · 前航远歌" : "前航远歌",
            PredefinedCategory.Paintings => "画中秘境",
            PredefinedCategory.RhineLaboratory => "莱茵实验室",
            PredefinedCategory.Intelligence => "情报处理室",
            _ => throw new NotImplementedException(),
        };
    }
}

public record struct PredefinedCategoryWrapper(PredefinedCategory? PredefinedCategory)
{
    public static implicit operator PredefinedCategory?(PredefinedCategoryWrapper wrapper)
    {
        return wrapper.PredefinedCategory;
    }

    public static implicit operator PredefinedCategoryWrapper(PredefinedCategory? predefinedCategory)
    {
        return new PredefinedCategoryWrapper(predefinedCategory);
    }
}
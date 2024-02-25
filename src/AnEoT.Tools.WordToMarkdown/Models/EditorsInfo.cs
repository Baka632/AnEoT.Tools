namespace AnEoT.Tools.WordToMarkdown.Models;

/// <summary>
/// 表示文章相关编辑者信息的类
/// </summary>
/// <param name="Editor">责任编辑名称</param>
/// <param name="WebsiteLayoutDesigner">网页排版者名称</param>
/// <param name="Illustrator">文章中插图画师名称</param>
public readonly record struct EditorsInfo(string? Editor, string? WebsiteLayoutDesigner, string? Illustrator)
{
    public override string ToString()
    {
        List<string> editorsInfoPart = new(3);

        if (string.IsNullOrWhiteSpace(Editor) != true)
        {
            editorsInfoPart.Add($"责任编辑：{Editor}");
        }

        if (string.IsNullOrWhiteSpace(WebsiteLayoutDesigner) != true)
        {
            editorsInfoPart.Add($"网页排版：{WebsiteLayoutDesigner}");
        }

        if (string.IsNullOrWhiteSpace(Illustrator) != true)
        {
            editorsInfoPart.Add($"绘图：{Illustrator}");
        }

        if (editorsInfoPart.Count == 0)
        {
            return string.Empty;
        }
        else
        {
            string editorInfos = string.Join('；', editorsInfoPart);
            return $"（{editorInfos}）";
        }
    }
}

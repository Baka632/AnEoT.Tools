namespace AnEoT.Tools.Shared.Models;

/// <summary>
/// 表示文章相关编辑者信息的结构
/// </summary>
/// <param name="Editor">责任编辑名称</param>
/// <param name="WebsiteLayoutDesigner">网页排版者名称</param>
/// <param name="Illustrator">文章中插图画师名称</param>
public readonly record struct EditorsInfo(string? Editor, string? WebsiteLayoutDesigner, string? Illustrator, IEnumerable<(string, string)>? AdditionalParts = null)
{
    public override string ToString()
    {
        List<string> editorsInfoPart = new(4);

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

        if (AdditionalParts is not null)
        {
            foreach ((string, string) item in AdditionalParts)
            {
                (string part1, string part2) = item;

                if (!string.IsNullOrWhiteSpace(part1) && !string.IsNullOrWhiteSpace(part2))
                {
                    editorsInfoPart.Add($"{part1}：{part2}");
                }
            }
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

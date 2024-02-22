// TODO: 这里的代码一团糟，以后一定要重构

using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AnEoT.Tools.WordToMarkdown.Services;

public static class WordToMarkdownService
{
    public static string GetMarkdown(WordprocessingDocument document, bool leaveOpen = true)
    {
        StringBuilder stringBuilder = new();

        MainDocumentPart? mainDocumentPart = document.MainDocumentPart;

        if (mainDocumentPart is not null)
        {
            foreach (OpenXmlElement element in mainDocumentPart.Document.Body ?? [])
            {
                if (element is Paragraph paragraph)
                {
                    ParagraphProperties? paraProp = paragraph.ParagraphProperties;

                    #region 读取段落样式
                    Style? paraStyle = null;
                    if (string.IsNullOrWhiteSpace(paraProp?.ParagraphStyleId?.Val?.Value) != true && mainDocumentPart?.StyleDefinitionsPart?.Styles is not null)
                    {
                        paraStyle = (Style?)mainDocumentPart.StyleDefinitionsPart.Styles.FirstOrDefault(element =>
                        {
                            return element is Style style && style.StyleId == paraProp.ParagraphStyleId.Val.Value;
                        });
                    }
                    #endregion

                    #region 写 Markdown 相关的东西

                    #region Markdown 标题
                    if (paraProp?.OutlineLevel?.Val != null)
                    {
                        WriteMarkdownHeaderByOutlineLevel(stringBuilder, paraProp.OutlineLevel);
                    }
                    else if (paraStyle is not null && paraStyle?.StyleParagraphProperties?.OutlineLevel is not null)
                    {
                        WriteMarkdownHeaderByOutlineLevel(stringBuilder, paraStyle.StyleParagraphProperties.OutlineLevel);
                    }
                    #endregion

                    #region 加粗 + 倾斜 + 删除线 + 下划线
                    bool isBold = false;
                    bool isStrike = false;
                    bool isItalic = false;
                    bool isUnderline = false;
                    if (paraProp?.ParagraphMarkRunProperties is not null)
                    {
                        DetermineRunPropertiesByChildElement(paraProp.ParagraphMarkRunProperties.ChildElements,
                                                             ref isBold,
                                                             ref isStrike,
                                                             ref isItalic,
                                                             ref isUnderline);
                    }
                    else if (paraStyle is not null && paraStyle?.StyleRunProperties is not null)
                    {
                        DetermineRunPropertiesByChildElement(paraStyle.StyleRunProperties.ChildElements,
                                                             ref isBold,
                                                             ref isStrike,
                                                             ref isItalic,
                                                             ref isUnderline);
                    }
                    #endregion

                    #endregion

                    #region 写文本

                    if (isUnderline)
                    {
                        stringBuilder.Append("<u>");
                    }

                    if (isStrike)
                    {
                        stringBuilder.Append("~~");
                    }

                    if (isItalic)
                    {
                        stringBuilder.Append('*');
                    }

                    if (isBold)
                    {
                        stringBuilder.Append("**");
                    }

                    foreach (Run run in paragraph.Descendants<Run>())
                    {
                        if (run is not null)
                        {
                            string? colorHex = run.RunProperties?.Color?.Val?.Value;

                            if (colorHex is not null)
                            {
                                stringBuilder.Append($"""<span style="color:#{colorHex}">""");
                            }

                            stringBuilder.Append(run.InnerText);

                            if (colorHex is not null)
                            {
                                stringBuilder.Append("</span>");
                            }
                        }
                    }

                    if (isBold)
                    {
                        stringBuilder.Append("**");
                    }

                    if (isItalic)
                    {
                        stringBuilder.Append('*');
                    }

                    if (isStrike)
                    {
                        stringBuilder.Append("~~");
                    }

                    if (isUnderline)
                    {
                        stringBuilder.Append("</u>");
                    }
                    #endregion

                    #region 写样式
                    List<string> classList = new(3);

                    #region 段落布局设置
                    if (paraProp?.Justification?.Val is not null)
                    {
                        AddLayoutByJustificationValue(classList, paraProp.Justification.Val);
                    }
                    else if (paraStyle?.StyleParagraphProperties?.Justification?.Val is not null)
                    {
                        AddLayoutByJustificationValue(classList, paraStyle.StyleParagraphProperties.Justification.Val);
                    }
                    #endregion

                    if (classList.Count != 0)
                    {
                        stringBuilder.Append('{');

                        if (classList.Count == 1)
                        {
                            stringBuilder.Append(classList[0]);
                        }
                        else
                        {
                            stringBuilder.Append($" {string.Join(' ', classList)} ");
                        }
                        
                        stringBuilder.Append('}');
                    }
                    #endregion

                    stringBuilder.AppendLine();
                }

                stringBuilder.AppendLine();
            }
        }

        if (leaveOpen != true)
        {
            document.Dispose();
        }

        return stringBuilder.ToString();
    }

    private static void DetermineRunPropertiesByChildElement(OpenXmlElementList childElements, ref bool isBold, ref bool isStrike, ref bool isItalic, ref bool isUnderline)
    {
        isBold = childElements.Any(element => element is Bold bold);
        isStrike = childElements.Any(element => element is Strike strike);
        isItalic = childElements.Any(element => element is Italic italic);
        isUnderline = childElements.Any(element => element is Underline underline);
    }

    private static void AddLayoutByJustificationValue(List<string> classList, JustificationValues justification)
    {
        ArgumentNullException.ThrowIfNull(classList);

        if (justification == JustificationValues.Right)
        {
            classList.Add(".aright");
        }
        else if (justification == JustificationValues.Left)
        {
            classList.Add(".aleft");
        }
        else if (justification == JustificationValues.Center)
        {
            classList.Add(".centering");
        }
    }

    private static void WriteMarkdownHeaderByOutlineLevel(StringBuilder writer, OutlineLevel outlineLevel)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(outlineLevel);

        if (outlineLevel.Val != null && outlineLevel.Val.Value != 9)
        {
            IEnumerable<char> paraIndicators = Enumerable.Repeat('#', outlineLevel.Val.Value + 1);

            string paraIndicator = new(paraIndicators.ToArray());
            writer.Append($"{paraIndicator} ");
        }
    }
}

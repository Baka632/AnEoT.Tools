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

                    if (paraProp?.OutlineLevel?.Val != null)
                    {
                        WriteMarkdownHeaderByOutlineLevel(stringBuilder, paraProp.OutlineLevel);
                    }
                    else if (string.IsNullOrWhiteSpace(paraProp?.ParagraphStyleId?.Val?.Value) != true && mainDocumentPart?.StyleDefinitionsPart?.Styles is not null)
                    {
                        Style? targetStyle = (Style?)mainDocumentPart.StyleDefinitionsPart.Styles.FirstOrDefault(element => element is Style style && style.StyleId == paraProp.ParagraphStyleId.Val.Value);
                        if (targetStyle != null && targetStyle.StyleParagraphProperties?.OutlineLevel is not null)
                        {
                            WriteMarkdownHeaderByOutlineLevel(stringBuilder, targetStyle.StyleParagraphProperties.OutlineLevel);
                        }
                    }

                    stringBuilder.Append(paragraph.InnerText);

                    if (paraProp?.Justification?.Val != null)
                    {
                        if (paraProp.Justification.Val.Value == JustificationValues.Right)
                        {
                            stringBuilder.Append("{.aright}");
                        }
                    }

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

#pragma warning disable CS0618
#pragma warning disable IDE0290

using Microsoft.UI.Xaml.Documents;
using System.Text.RegularExpressions;
using AnEoT.Tools.VolumeCreator.Controls;
using CommunityToolkit.Common.Parsers.Markdown;
using CommunityToolkit.Common.Parsers.Markdown.Inlines;
using CommunityToolkit.Common.Parsers.Markdown.Render;
using CommunityToolkit.WinUI.UI.Controls.Markdown.Render;

namespace AnEoT.Tools.VolumeCreator.Helpers;

public partial class AnEoTMarkdownRenderer : MarkdownRenderer
{
    public AnEoTMarkdownRenderer(MarkdownDocument document, ILinkRegister linkRegister, IImageResolver imageResolver, ICodeBlockResolver codeBlockResolver) : base(document, linkRegister, imageResolver, codeBlockResolver)
    {
    }

    protected override void RenderTextRun(TextRunInline element, IRenderContext context)
    {
        string text = element.Text;
        Match tagsMatch = GetTagsRegex().Match(text);
        Regex eodMatch = GetEodRegex();
        Regex fakeAdsMatch = GetFakeAdsRegex();

        if (tagsMatch.Success)
        {
            ReadOnlySpan<char> textSpan = text.AsSpan();

            List<(Range, string) > tagsRanges = new(10);

            while (tagsMatch.Success)
            {
                Range tagRange = new(tagsMatch.Index, tagsMatch.Index + tagsMatch.Length);
                tagsRanges.Add((tagRange, tagsMatch.Value));
                tagsMatch = tagsMatch.NextMatch();
            }

            Range lastTagRange = new(0, 0);

            for (int i = 0; i < tagsRanges.Count; i++)
            {
                (Range tagRange, string tagString) = tagsRanges[i];
                bool hasNextTag = i + 1 < tagsRanges.Count;

                ReadOnlySpan<char> normalTextSpan = textSpan[lastTagRange.End.Value..tagRange.Start.Value];

                if (normalTextSpan.Length > 0)
                {
                    TextRunInline newRunInline = new()
                    {
                        Text = normalTextSpan.ToString(),
                        Type = element.Type
                    };
                    base.RenderTextRun(newRunInline, context);
                }

                if (eodMatch.IsMatch(tagString))
                {
                    InlineRenderContext localContext = (InlineRenderContext)context;
                    InlineCollection inlineCollection = localContext.InlineCollection;

                    InlineUIContainer item = new()
                    {
                        Child = new Eod(),
                    };
                    inlineCollection.Add(item);
                }
                else if (fakeAdsMatch.IsMatch(tagString))
                {
                    InlineRenderContext localContext = (InlineRenderContext)context;
                    InlineCollection inlineCollection = localContext.InlineCollection;

                    InlineUIContainer item = new()
                    {
                        Child = new FakeAds(),
                    };
                    inlineCollection.Add(item);
                }

                if (hasNextTag == false)
                {
                    ReadOnlySpan<char> remainingTextSpan = textSpan[tagRange.End.Value..];
                    if (remainingTextSpan.Length > 0)
                    {
                        TextRunInline newRunInline = new()
                        {
                            Text = remainingTextSpan.ToString(),
                            Type = element.Type
                        };
                        base.RenderTextRun(newRunInline, context);
                    }
                }

                lastTagRange = tagRange;
            }
        }
        else
        {
            base.RenderTextRun(element, context);
        }
    }

    [GeneratedRegex(@"(<eod( *)/>)|(<FakeAds( *)/>)")]
    private static partial Regex GetTagsRegex();

    [GeneratedRegex(@"<eod( *)/>")]
    private static partial Regex GetEodRegex();
    
    [GeneratedRegex(@"<FakeAds( *)/>")]
    private static partial Regex GetFakeAdsRegex();
}
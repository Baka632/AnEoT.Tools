#pragma warning disable CS0618
#pragma warning disable IDE0290

using CommunityToolkit.Common.Parsers.Markdown;
using CommunityToolkit.Common.Parsers.Markdown.Blocks;
using CommunityToolkit.Common.Parsers.Markdown.Inlines;
using CommunityToolkit.Common.Parsers.Markdown.Render;
using CommunityToolkit.WinUI.UI.Controls.Markdown.Render;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;

namespace AnEoT.Tools.VolumeCreator.Helpers;

public class AnEoTMarkdownRenderer : MarkdownRenderer
{
    public AnEoTMarkdownRenderer(MarkdownDocument document, ILinkRegister linkRegister, IImageResolver imageResolver, ICodeBlockResolver codeBlockResolver) : base(document, linkRegister, imageResolver, codeBlockResolver)
    {
    }

    protected override void RenderTextRun(TextRunInline element, IRenderContext context)
    {
        if (element.Text.Replace(" ", string.Empty).Equals("<eod/>"))
        {
            InlineRenderContext localContext = (InlineRenderContext)context;
            InlineCollection inlineCollection = localContext.InlineCollection;

            const string pathIconXaml = $"""<PathIcon xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" HorizontalAlignment="Center" Data="M 161.03 154.06 305.81 154.06 305.81 240.52 371.61 240.52 371.61 154.06 483.87 154.06 516.39 92.13 53.27 93.53 339.1 603.1 681.29 0 0 0 34.21 60.53 569.03 61.16 371.61 411.87 370.84 278.71 305.81 279.23 305.81 412.39 161.03 154.06 Z" />""";

            PathIcon? icon = XamlReader.Load(pathIconXaml) as PathIcon;

            Viewbox viewbox = new()
            {
                Child = icon,
                Stretch = Stretch.Uniform,
                Height = 15,
                Width = 15,
            };

            InlineUIContainer item = new()
            {
                Child = viewbox,
            };
            inlineCollection.Add(item);
        }
        else
        {
            base.RenderTextRun(element, context);
        }
    }
}
using System.Globalization;
using System.Text;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;

[INotifyPropertyChanged]
public sealed partial class CompletePage : Page
{
    private CreatePaintingPageWindowAccessor windowAccessor;
    private CreatePaintingPageData data;

    [ObservableProperty]

    private string generatedMarkdown;

#pragma warning disable CS8618 // OnNavigatedTo 会出手
    public CompletePage()
    {
        this.InitializeComponent();
    }
#pragma warning restore CS8618

    partial void OnGeneratedMarkdownChanged(string value)
    {
        windowAccessor.PaintingPageData = windowAccessor.PaintingPageData with
        {
            GeneratedMarkdown = value
        };
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is CreatePaintingPageWindowAccessor accessor)
        {
            windowAccessor = accessor;
            data = windowAccessor.PaintingPageData;

            accessor.ShowComplete = true;
        }
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        GeneratedMarkdown = GeneratePaintingPageMarkdown(data);
    }

    private static string GeneratePaintingPageMarkdown(CreatePaintingPageData data)
    {
        StringBuilder builder = new(700);
        builder.AppendLine("---");
        builder.AppendLine("icon: palette");
        builder.AppendLine("title: 画中秘境");
        builder.AppendLine($"date: {DateTimeOffset.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        builder.AppendLine("category:");
        builder.AppendLine("  - 画中秘境");
        builder.AppendLine("tag:");
        builder.AppendLine("  - 插画");
        builder.AppendLine("");
        builder.AppendLine("order: -3");
        builder.AppendLine("---");
        builder.AppendLine("<!-- more -->");
        builder.AppendLine();
        builder.AppendLine("---");
        WritePaintingContent(builder, data);
        builder.AppendLine();
        builder.Append("<FakeAds />");

        string markdown = builder.ToString();
        return markdown;
    }

    private static void WritePaintingContent(StringBuilder builder, CreatePaintingPageData data)
    {
        foreach (PaintingInfo info in data.PaintingInfos)
        {
            builder.AppendLine();

            string imageUri = CommonValues.ConstructImageUriByFileNode(info.File);
            if (data.ConvertWebP)
            {
                imageUri = Path.ChangeExtension(imageUri, ".webp");
            }
            string markdownImageMark = $"![]({imageUri})";

            builder.AppendLine(markdownImageMark);
            builder.AppendLine();

            builder.AppendLine($"Artist: {info.AuthorName}");
            builder.AppendLine();
            builder.AppendLine("---");
        }
    }
}

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Views;
using AnEoT.Tools.Shared.Models;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Text;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class MarkdownEditViewModel : ObservableObject
{
    public MarkdownWrapper MarkdownWrapper { get; }
    public ObservableCollection<ImageListNode> ImageFiles { get; }
    public StorageFile? CoverImageFile { get; }
    public Dictionary<string, string> MarkdownImageUriToFileMapping { get; } = new(10);

    private readonly MarkdownEditPage view;

    [ObservableProperty]
    private string markdownString;
    [ObservableProperty]
    private string articleQuote = string.Empty;

    public MarkdownEditViewModel(MarkdownWrapper wrapper, MarkdownEditPage viewPage, ObservableCollection<ImageListNode> imageFiles, StorageFile? coverImageFile)
    {
        markdownString = wrapper.Markdown;
        view = viewPage;
        CoverImageFile = coverImageFile;
        ImageFiles = imageFiles;
        MarkdownWrapper = wrapper;

        if (coverImageFile is not null)
        {
            MarkdownImageUriToFileMapping["./res/cover.webp"] = coverImageFile.Path;
        }
    }

    partial void OnMarkdownStringChanged(string value)
    {
        MarkdownWrapper.Markdown = value;
    }

    partial void OnArticleQuoteChanged(string value)
    {
        int index = 0;
        bool hasYaml = MarkdownString.StartsWith("---");
        if (hasYaml)
        {
            int targetIndex = MarkdownString.IndexOf("---", 3);
            if (targetIndex != -1)
            {
                index = targetIndex + 3;
            }
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            string quoteLiteral = hasYaml
                ? $"{Environment.NewLine}<!-- more -->"
                : $"<!-- more -->{Environment.NewLine}{Environment.NewLine}";
            MarkdownString = MarkdownString.Insert(index, quoteLiteral);
        }
        else
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (hasYaml)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine(value.ReplaceLineEndings($"{Environment.NewLine}{Environment.NewLine}"));
            stringBuilder.AppendLine();
            if (hasYaml)
            {
                stringBuilder.Append("<!-- more -->");
            }
            else
            {
                stringBuilder.AppendLine("<!-- more -->");
                stringBuilder.AppendLine();
            }

            string quoteLiteral = stringBuilder.ToString();
            MarkdownString = MarkdownString.Insert(index, quoteLiteral);
        }
    }

    [RelayCommand]
    private void AddEodTagToText(TextBox textBox)
    {
        const string eodTag = "  <eod />";

        ArgumentNullException.ThrowIfNull(textBox);
        int position = textBox.SelectionStart;
        MarkdownString = textBox.Text.Insert(position, eodTag);
        textBox.Select(position, eodTag.Length);
    }

    [RelayCommand]
    private void AddBreakLineToText(TextBox textBox)
    {
        string breakLine = $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}---{Environment.NewLine}";

        ArgumentNullException.ThrowIfNull(textBox);
        int position = textBox.SelectionStart;
        MarkdownString = textBox.Text.Insert(position, breakLine);
        textBox.Select(position + Environment.NewLine.Length * 2, 0);
    }

    [RelayCommand]
    private async Task AddFrontMatterToText(TextBox textBox)
    {
        FrontMatterDialog dialog = new()
        {
            XamlRoot = view.XamlRoot
        };
        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            (FrontMatter frontMatter, PredefinedCategory? predefinedCategory) = dialog.Result;
            MarkdownWrapper.CategoryInIndexPage = predefinedCategory;
            string yamlHeader = GetYamlFrontMatterString(frontMatter);

            int orderValueIndex = yamlHeader.IndexOf("order:");
            if (orderValueIndex != -1)
            {
                yamlHeader = yamlHeader.Insert(orderValueIndex, Environment.NewLine);
            }

            MarkdownString = MarkdownString.Insert(0, yamlHeader);
            textBox.Select(0, 0);
        }
    }

    [RelayCommand]
    private async Task AddEditorsInfoToText(TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        EditorsInfoDialog dialog = new()
        {
            XamlRoot = view.XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            EditorsInfo editorsInfo = dialog.EditorsInfo;
            string editorsInfoLiteral = $"{Environment.NewLine}{Environment.NewLine}{editorsInfo}";

            if (string.IsNullOrWhiteSpace(editorsInfoLiteral) != true)
            {
                MarkdownString += editorsInfoLiteral;
                textBox.Select(MarkdownString.Length, 0);
            }
        }
    }

    [RelayCommand]
    private void AddFakeAdsTagToText(TextBox textBox)
    {
        const string fakeAdsTag = "<FakeAds />";

        ArgumentNullException.ThrowIfNull(textBox);
        MarkdownString += $"{Environment.NewLine}{Environment.NewLine}{fakeAdsTag}";
        textBox.Select(MarkdownString.Length - fakeAdsTag.Length, fakeAdsTag.Length);
    }

    [RelayCommand]
    private async Task InsertStyles(TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        InsertStyleDialog dialog = new()
        {
            XamlRoot = view.XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && string.IsNullOrWhiteSpace(dialog.StyleString) != true)
        {
            int position = textBox.SelectionStart;
            string style = dialog.StyleString;

            MarkdownString = textBox.Text.Insert(position, style);
            textBox.Select(position, style.Length);
        }
    }

    [RelayCommand]
    private void InsertAlignLeft(TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        string style = GenerateCssClassList("aleft");
        int position = textBox.SelectionStart;
        MarkdownString = textBox.Text.Insert(position, style);
        textBox.Select(position, style.Length);
    }

    [RelayCommand]
    private void InsertAlignRight(TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        string style = GenerateCssClassList("aright");
        int position = textBox.SelectionStart;
        MarkdownString = textBox.Text.Insert(position, style);
        textBox.Select(position, style.Length);
    }

    [RelayCommand]
    private void InsertAlignCenter(TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        string style = GenerateCssClassList("centering");
        int position = textBox.SelectionStart;
        MarkdownString = textBox.Text.Insert(position, style);
        textBox.Select(position, style.Length);
    }

    [RelayCommand]
    private void InsertTextKai(TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        string style = GenerateCssClassList("textkai");
        int position = textBox.SelectionStart;
        MarkdownString = textBox.Text.Insert(position, style);
        textBox.Select(position, style.Length);
    }

    private static string GenerateCssClassList(string className)
    {
        return $"{{.{className}}}";
    }

    public void InsertImageToText(TextBox textBox, FileNode fileNode)
    {
        List<string> targetParts = new(3);
        ImageListNode? parentNode = fileNode;
        while (true)
        {
            if (parentNode is null)
            {
                break;
            }
            targetParts.Add(parentNode.DisplayName);

            if (parentNode.DisplayName.Equals("res", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            parentNode = parentNode.Parent;
        }

        targetParts.Reverse();

        string imageUri = $"./{string.Join('/', targetParts)}";
        string markdownImageMark = $"![]({imageUri})";

        // TODO: 这里只会在插入图片时初始化，所以关闭窗口后再次进入时会导致无法显示图像。
        MarkdownImageUriToFileMapping[imageUri] = fileNode.FilePath;

        int position = textBox.SelectionStart;
        string markToInsert = $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{markdownImageMark}{Environment.NewLine}";
        MarkdownString = textBox.Text.Insert(position, markToInsert);
        textBox.Select(position + markToInsert.Length, 0);
    }

    private static string GetYamlFrontMatterString(FrontMatter frontMatter)
    {
        ISerializer serializer = new SerializerBuilder()
                        .WithIndentedSequences()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
        string yamlString = serializer.Serialize(frontMatter).Trim();

        StringBuilder stringBuilder = new(200);
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine(yamlString);
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }
}
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

public sealed partial class MarkdownEditViewModel(MarkdownWrapper wrapper, MarkdownEditPage view, ObservableCollection<ImageListNode> imageFiles) : ObservableObject
{
    public MarkdownWrapper MarkdownWrapper { get; } = wrapper;
    public ObservableCollection<ImageListNode> ImageFiles { get; } = imageFiles;
    public Dictionary<string, StorageFile> MarkdownImageUriToFileMapping { get; } = new(10);

    [ObservableProperty]
    private string markdownString = wrapper.Markdown;
    [ObservableProperty]
    private string articleQuote = string.Empty;

    partial void OnMarkdownStringChanged(string value)
    {
        MarkdownWrapper.Markdown = value;
    }

    partial void OnArticleQuoteChanged(string value)
    {
        int index = 0;
        if (MarkdownString.StartsWith("---"))
        {
            int targetIndex = MarkdownString.IndexOf("---", 3);
            if (targetIndex != -1)
            {
                index = targetIndex + 3;
            }
        }

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(value.ReplaceLineEndings($"{Environment.NewLine}{Environment.NewLine}"));
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("<!-- more -->");
        stringBuilder.AppendLine();

        string quoteLiteral = stringBuilder.ToString();
        MarkdownString = MarkdownString.Insert(index, quoteLiteral);
    }

    [RelayCommand]
    private void AddEodTagToText(TextBox textBox)
    {
        const string eodTag = "<eod />";

        ArgumentNullException.ThrowIfNull(textBox);
        int position = textBox.SelectionStart;
        MarkdownString = MarkdownString.Insert(position, eodTag);
        textBox.Select(position + eodTag.Length, 0);
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
            FrontMatter frontMatter = dialog.FrontMatter;
            string yamlHeader = GetYamlFrontMatterString(frontMatter);
            MarkdownString = MarkdownString.Insert(0, yamlHeader);
            textBox.Select(0, 0);
        }
    }

    [RelayCommand]
    private async Task AddEditorsInfoToText(TextBox textBox)
    {
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
        textBox.Select(MarkdownString.Length, 0);
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

        MarkdownImageUriToFileMapping[imageUri] = fileNode.File;

        int position = textBox.SelectionStart;
        MarkdownString = MarkdownString.Insert(position, markdownImageMark);
        textBox.Select(position + markdownImageMark.Length, 0);
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
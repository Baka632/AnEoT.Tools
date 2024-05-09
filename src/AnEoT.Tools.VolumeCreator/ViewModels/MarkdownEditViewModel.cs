using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Views;
using AnEoT.Tools.Shared.Models;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Text;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class MarkdownEditViewModel(MarkdownWrapper wrapper, MarkdownEditPage view) : ObservableObject
{
    public MarkdownWrapper MarkdownWrapper { get; } = wrapper;

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

            StringBuilder stringBuilder = new(70);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append(editorsInfo.ToString());

            string editorsInfoLiteral = stringBuilder.ToString();

            if (string.IsNullOrWhiteSpace(editorsInfoLiteral) != true)
            {
                MarkdownString += editorsInfoLiteral;
                textBox.Select(MarkdownString.Length, 0);
            }
        }
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
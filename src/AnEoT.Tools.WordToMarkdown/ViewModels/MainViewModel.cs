using System.IO;
using System.Windows;
using AnEoT.Tools.WordToMarkdown.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Win32;

namespace AnEoT.Tools.WordToMarkdown.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;
    [ObservableProperty]
    private string markdownString = string.Empty;

    [RelayCommand]
    private void OpenWordAndStartLoading()
    {
        IsLoading = true;

        OpenFileDialog dialog = new()
        {
            DefaultExt = ".docx",
            Filter = "Word 文档|*.docx"
        };

        bool? isOk = dialog.ShowDialog();

        if (isOk == true)
        {
            using WordprocessingDocument doc = WordprocessingDocument.Open(dialog.FileName, false);
            string markdown = WordToMarkdownService.GetMarkdown(doc);
            MarkdownString = markdown;
        }

        IsLoading = false;
    }

    [RelayCommand]
    private async Task SaveMarkdown()
    {
        if (string.IsNullOrWhiteSpace(MarkdownString))
        {
            MessageBox.Show("Markdown 文本为空。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SaveFileDialog dialog = new()
        {
            DefaultExt = ".md",
            Filter = "Markdown 文档|*.md"
        };

        bool? isOk = dialog.ShowDialog();

        if (isOk == true)
        {
            using StreamWriter writer = File.CreateText(dialog.FileName);
            await writer.WriteAsync(MarkdownString);

            MessageBox.Show("保存成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
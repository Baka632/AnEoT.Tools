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
            try
            {
                using WordprocessingDocument doc = WordprocessingDocument.Open(dialog.FileName, false);
                string markdown = WordToMarkdownService.GetMarkdown(doc);
                MarkdownString = markdown;
            }
            catch (IOException)
            {
                MessageBox.Show("文件正在被其他进程使用。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FileFormatException)
            {
                MessageBox.Show("此文件似乎不是有效的 docx 文件。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"出现未知错误。\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            try
            {
                using StreamWriter writer = File.CreateText(dialog.FileName);
                await writer.WriteAsync(MarkdownString);

                MessageBox.Show("保存成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"出现未知错误。\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AnEoT.Tools.WordToMarkdown.Models;
using AnEoT.Tools.WordToMarkdown.Services;
using AnEoT.Tools.WordToMarkdown.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Win32;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AnEoT.Tools.WordToMarkdown.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;
    [ObservableProperty]
    private bool isTextBoxReadOnly;
    [ObservableProperty]
    private string markdownString = string.Empty;

    #region 通过命令行传递的参数
    public string? WordFilePath { get; set; }
    public string? OutputFilePath { get; set; }
    public string? ArticleQuote { get; set; }
    public ArticleInfo? FrontMatter { get; set; }
    public EditorsInfo? EditorsInfo { get; set; }
    #endregion

    [RelayCommand]
    private async Task OpenWordAndStartLoading()
    {
        IsLoading = true;

        if (string.IsNullOrWhiteSpace(WordFilePath) != true && Path.Exists(WordFilePath))
        {
            if (await SetMarkdownStringFromWord(WordFilePath))
            {
                WordFilePath = null;
            }
            IsLoading = false;
            return;
        }

        OpenFileDialog dialog = new()
        {
            DefaultExt = ".docx",
            Filter = "Word 文档|*.docx"
        };

        bool? isOk = dialog.ShowDialog();

        if (isOk == true)
        {
            _ = await SetMarkdownStringFromWord(dialog.FileName);
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
        else if (string.IsNullOrWhiteSpace(OutputFilePath) != true && Path.Exists(OutputFilePath))
        {
            if (await WriteMarkdownToFile(OutputFilePath, MarkdownString))
            {
                OutputFilePath = null;
            }
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
            await WriteMarkdownToFile(dialog.FileName, MarkdownString);
        }
    }

    [RelayCommand]
    private void AddEodTagToTextBox(TextBox textBox)
    {
        if (IsTextBoxReadOnly)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(textBox);

        int caretIndex = textBox.CaretIndex;
        MarkdownString = MarkdownString.Insert(caretIndex, "<eod />");
        textBox.Select(caretIndex, 0);
    }

    [RelayCommand]
    private async Task AddEditorInfoToTextBox(TextBox? textBox)
    {
        if (IsTextBoxReadOnly)
        {
            return;
        }
        else if (EditorsInfo.HasValue)
        {
            IsLoading = true;
            await Task.Run(() =>
            {
                string editorInfo = GetEditorsInfoString(EditorsInfo.Value);
                if (string.IsNullOrWhiteSpace(editorInfo) != true)
                {
                    MarkdownString += editorInfo;
                }
            });
            EditorsInfo = null;
            IsLoading = false;
            return;
        }

        ArgumentNullException.ThrowIfNull(textBox);

        EditorsInfoDialog dialog = new()
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            string editorInfo = GetEditorsInfoString(dialog.EditorsInfo);

            if (string.IsNullOrWhiteSpace(editorInfo) != true)
            {
                textBox.AppendText(editorInfo);
                textBox.Select(MarkdownString.Length - 1, 0);
            }
        }
    }
    
    [RelayCommand]
    private async Task AddArticleQuoteToTextBox(TextBox? textBox)
    {
        if (IsTextBoxReadOnly)
        {
            return;
        }
        else if (string.IsNullOrWhiteSpace(ArticleQuote) != true)
        {
            IsLoading = true;
            await Task.Run(() =>
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

                string value = $"""
                {ArticleQuote.ReplaceLineEndings($"{Environment.NewLine}{Environment.NewLine}")}

                <!-- more -->


                """;
                MarkdownString = MarkdownString.Insert(index, value);
            });
            IsLoading = false;
            ArticleQuote = null;
            return;
        }

        ArgumentNullException.ThrowIfNull(textBox);

        ArticleQuoteDialog dialog = new()
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            if (string.IsNullOrWhiteSpace(dialog.ArticleQuote) != true)
            {
                string value = $"""
                {dialog.ArticleQuote.ReplaceLineEndings($"{Environment.NewLine}{Environment.NewLine}")}

                <!-- more -->


                """;
                int index = 0;
                if (MarkdownString.StartsWith("---"))
                {
                    int targetIndex = MarkdownString.IndexOf("---", 3);
                    if (targetIndex != -1)
                    {
                        index = targetIndex + 3;
                        value = $"{Environment.NewLine}{Environment.NewLine}{value}";
                    }
                }
                MarkdownString = MarkdownString.Insert(index, value);
            }
        }
    }

    [RelayCommand]
    private async Task AddFrontMatterToTextBox(TextBox? textBox)
    {
        if (IsTextBoxReadOnly)
        {
            return;
        }
        else if (FrontMatter.HasValue)
        {
            IsLoading = true;
            await Task.Run(() =>
            {
                string yamlHeader = GetYamlFrontMatterString(FrontMatter.Value);
                MarkdownString = MarkdownString.Insert(0, yamlHeader);
            });
            IsLoading = false;
            FrontMatter = null;
            return;
        }

        ArgumentNullException.ThrowIfNull(textBox);
        FrontMatterInfoDialog dialog = new()
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            ArticleInfo articleInfo = dialog.FrontMatter;
            string yamlHeader = GetYamlFrontMatterString(articleInfo);
            MarkdownString = MarkdownString.Insert(0, yamlHeader);
            textBox.Select(0, 0);
        }
    }

    private static async Task<string> GetMarkdownString(string path)
    {
        try
        {
            return await Task.Run(() =>
            {
                using WordprocessingDocument doc = WordprocessingDocument.Open(path, false);
                return WordToMarkdownService.GetMarkdown(doc);
            });
        }
        catch (IOException)
        {
            MessageBox.Show("文件正在被其他进程使用。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            return string.Empty;
        }
        catch (FileFormatException)
        {
            MessageBox.Show("此文件似乎不是有效的 docx 文件。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return string.Empty;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"出现未知错误。\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return string.Empty;
        }
    }

    private async Task<bool> SetMarkdownStringFromWord(string path)
    {
        string markdown = await GetMarkdownString(path);

        if (string.IsNullOrWhiteSpace(markdown) != true)
        {
            if (markdown.Length > 100000)
            {
                IsTextBoxReadOnly = true;
                MarkdownString = "解析出来的 Markdown 文档太长，预览已禁用。\n\n不过，您仍可以保存 Markdown 文档。";
#pragma warning disable MVVMTK0034
                markdownString = markdown;
#pragma warning restore MVVMTK0034
            }
            else
            {
                IsTextBoxReadOnly = false;
                MarkdownString = markdown;
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private static string GetYamlFrontMatterString(ArticleInfo articleInfo)
    {
        ISerializer serializer = new SerializerBuilder()
                        .WithIndentedSequences()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
        string yamlString = serializer.Serialize(articleInfo).Trim();
        string yamlHeader = $"""
                ---
                {yamlString}
                ---


                """;
        return yamlHeader;
    }

    private static string GetEditorsInfoString(EditorsInfo editorsInfo)
    {
        return $"""


            {editorsInfo}
            """;
    }

    private static async Task<bool> WriteMarkdownToFile(string path, string markdown)
    {
        try
        {
            StreamWriter writer = File.CreateText(path);
            await writer.WriteAsync(markdown);
            await writer.DisposeAsync();

            MessageBox.Show("保存成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"出现未知错误。\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}
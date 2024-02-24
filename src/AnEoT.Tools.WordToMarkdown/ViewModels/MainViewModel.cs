using System.Diagnostics;
using System.IO;
using System.Text;
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
using YamlDotNet.Serialization.TypeInspectors;

namespace AnEoT.Tools.WordToMarkdown.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;
    [ObservableProperty]
    private bool isTextBoxReadOnly;
    [ObservableProperty]
    private string markdownString = string.Empty;

    [RelayCommand]
    private async Task OpenWordAndStartLoading()
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
                string markdown = await Task.Run(() =>
                {
                    using WordprocessingDocument doc = WordprocessingDocument.Open(dialog.FileName, false);
                    return WordToMarkdownService.GetMarkdown(doc);
                });

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
                StreamWriter writer = File.CreateText(dialog.FileName);
                await writer.WriteAsync(MarkdownString);
                await writer.DisposeAsync();

                MessageBox.Show("保存成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"出现未知错误。\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    private void AddEditorInfoToTextBox(TextBox textBox)
    {
        if (IsTextBoxReadOnly)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(textBox);

        EditorsInfoDialog dialog = new()
        {
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            List<string> editorsInfoPart = new(3);
            StringBuilder builder = new(50);

            if (string.IsNullOrWhiteSpace(dialog.EditorString) != true)
            {
                editorsInfoPart.Add($"责任编辑：{dialog.EditorString}");
            }
            
            if (string.IsNullOrWhiteSpace(dialog.WebsiteLayoutDesigner) != true)
            {
                editorsInfoPart.Add($"网页排版：{dialog.WebsiteLayoutDesigner}");
            }
            
            if (string.IsNullOrWhiteSpace(dialog.Illustrator) != true)
            {
                editorsInfoPart.Add($"绘图：{dialog.Illustrator}");
            }

            if (editorsInfoPart.Count == 0)
            {
                return;
            }

            string editorInfos = string.Join('；', editorsInfoPart);
            builder.AppendLine();
            builder.Append($"（{editorInfos}）");

            textBox.AppendText(builder.ToString());

            textBox.Select(MarkdownString.Length - 1, 0);
        }
    }

    [RelayCommand]
    private void AddFrontMatterToTextBox(TextBox textBox)
    {
        if (IsTextBoxReadOnly)
        {
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
            ISerializer serializer = new SerializerBuilder()
                .WithIndentedSequences()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            string yamlString = serializer.Serialize(articleInfo).Trim();
            string yamlHeader =$"""
                ---
                {yamlString}
                ---

                """;

            MarkdownString = MarkdownString.Insert(0, yamlHeader);
            textBox.Select(0, 0);
        }
    }
}
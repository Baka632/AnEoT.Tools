using AnEoT.Tools.WordToMarkdown.ViewModels;
using AnEoT.Tools.WordToMarkdown.Views;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using System.IO;
using AnEoT.Tools.WordToMarkdown.Models;
using System.Globalization;
using AnEoT.Tools.WordToMarkdown.Services;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace AnEoT.Tools.WordToMarkdown;

partial class App
{
    private static async Task RunByOptions(CommonOptions options)
    {
        string? filePath = options.WordFilePath;
        string? outputPath = options.OutputFilePath;
        string? author = options.Author;
        string? title = options.Title;
        string? shortTitle = options.ShortTitle;
        string? iconString = options.IconString;
        string? date = options.DateString;
        string? description = options.Description;
        int order = options.Order;
        IEnumerable<string>? categories = options.Categories;
        IEnumerable<string>? tags = options.Tag;

        EditorsInfo editorsInfo = new(options.EditorName, options.WebsiteLayoutDesigner, options.Illustrator);
        ArticleInfo articleInfo = new()
        {
            Author = author,
            Title = title ?? string.Empty,
            ShortTitle = shortTitle,
            Icon = iconString ?? "article",
            Category = categories,
            Tag = tags,
            Date = DateOnly.TryParse(date, CultureInfo.InvariantCulture, out _)
                ? date : DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Description = description,
            Order = order,
        };

        if (options is GuiOptions gui)
        {
            MainWindow mainWindow = new();
            MainViewModel viewModel = mainWindow.ViewModel;

            viewModel.OutputFilePath = outputPath;
            viewModel.WordFilePath = filePath;
            viewModel.EditorsInfo = editorsInfo;
            viewModel.FrontMatter = articleInfo;

            if (string.IsNullOrEmpty(filePath) != true && gui.DisableAutoStart != true)
            {
                Console.WriteLine("请稍后，正在准备数据...");
                await viewModel.OpenWordAndStartLoadingCommand.ExecuteAsync(null);
                await viewModel.AddFrontMatterToTextBoxCommand.ExecuteAsync(null);
                await viewModel.AddEditorInfoToTextBoxCommand.ExecuteAsync(null);
            }

            await Task.Delay(500);

            HideConsole();
            mainWindow.Show();
            Console.WriteLine("已加载图形界面。");
            Console.WriteLine("啊啊啊，你为什么在看这里！看图形界面，看图形界面啊！");
        }
        else if (options is CliOptions cli)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                PrintErrorAndExist("错误：指定的文件路径为空。");
            }
            else if (Path.Exists(filePath) != true)
            {
                PrintErrorAndExist("错误：指定的文件路径不存在或无法访问。");
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.Combine(Environment.CurrentDirectory, $"{title ?? Path.GetRandomFileName()}.md");
                Console.WriteLine($"警告：未指定的输出路径，将使用以下路径：{outputPath}");
            }
            else if (File.Exists(outputPath))
            {
                Console.WriteLine("警告：指定的输出路径已存在同名文件，将覆盖目标。");
            }

            string markdown = GetMarkdownString(filePath, articleInfo, editorsInfo);
            if (cli.AutoAppendEod)
            {
                markdown = markdown.Insert(markdown.Length - 1, "<eod />");
            }

            if (await WriteMarkdownToFile(outputPath, markdown))
            {
                Environment.Exit(0);
            }
        }
    }

    private static void HideConsole()
    {
        HWND hwnd = PInvoke.GetConsoleWindow();
        PInvoke.ShowWindow(hwnd, 0); // 实际上是隐藏窗口
    }

    private static async Task<bool> WriteMarkdownToFile(string outputPath, string markdown)
    {
        try
        {
            StreamWriter writer = File.CreateText(outputPath);
            await writer.WriteAsync(markdown);
            await writer.DisposeAsync();

            Console.WriteLine("操作成功完成。");
            return true;
        }
        catch (Exception ex)
        {
            PrintErrorAndExist($"出现未知错误：\n{ex}");
            return false;
        }
    }

    private static string GetMarkdownString(string filePath, ArticleInfo articleInfo, EditorsInfo editorsInfo)
    {
        string? markdown = null;
        try
        {
            using WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false);
            markdown = WordToMarkdownService.GetMarkdown(doc);
        }
        catch (IOException)
        {
            PrintErrorAndExist("错误：指定的 Word 文件正在被其他进程使用。");
        }
        catch (FileFormatException)
        {
            PrintErrorAndExist("错误：指定的文件似乎不是有效的 docx 文件。");
        }
        catch (Exception ex)
        {
            PrintErrorAndExist($"出现未知错误：\n{ex}");
        }

        string yamlHeader = GetYamlFrontMatterString(articleInfo);
        string editorsInfoString = $"""


            {editorsInfo}
            """;

        StringBuilder stringBuilder = new(markdown);
        stringBuilder.Insert(0, yamlHeader);
        stringBuilder.Append(editorsInfoString);

        return stringBuilder.ToString();
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

    [DoesNotReturn]
    private static void PrintErrorAndExist(string message)
    {
        Console.WriteLine(message);
        Environment.Exit(1);
    }
}

[Verb("gui", HelpText = "以图形界面启动。")]
internal sealed class GuiOptions : CommonOptions
{
    [Option("disable-auto-start", HelpText = "禁用启动转换操作。")]
    public bool DisableAutoStart { get; set; }
}

[Verb("cli", isDefault: true, HelpText = "以命令行形式启动。")]
internal sealed class CliOptions : CommonOptions
{
    [Option("auto-append-eod", HelpText = "自动在文章末尾添加 <eod /> 标签。")]
    public bool AutoAppendEod { get; set; }

    [Option('f', "file", Required = true, HelpText = "Word 文件的路径。")]
    public override string? WordFilePath { get; set; }

    [Option('o', "output", Required = true, HelpText = "Markdown 文件的输出路径。")]
    public override string? OutputFilePath { get; set; }
}

internal class CommonOptions
{
    #region Files
    [Option('f', "file", HelpText = "Word 文件的路径。")]
    public virtual string? WordFilePath { get; set; }

    [Option('o', "output", HelpText = "Markdown 文件的输出路径。")]
    public virtual string? OutputFilePath { get; set; }
    #endregion

    #region Front Matter
    [Option("author", HelpText = "文章作者。")]
    public string? Author { get; set; }

    [Option("title", HelpText = "文章标题。")]
    public string? Title { get; set; }
    
    [Option("short-title", HelpText = "文章标题。")]
    public string? ShortTitle { get; set; }

    [Option("icon-string", HelpText = "文章类型图标。")]
    public string? IconString { get; set; }

    [Option("date", HelpText = "以 'yyyy-MM-dd' 为格式的文章日期。")]
    public string? DateString { get; set; }

    [Option("category", HelpText = "文章类别。")]
    public IEnumerable<string>? Categories { get; set; }

    [Option("tag", HelpText = "文章标签。")]
    public IEnumerable<string>? Tag { get; set; }

    [Option("order", HelpText = "文章在本期期刊的顺序。")]
    public int Order { get; set; }

    [Option("description", HelpText = "文章的描述。")]
    public string? Description { get; set; }
    #endregion

    #region Editors Info
    [Option("editor", HelpText = "责任编辑名称。")]
    public string? EditorName { get; set; }

    [Option("website-designer", HelpText = "网页排版者名称。")]
    public string? WebsiteLayoutDesigner { get; set; }

    [Option("illustrator", HelpText = "绘图者名称。")]
    public string? Illustrator { get; set; }
    #endregion
}

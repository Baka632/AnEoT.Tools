using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using AnEoT.Tools.VolumeCreator.Views;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Models.Resources;
using System.Text;
using System.Collections.ObjectModel;
using AnEoT.Tools.Shared;
using AnEoT.Tools.Shared.Models;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using System.Globalization;
using Markdig;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class MarkdownEditViewModel : ObservableObject
{
    public MarkdownWrapper MarkdownWrapper { get; }
    public ObservableCollection<AssetNode> Assets { get; }
    public IVolumeResourcesHelper ResourcesHelper { get; }
    public bool ConvertWebP { get; set; }
    public Dictionary<string, FileNode> MarkdownImageUriToFileMapping { get; } = new(10);

    private readonly MarkdownEditPage view;

    [ObservableProperty]
    private string markdownString;
    [ObservableProperty]
    private string articleQuote = string.Empty;

    public MarkdownEditViewModel(MarkdownWrapper wrapper, MarkdownEditPage viewPage, ObservableCollection<AssetNode> assets, IVolumeResourcesHelper resourcesHelper, bool convertWebP)
    {
        markdownString = wrapper.Markdown;
        view = viewPage;
        ResourcesHelper = resourcesHelper;
        Assets = assets;
        MarkdownWrapper = wrapper;
        ConvertWebP = convertWebP;

        assets.CollectionChanged += (s, e) => InitializeImageFileMapping();
        InitializeImageFileMapping();
    }

    partial void OnMarkdownStringChanged(string value)
    {
        CommonValues.IsProjectSaved = false;
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
            XamlRoot = view.XamlRoot,
            PredefinedCategoryIndex = FrontMatterDialog.AvailablePredefinedCategories.IndexOf(MarkdownWrapper.CategoryInIndexPage)
        };

        bool shouldReplaceYaml = false;
        SourceSpan originalYamlSpan = default;
        MarkdownDocument document = Markdown.Parse(MarkdownString, CommonValues.MarkdownPipeline);
        YamlFrontMatterBlock? yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

        if (yamlBlock is not null)
        {
            string yaml = MarkdownString.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);

            if (FrontMatter.TryParse(yaml, out FrontMatter frontMatter))
            {
                shouldReplaceYaml = true;
                originalYamlSpan = yamlBlock.Span;

                dialog.ArticleTitle = frontMatter.Title;
                dialog.ArticleShortTitle = frontMatter.ShortTitle;
                dialog.IconString = frontMatter.Icon;
                dialog.Author = frontMatter.Author;
                dialog.Order = frontMatter.Order;
                dialog.Description = frontMatter.Description;
                dialog.IsArticle = frontMatter.Article;

                if (DateTimeOffset.TryParse(frontMatter.Date, CultureInfo.InvariantCulture, out DateTimeOffset date))
                {
                    dialog.ArticleDate = date;
                }
                if (frontMatter.Category is not null)
                {
                    dialog.Categories = new(frontMatter.Category.Select(str => new StringView(str)));
                }
                if (frontMatter.Tag is not null)
                {
                    dialog.Tags = new(frontMatter.Tag.Select(str => new StringView(str)));
                }
            }
        }

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            (FrontMatter frontMatter, PredefinedCategory? predefinedCategory) = dialog.Result;
            MarkdownWrapper.CategoryInIndexPage = predefinedCategory;
            string yamlHeader = MarkdownHelper.GetYamlHeaderString(frontMatter);

            int orderValueIndex = yamlHeader.IndexOf("order:");
            if (orderValueIndex != -1)
            {
                yamlHeader = yamlHeader.Insert(orderValueIndex, Environment.NewLine);
            }

            StringBuilder builder = new(MarkdownString);

            if (shouldReplaceYaml)
            {
                yamlHeader = yamlHeader.TrimEnd();
                builder.Remove(originalYamlSpan.Start, originalYamlSpan.Length);
            }
            builder.Insert(0, yamlHeader);

            MarkdownString = builder.ToString();
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
        string imageUri = ConstructImageUriByFileNode(fileNode);
        if (ConvertWebP)
        {
            imageUri = Path.ChangeExtension(imageUri, ".webp");
        }
        string markdownImageMark = $"![]({imageUri})";

        int position = textBox.SelectionStart;
        string markToInsert = $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{markdownImageMark}{Environment.NewLine}";
        MarkdownString = textBox.Text.Insert(position, markToInsert);
        textBox.Select(position + markToInsert.Length, 0);
    }

    private void InitializeImageFileMapping()
    {
        MarkdownImageUriToFileMapping.Clear();

        foreach (AssetNode node in Assets)
        {
            foreach (FileNode fileNode in DescendantsFileNode(node))
            {
                string imageUri = ConstructImageUriByFileNode(fileNode);
                MarkdownImageUriToFileMapping[imageUri] = fileNode;
            }
        }
    }

    private static string ConstructImageUriByFileNode(FileNode fileNode)
    {
        List<string> targetParts = new(3);
        AssetNode? parentNode = fileNode;
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
        return imageUri;
    }

    private static List<FileNode> DescendantsFileNode(AssetNode node)
    {
        List<FileNode> fileNodes = new(10);

        foreach (AssetNode item in node.Children)
        {
            if (item is FileNode fileNode)
            {
                fileNodes.Add(fileNode);
            }
            else if (item is FolderNode folderNode)
            {
                fileNodes.AddRange(DescendantsFileNode(folderNode));
            }
        }

        return fileNodes;
    }
}
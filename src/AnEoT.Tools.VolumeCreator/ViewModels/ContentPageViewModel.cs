using WinRT.Interop;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.Shared;
using AnEoT.Tools.VolumeCreator.Views;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp;
using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Extensions.Yaml;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Events;
using AnEoT.Tools.Shared.Models;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class ContentPageViewModel : ObservableValidator
{
    public ContentPageViewModel()
    {
        WordFiles.CollectionChanged += OnWordFilesCollectionChanged;
        ImageFiles.CollectionChanged += OnImagesFilesCollectionChanged;
        IndexMarkdown.CollectionChanged += OnIndexMarkdownCollectionChanged;
        InitializeImageFiles();
    }

    [RelayCommand]
    private async Task ExportVolume()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            string message = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));
            await ShowDialogAsync("无法导出，存在错误", message);
            return;
        }

        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);
        FolderPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        StorageFolder folder = await picker.PickSingleFolderAsync();

        if (folder != null)
        {
            if (Directory.Exists(Path.Combine(folder.Path, VolumeFolderName)))
            {
                ContentDialogResult result = await ShowDialogAsync("指定的文件夹内已经包含同名的期刊文件夹",
                                      "是否继续操作？如果继续，原文件夹中的内容将被清空。",
                                      "继续",
                                      closeText: "取消");

                if (result != ContentDialogResult.Primary)
                {
                    return;
                }
            }

            ShowTeachingTip("正在导出...", "请不要关闭应用。", false, TeachingTipPlacementMode.RightBottom,
                            new SymbolIconSource() { Symbol = Symbol.More });

            StorageFolder volumeFolder = await folder.CreateFolderAsync(VolumeFolderName, CreationCollisionOption.ReplaceExisting);

            await CreateResourcesFolder(volumeFolder);
            await SaveMarkdownContent(volumeFolder);

            IsShowTeachingTip = false;

            await ShowDialogAsync("导出成功",
                                      $"内容已导出在 {volumeFolder.Path} 中。",
                                      closeText: "确定");
        }
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        // TODO: 之前打开的项目没保存怎么办？

        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

        FileOpenPicker picker = new();
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add(".aneot-proj");
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

        StorageFile file = await picker.PickSingleFileAsync();
        await LoadProject(file);
    }

    public async Task LoadProject(StorageFile? file)
    {
        if (file is null)
        {
            return;
        }

        Stream stream = await file.OpenStreamForReadAsync();

        try
        {
            Project? project = await JsonSerializer.DeserializeAsync<Project>(stream, CommonValues.DefaultJsonSerializerOption);

            if (project is null)
            {
                ShowTeachingTip("无法打开工程文件", "文件可能无效或损坏。", false, TeachingTipPlacementMode.RightBottom,
                            new FontIconSource() { Glyph = "\uEA39" });
            }
            else
            {
                VolumeName = project.VolumeName ?? string.Empty;
                VolumeFolderName = project.VolumeFolderName ?? string.Empty;
                CoverFile = Path.Exists(project.CoverImagePath)
                    ? await StorageFile.GetFileFromPathAsync(project.CoverImagePath)
                    : null;
                await SetCoverByFile(CoverFile);

                ConvertToWebp = project.ImageConvertToWebp;
                IsCoverSizeFixed = project.IsCoverSizeFixed;

                WordFiles.CollectionChanged -= OnWordFilesCollectionChanged;
                ImageFiles.CollectionChanged -= OnImagesFilesCollectionChanged;
                IndexMarkdown.CollectionChanged -= OnIndexMarkdownCollectionChanged;

                WordFiles = project.WordFiles;
                ImageFiles = project.ImageFiles;
                IndexMarkdown = project.IndexMarkdown is null ? [] : [project.IndexMarkdown];

                OnPropertyChanged(nameof(ShowNotifyAddWordFile));
                OnPropertyChanged(nameof(ShowNotifyAddImagesFile));
                OnPropertyChanged(nameof(ShowNotifyGenerateIndex));

                WordFiles.CollectionChanged += OnWordFilesCollectionChanged;
                ImageFiles.CollectionChanged += OnImagesFilesCollectionChanged;
                IndexMarkdown.CollectionChanged += OnIndexMarkdownCollectionChanged;

                ProjectFile = file;
            }
        }
        catch (JsonException)
        {
            ShowTeachingTip("无法打开工程文件", "文件可能无效或损坏。", false, TeachingTipPlacementMode.RightBottom,
                            new FontIconSource() { Glyph = "\uEA39" });
        }
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        Project project = new(
            CoverFile?.Path, ConvertToWebp, IsCoverSizeFixed, VolumeFolderName, VolumeName, WordFiles, ImageFiles,
            IndexMarkdown.FirstOrDefault());

        StorageFile file;
        if (ProjectFile is not null)
        {
            file = ProjectFile;
            await SaveProjectCore(project, file);
            ShowTeachingTip("工程保存成功", string.Empty, true, TeachingTipPlacementMode.RightBottom,
                            new FontIconSource() { Glyph = "\uE73E" });
        }
        else
        {
            nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

            FileSavePicker picker = new();
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeChoices.Add("《回归线》网页版工程文件", [".aneot-proj"]);
            picker.SuggestedFileName = VolumeName;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            file = await picker.PickSaveFileAsync();

            if (file is null)
            {
                return;
            }

            await SaveProjectCore(project, file);
            await ShowDialogAsync("保存成功",
                                      $"工程文件已保存到 {file.Path}。",
                                      closeText: "确定");
        }
    }

    private static async Task SaveProjectCore(Project project, StorageFile file)
    {
        using Stream stream = await file.OpenStreamForWriteAsync();
        stream.SetLength(0);
        await JsonSerializer.SerializeAsync(stream, project, CommonValues.DefaultJsonSerializerOption);
    }

    private async Task SaveMarkdownContent(StorageFolder volumeFolder)
    {
        foreach (KeyValuePair<MarkdownWrapper, string> pair in GetOutputFileNameDictionary())
        {
            StorageFile file = await volumeFolder.CreateFileAsync(pair.Value, CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteTextAsync(file, pair.Key.Markdown);
        }

        if (IndexMarkdown.Count > 0 && IndexMarkdown[0] is not null)
        {
            MarkdownWrapper target = IndexMarkdown[0];

            StorageFile file = await volumeFolder.CreateFileAsync("README.md", CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteTextAsync(file, target.Markdown);
        }
    }

    private async Task CreateResourcesFolder(StorageFolder volumeFolder)
    {
        StorageFolder resourceFolder = await volumeFolder.CreateFolderAsync("res", CreationCollisionOption.ReplaceExisting);

        StorageFile target = await resourceFolder.CreateFileAsync("cover.webp");
        using ImageSharpImage image = await ImageSharpImage.LoadAsync(CoverFile!.Path);

        if (IsCoverSizeFixed)
        {
            image.Mutate(context => context.Resize(768, 1080));
        }

        using Stream targetStream = await target.OpenStreamForWriteAsync();
        await Task.Run(() => image.SaveAsWebp(targetStream)); // 防止卡主线程，ImageSharp 库自带的异步方法有点问题

        await CopyContentRecursively(ImageFiles.Single(), resourceFolder);
    }

    private async Task CopyContentRecursively(ImageListNode node, StorageFolder rootFolder)
    {
        if (node.Type == ImageListNodeType.Folder)
        {
            foreach (ImageListNode item in node.Children)
            {
                if (item.Type == ImageListNodeType.Folder)
                {
                    if (item.Children.Count > 0)
                    {
                        StorageFolder folder = await rootFolder.CreateFolderAsync(item.DisplayName, CreationCollisionOption.OpenIfExists);

                        foreach (ImageListNode subItem in item.Children)
                        {
                            await CopyContentRecursively(subItem, folder);
                        }
                    }
                }
                else if (item.Type == ImageListNodeType.File && item is FileNode fileNode)
                {
                    await SaveFileNode(fileNode, rootFolder);
                }
            }
        }
        else if (node.Type == ImageListNodeType.File && node is FileNode fileNode)
        {
            await SaveFileNode(fileNode, rootFolder);
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debugger.Break();
        }
#endif

        async Task SaveFileNode(FileNode fileNode, StorageFolder rootFolder)
        {
            if (ConvertToWebp && fileNode.File.ContentType != "image/webp")
            {
                using ImageSharpImage image = await ImageSharpImage.LoadAsync(fileNode.File.Path);
                StorageFile target = await rootFolder.CreateFileAsync(Path.ChangeExtension(fileNode.DisplayName, ".webp"));

                using Stream targetStream = await target.OpenStreamForWriteAsync();
                await Task.Run(() => image.SaveAsWebp(targetStream)); // 防止卡主线程，ImageSharp 库自带的异步方法有点问题
            }
            else
            {
                await fileNode.File.CopyAsync(rootFolder);
            }
        }
    }

    [RelayCommand]
    private async Task PickImage()
    {
        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

        FileOpenPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");

        StorageFile file = await picker.PickSingleFileAsync();
        await SetCoverByFile(file);
    }

    internal async Task SetCoverByFile(StorageFile? file)
    {
        if (file != null)
        {
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            try
            {
                BitmapImage bitmapImage = new();
                await bitmapImage.SetSourceAsync(stream);
                CoverFile = file;

                VolumeCover = bitmapImage;
                IsVolumeCoverError = false;
            }
            catch (COMException ex) when (ex.ErrorCode == -2003292336)
            {
                IsVolumeCoverError = true;
            }
        }
    }

    [RelayCommand]
    private async Task AddWordFileItem()
    {
        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

        FileOpenPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add(".docx");

        IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

        if (files is not null)
        {
            foreach (StorageFile file in files)
            {
                await AddSingleWordFileItem(file);
            }
        }
    }

    [RelayCommand]
    private void AddEmptyWordFileItem()
    {
        MarkdownWrapper emptyMarkdownFile = new("<自定义文件>", string.Empty, MarkdownWrapperType.Others);
        WordFiles.Add(emptyMarkdownFile);
    }

    [RelayCommand]
    private void AddPaintingWordFileItem()
    {
        MarkdownWrapper paintingMarkdownFile = new("<自定义文件>", string.Empty, MarkdownWrapperType.Paintings);
        WordFiles.Add(paintingMarkdownFile);
    }

    public async Task AddSingleWordFileItem(StorageFile file)
    {
        MarkdownWrapper toMarkdownFile;

        try
        {
            string markdown = await Task.Run(() => WordToMarkdownService.GetMarkdown(file.Path));
            toMarkdownFile = new(file.DisplayName, markdown);
            WordFiles.Add(toMarkdownFile);
        }
        catch (FileFormatException ex)
        {
            await ShowDialogAsync($"文件 {file.DisplayName} 不是有效的 DOCX 文件", $"错误信息：{ex.Message}");
        }
    }

    [RelayCommand]
    private void RemoveWordFileItem(MarkdownWrapper target)
    {
        WordFiles.Remove(target);
    }
    
    [RelayCommand]
    private void ViewWordFileItem(MarkdownWrapper wrapper)
    {
        MarkdownEditWindow window = new()
        {
            Model = (wrapper, ImageFiles, CoverFile),
            Title = $"{wrapper.DisplayName} - Markdown 编辑窗口"
        };
        window.Activate();
    }

    [RelayCommand]
    private static async Task AddImageFile(FolderNode node)
    {
        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

        FileOpenPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");

        IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

        if (files is not null)
        {
            foreach (StorageFile file in files)
            {
                if (!node.Children.Any(node => node is FileNode fileNode && fileNode.File.Path == file.Path))
                {
                    node.Children.Add(new FileNode(file, node));
                }
            }
        }
    }

    [RelayCommand]
    private static void RemoveImageFile(FileNode node)
    {
        node.Parent?.Children.Remove(node);
    }

    [RelayCommand]
    private void WordFileItemGoUp(MarkdownWrapper wrapper)
    {
        int currentItemIndex = WordFiles.IndexOf(wrapper);
        int upperIndex = currentItemIndex - 1;

        if (currentItemIndex == -1 || upperIndex < 0)
        {
            return;
        }
        WordFiles.Move(currentItemIndex, upperIndex);
    }

    [RelayCommand]
    private void WordFileItemGoDown(MarkdownWrapper wrapper)
    {
        int currentItemIndex = WordFiles.IndexOf(wrapper);
        int downIndex = currentItemIndex + 1;

        if (currentItemIndex == -1 || downIndex + 1 > WordFiles.Count)
        {
            return;
        }
        WordFiles.Move(currentItemIndex, downIndex);
    }

    [RelayCommand]
    private void GenerateIndexPage()
    {
        StringBuilder builder = new(700);
        builder.AppendLine("---");
        builder.AppendLine("icon: repo");
        builder.AppendLine("article: false");
        builder.AppendLine($"title: {VolumeName}");
        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine("![](./res/cover.webp) {.centering}");
        builder.AppendLine();
        builder.AppendLine("## 目录");
        builder.AppendLine();
        WriteIndexContent(builder);
        builder.AppendLine();
        builder.AppendLine("<FakeAds />");

        string markdown = builder.ToString();
        MarkdownWrapper wrapper = new("<自定义文件>", markdown, MarkdownWrapperType.Others, "README");

        if (IndexMarkdown.Count == 1)
        {
            IndexMarkdown[0] = wrapper;
        }
        else
        {
            IndexMarkdown.Add(wrapper);
        }
    }

    private void WriteIndexContent(StringBuilder builder)
    {
        Dictionary<MarkdownWrapper, string> outputFileNameMapping = GetOutputFileNameDictionary();

        if (WordFiles.Any(wrapper => wrapper.Type == MarkdownWrapperType.Intro))
        {
            builder.AppendLine("- [**卷首语**](intro.html)");
        }

        IEnumerable<IGrouping<PredefinedCategory, MarkdownWrapper>> articleGroups = WordFiles
            .Where(wrapper => wrapper.CategoryInIndexPage.HasValue)
            .GroupBy(wrapper => wrapper.CategoryInIndexPage!.Value);
        foreach (IGrouping<PredefinedCategory, MarkdownWrapper> articleGroup in articleGroups)
        {
            builder.AppendLine($"- **{articleGroup.Key.AsCategoryString(true)}**");
            foreach (MarkdownWrapper item in articleGroup)
            {
                string fileName = $"{outputFileNameMapping[item].Replace(".md", string.Empty)}.html";
                string title = GetTitleInMarkdown(item.Markdown) ?? item.DisplayName;

                builder.AppendLine($"  - [{title}]({fileName})");
            }
        }

        if (WordFiles.Any(wrapper => wrapper.Type == MarkdownWrapperType.Interview))
        {
            builder.AppendLine("- **创作者访谈**");
            foreach (MarkdownWrapper item in WordFiles.Where(wrapper => wrapper.Type == MarkdownWrapperType.Interview))
            {
                string title = GetTitleInMarkdown(item.Markdown) ?? item.DisplayName;
                builder.AppendLine($"  - [{title}](interview.html)");
            }
        }

        if (WordFiles.Any(wrapper => wrapper.Type == MarkdownWrapperType.OperatorSecret))
        {
            builder.AppendLine("- **干员秘闻**");
            foreach (MarkdownWrapper item in WordFiles.Where(wrapper => wrapper.Type == MarkdownWrapperType.OperatorSecret))
            {
                string title = GetTitleInMarkdown(item.Markdown) ?? item.DisplayName;
                builder.AppendLine($"  - [{title}](ope_sec.html)");
            }
        }

        foreach (MarkdownWrapper item in WordFiles.Where(item => item.Type == MarkdownWrapperType.Others))
        {
            builder.AppendLine($"- **{item.OutputTitle}**");
        }
    }

    private static string? GetTitleInMarkdown(string markdown)
    {
        MarkdownDocument document = Markdown.Parse(markdown, CommonValues.MarkdownPipeline);
        YamlFrontMatterBlock? yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

        if (yamlBlock is null)
        {
            return null;
        }

        string yaml = markdown.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);

        if (TryReadYaml(yaml, out FrontMatter result))
        {
            return result.Title;
        }
        else
        {
            return null;
        }
    }

    private Dictionary<MarkdownWrapper, string> GetOutputFileNameDictionary()
    {
        Dictionary<MarkdownWrapper, string> pairs = new(WordFiles.Count);

        int articleIndex = 1;
        int comicIndex = 1;

        foreach (MarkdownWrapper wrapper in WordFiles)
        {
            string saveFileName = string.Empty;

            switch (wrapper.Type)
            {
                case MarkdownWrapperType.Intro:
                    saveFileName = DetermineSaveName(wrapper, "intro.md");
                    break;
                case MarkdownWrapperType.Article:
                    if (string.IsNullOrWhiteSpace(wrapper.OutputTitle))
                    {
                        saveFileName = $"article{articleIndex}.md";
                        articleIndex++;
                    }
                    else
                    {
                        saveFileName = $"{wrapper.OutputTitle}.md";
                    }
                    break;
                case MarkdownWrapperType.Interview:
                    saveFileName = DetermineSaveName(wrapper, "interview.md");
                    break;
                case MarkdownWrapperType.Comic:
                    if (string.IsNullOrWhiteSpace(wrapper.OutputTitle))
                    {
                        saveFileName = $"comic{comicIndex}.md";
                        comicIndex++;
                    }
                    else
                    {
                        saveFileName = $"{wrapper.OutputTitle}.md";
                    }
                    break;
                case MarkdownWrapperType.OperatorSecret:
                    saveFileName = DetermineSaveName(wrapper, "ope_sec.md");
                    break;
                case MarkdownWrapperType.Paintings:
                    saveFileName = DetermineSaveName(wrapper, "paintings.md");
                    break;
                case MarkdownWrapperType.Others:
                    saveFileName = $"{wrapper.OutputTitle}.md";
                    break;
                default:
                    throw new NotImplementedException();
            }

            pairs[wrapper] = saveFileName;
        }

        return pairs;
    }

    private static string DetermineSaveName(MarkdownWrapper wrapper, string alterFileName)
    {
        if (string.IsNullOrWhiteSpace(wrapper.OutputTitle))
        {
            return alterFileName;
        }
        else
        {
            return $"{wrapper.OutputTitle}.md";
        }
    }

    /// <summary>
    /// 显示一个对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="message">要在对话框中显示的信息</param>
    /// <param name="primaryText">主按钮文本</param>
    /// <param name="secondaryText">第二按钮文本</param>
    /// <param name="closeText">关闭按钮文本</param>
    /// <returns>指示对话框结果的<seealso cref="ContentDialogResult"/></returns>
    private static async Task<ContentDialogResult> ShowDialogAsync(string title, string message, string? primaryText = null, string? secondaryText = null, string? closeText = null)
    {
        // null-coalescing 操作符——当 closeText 为空时才赋值
        closeText ??= "关闭";
        primaryText ??= string.Empty;
        secondaryText ??= string.Empty;

        ContentDialog dialog = new()
        {
            Title = title,
            Content = new ScrollViewer() { Content = message },
            PrimaryButtonText = primaryText,
            SecondaryButtonText = secondaryText,
            CloseButtonText = closeText,
            XamlRoot = (Application.Current as App)?.Window.Content.XamlRoot
        };

        return await dialog.ShowAsync();
    }

    /// <summary>
    /// 显示一个教学提示
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="subtitle">副标题</param>
    /// <param name="enableLightDismiss">是否启用单击关闭</param>
    /// <param name="preferredPlacement">提示的位置</param>
    private void ShowTeachingTip(string title, string subtitle, bool enableLightDismiss, TeachingTipPlacementMode preferredPlacement, IconSource icon)
    {
        IsShowTeachingTip = false;

        TeachingTipTitle = title;
        TeachingTipSubtitle = subtitle;
        IsTeachingTipLightDismissEnabled = enableLightDismiss;
        TeachingTipPreferredPlacement = preferredPlacement;
        TeachingTipIconSource = icon;

        IsShowTeachingTip = true;
    }

    [RequiresDynamicCode("此方法调用了不支持 IL 裁剪的 YamlDotNet.Serialization.DeserializerBuilder.DeserializerBuilder()")]
    public static bool TryReadYaml<T>(string yaml, [MaybeNullWhen(false)] out T result)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            result = default;
            return false;
        }

        T obj;
        try
        {
            StringReader input = new(yaml);
            Parser yamlParser = new(input);
            yamlParser.Consume<StreamStart>();
            yamlParser.Consume<DocumentStart>();

            IDeserializer yamlDes = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            obj = yamlDes.Deserialize<T>(yamlParser);
            yamlParser.Consume<DocumentEnd>();
        }
        catch
        {
            result = default;
            return false;
        }

        result = obj;
        return true;
    }
}
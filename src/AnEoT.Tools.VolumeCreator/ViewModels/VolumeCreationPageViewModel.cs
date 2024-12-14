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
using AnEoT.Tools.Shared.Models;
using SixLabors.ImageSharp.Processing;
using AnEoT.Tools.VolumeCreator.Models.Resources;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class VolumeCreationPageViewModel : ObservableValidator
{
    public VolumeCreationPage View { get; }

    public VolumeCreationPageViewModel(VolumeCreationPage view)
    {
        Articles.CollectionChanged += OnWordFilesCollectionChanged;
        Assets.CollectionChanged += OnImagesFilesCollectionChanged;
        IndexMarkdown.CollectionChanged += OnIndexMarkdownCollectionChanged;
        InitializeAssets();

        CommonValues.IsProjectSaved = true;
        View = view;
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

        if (!ResourcesHelper.ValidateAssets(Assets, out string? errorMessage))
        {
            ContentDialogResult result = await ShowDialogAsync("警告",
                                                               $"以下文件不存在，无法导出它们。{Environment.NewLine}要继续吗？{Environment.NewLine}{Environment.NewLine}{errorMessage.TrimEnd()}",
                                                               "继续操作",
                                                               closeText: "取消操作");

            if (result != ContentDialogResult.Primary)
            {
                return;
            }
        }

        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);
        FolderPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        StorageFolder folder = await picker.PickSingleFolderAsync();

        if (folder != null)
        {
            if (ResourcesHelper is ProjectPackageResourcesHelper pkgHelper)
            {
                await SaveProjectInternal(false);
            }

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

            await ExportAssets(volumeFolder);
            await ExportArticles(volumeFolder);

            IsShowTeachingTip = false;

            await ShowDialogAsync("导出成功",
                                      $"内容已导出在 {volumeFolder.Path} 中。",
                                      closeText: "确定");
        }
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        if (!CommonValues.IsProjectSaved)
        {
            ContentDialogResult result = await ShowDialogAsync("尚未保存当前的工程",
                                                               "要继续吗？未保存的更改将丢失。",
                                                               "继续",
                                                               closeText: "取消");
            if (result != ContentDialogResult.Primary)
            {
                return;
            }
        }

        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

        FileOpenPicker picker = new();
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add(".aneot-proj");
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

        StorageFile file = await picker.PickSingleFileAsync();
        await LoadProject(file);
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "已经保留了必需的类型信息")]
    public async Task LoadProject(StorageFile? file)
    {
        if (file is null)
        {
            return;
        }

        try
        {
            ProjectPackage projectPackage = await ProjectPackage.LoadAsync(file.Path);
            ProjectPackageResourcesHelper resHelper = new(projectPackage);

            bool containsError = false;
            StringBuilder stringBuilder = new();

            (VolumeName, VolumeFolderName, IsCoverSizeFixed, ConvertToWebp, _) = resHelper.ProjectPackage.Info;
            Stream? coverStream = await resHelper.GetCoverAsync();
            VolumeCover = null;
            if (coverStream != null)
            {
                using (coverStream)
                {
                    await SetCoverByStream(coverStream);
                }

                if (IsVolumeCoverError)
                {
                    resHelper.ProjectPackage.RemoveCoverFile();
                    containsError = true;
                    stringBuilder.AppendLine("【期刊封面】");
                    stringBuilder.AppendLine($"项目文件内的期刊封面不是可解码的图像。");
                    stringBuilder.AppendLine();
                }
            }

            Articles.CollectionChanged -= OnWordFilesCollectionChanged;
            Assets.CollectionChanged -= OnImagesFilesCollectionChanged;
            IndexMarkdown.CollectionChanged -= OnIndexMarkdownCollectionChanged;

            Articles = new(resHelper.ProjectPackage.Articles);
            Assets = new(resHelper.ProjectPackage.Assets);
            IndexMarkdown = resHelper.ProjectPackage.IndexMarkdown is null ? [] : [resHelper.ProjectPackage.IndexMarkdown];

            if (!resHelper.ValidateAssets(Assets, out string? msg))
            {
                containsError = true;
                stringBuilder.AppendLine("【资源文件】");
                stringBuilder.AppendLine("找不到以下文件：");
                stringBuilder.AppendLine(msg.TrimEnd());
            }

            if (containsError)
            {
                ShowTeachingTip("加载项目时出现以下问题", stringBuilder.ToString().TrimEnd(), false, TeachingTipPlacementMode.RightBottom,
                        new FontIconSource() { Glyph = "\uE7BA" });
            }

            OnPropertyChanged(nameof(ShowNotifyAddArticles));
            OnPropertyChanged(nameof(ShowNotifyAddAssets));
            OnPropertyChanged(nameof(ShowNotifyGenerateIndex));

            Articles.CollectionChanged += OnWordFilesCollectionChanged;
            Assets.CollectionChanged += OnImagesFilesCollectionChanged;
            IndexMarkdown.CollectionChanged += OnIndexMarkdownCollectionChanged;

            if (ResourcesHelper is ProjectPackageResourcesHelper oldPkgHelper)
            {
                await oldPkgHelper.DisposeAsync();
            }
            ResourcesHelper = resHelper;

            await Task.Delay(200);
            CommonValues.IsProjectSaved = true;
        }
        catch (InvalidDataException ex)
        {
            string message;
            if (ex.InnerException?.HResult == -2147024864)
            {
                message = "文件正在被其他进程使用。";
            }
            else
            {
                try
                {
                    Stream stream = await file.OpenStreamForReadAsync();
#pragma warning disable CS0618 // 类型或成员已过时——这是为了兼容性用途。
                    CompatibilityProject? project;
                    using (stream)
                    {
                        project = await JsonSerializer.DeserializeAsync<CompatibilityProject>(stream, CommonValues.DefaultJsonSerializerOption);
                    }
#pragma warning restore CS0618

                    if (project != null)
                    {
                        ContentDialogResult result = await ShowDialogAsync("此项目文件使用的是不再受支持的旧格式",
                                                                           "应用只能读取新格式的项目文件。\n不过我们可以尝试将此文件转换为新格式，要转换吗？",
                                                                           "转换", closeText: "不转换");

                        if (result == ContentDialogResult.Primary)
                        {
                            try
                            {
                                ProjectPackage projectPackage = ProjectPackage.Create(file.Path);
                                projectPackage.Info = new ProjectInfo(project.VolumeName, project.VolumeFolderName, project.IsCoverSizeFixed, project.ImageConvertToWebp, null);
                                projectPackage.Articles = project.WordFiles;
                                projectPackage.Assets = project.ImageFiles;
                                projectPackage.IndexMarkdown = project.IndexMarkdown;

                                if (File.Exists(project.CoverImagePath))
                                {
                                    await projectPackage.SetCoverFileAsync(project.CoverImagePath);
                                }
                                await projectPackage.DisposeAsync();
                            }
                            catch (Exception convertEx)
                            {
                                await ShowDialogAsync("转换过程中出现了异常", convertEx.Message, closeText: "关闭");
                                return;
                            }

                            await LoadProject(file);
                        }

                        return;
                    }
                }
                catch (JsonException)
                {
                }
                message = "文件可能无效或损坏。";
            }

            ShowTeachingTip("无法打开工程文件", message, false, TeachingTipPlacementMode.RightBottom,
                            new FontIconSource() { Glyph = "\uEA39" });
        }
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        await SaveProjectInternal();
    }

    private async Task SaveProjectInternal(bool showSavingTip = true)
    {
        if (ResourcesHelper is ProjectPackageResourcesHelper pkgHelper)
        {
            await SaveProjectCore(pkgHelper.ProjectPackage);

            if (showSavingTip)
            {
                ShowTeachingTip("工程保存成功", string.Empty, true, TeachingTipPlacementMode.RightBottom,
                            new FontIconSource() { Glyph = "\uE73E" });
            }
        }
        else if (ResourcesHelper is MemoryResourcesHelper memoryHelper)
        {
            nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

            FileSavePicker picker = new();
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeChoices.Add("《回归线》网页版工程文件", [".aneot-proj"]);
            picker.SuggestedFileName = VolumeName;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            StorageFile file = await picker.PickSaveFileAsync();

            if (file is null)
            {
                return;
            }

            ProjectPackage projectPackage = ProjectPackage.Create(file.Path);
            InitializeProjectPackage(projectPackage);
            pkgHelper = await memoryHelper.ToProjectBasedResourcesHelperAsync(projectPackage);
            ResourcesHelper = pkgHelper;

            await SaveProjectCore(projectPackage);
            await ShowDialogAsync("保存成功",
                                      $"工程文件已保存到 {file.Path}。",
                                      closeText: "确定");
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debugger.Break();
        }
#endif

        async Task SaveProjectCore(ProjectPackage projectPackage)
        {
            InitializeProjectPackage(projectPackage);
            // 封面图像不在这里设置
            await projectPackage.SaveAsync();
            CommonValues.IsProjectSaved = true;
        }
    }

    private void InitializeProjectPackage(ProjectPackage projectPackage)
    {
        projectPackage.Info = projectPackage.Info with
        {
            VolumeName = this.VolumeName,
            VolumeFolderName = this.VolumeFolderName,
            IsCoverSizeFixed = this.IsCoverSizeFixed,
            ImageConvertToWebp = this.ConvertToWebp,
        };
        projectPackage.IndexMarkdown = IndexMarkdown.FirstOrDefault();
        projectPackage.Articles = Articles;
        projectPackage.Assets = Assets;
    }

    private async Task ExportArticles(StorageFolder volumeFolder)
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

    private async Task ExportAssets(StorageFolder volumeFolder)
    {
        StorageFolder resourceFolder = await volumeFolder.CreateFolderAsync("res", CreationCollisionOption.ReplaceExisting);
        await ResourcesHelper.ExportAssetsAsync(Assets, resourceFolder);

        StorageFile target = await resourceFolder.CreateFileAsync("cover.webp");
        Stream? coverImageStream = await ResourcesHelper.GetCoverAsync();
        using ImageSharpImage image = await ImageSharpImage.LoadAsync(coverImageStream!);

        if (IsCoverSizeFixed)
        {
            image.Mutate(context => context.Resize(768, 1080));
        }

        using Stream targetStream = await target.OpenStreamForWriteAsync();
        await Task.Run(() => image.SaveAsWebp(targetStream)); // 防止卡主线程，ImageSharp 库自带的异步方法有点问题
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
            using IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            try
            {
                BitmapImage bitmapImage = new();
                await bitmapImage.SetSourceAsync(stream);
                await ResourcesHelper.SetCoverAsync(file.Path);

                VolumeCover = bitmapImage;
                IsVolumeCoverError = false;
            }
            catch (COMException ex) when (ex.ErrorCode == -2003292336)
            {
                IsVolumeCoverError = true;
            }
        }
    }

    private async Task SetCoverByStream(Stream? dotnetStream)
    {
        if (dotnetStream != null)
        {
            using IRandomAccessStream stream = dotnetStream.AsRandomAccessStream();
            try
            {
                BitmapImage bitmapImage = new();
                await bitmapImage.SetSourceAsync(stream);

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
    private async Task ImportWordFileItem()
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
                await ImportSingleWordFileItem(file);
            }
        }
    }

    [RelayCommand]
    private void AddEmptyArticle()
    {
        MarkdownWrapper emptyMarkdownFile = new("<自定义文章>", string.Empty, MarkdownWrapperType.Others);
        Articles.Add(emptyMarkdownFile);
    }

    [RelayCommand]
    private void AddPaintingArticle()
    {
        // TODO: 会有吗？
        MarkdownWrapper paintingMarkdownFile = new("<自定义文件>", string.Empty, MarkdownWrapperType.Paintings);
        Articles.Add(paintingMarkdownFile);
    }

    public async Task ImportSingleWordFileItem(StorageFile file)
    {
        MarkdownWrapper toMarkdownFile;

        try
        {
            string markdown = await Task.Run(() => WordToMarkdownService.GetMarkdown(file.Path));
            toMarkdownFile = new(file.DisplayName, markdown);
            Articles.Add(toMarkdownFile);
        }
        catch (FileFormatException ex)
        {
            await ShowDialogAsync($"文件 {file.DisplayName} 不是有效的 DOCX 文件", $"错误信息：{ex.Message}");
        }
    }

    [RelayCommand]
    private void RemoveArticle(MarkdownWrapper target)
    {
        Articles.Remove(target);
    }

    [RelayCommand]
    private void ViewArticle(MarkdownWrapper wrapper)
    {
        MarkdownEditWindow window = new()
        {
            Model = (wrapper, Assets, ResourcesHelper, ConvertToWebp),
            Title = $"{wrapper.DisplayName} - Markdown 编辑窗口"
        };
        window.Activate();
    }

    [RelayCommand]
    private async Task AddAsset(FolderNode? parentNode)
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
            ObservableCollection<AssetNode> target = parentNode is null ? Assets : parentNode.Children;

            foreach (StorageFile file in files)
            {
                if (!target.Any(node => node is FileNode fileNode && fileNode.FilePath == file.Path))
                {
                    target.Add(new FileNode(file, parentNode));
                    CommonValues.IsProjectSaved = false;
                }
            }
        }
    }

    [RelayCommand]
    private void RemoveAsset(FileNode node)
    {
        if (node.Parent is null)
        {
            Assets.Remove(node);
        }
        else
        {
            node.Parent?.Children.Remove(node);
        }
        CommonValues.IsProjectSaved = false;
    }

    [RelayCommand]
    private static async Task RepairAsset(FileNode node)
    {
        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);

        FileOpenPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".webp");

        StorageFile? file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        node.FilePath = file.Path;
        CommonValues.IsProjectSaved = false;
    }

    [RelayCommand]
    private async Task AddAssetFolder(FolderNode? parentNode)
    {
        NewAssetFolderDialog dialog = new()
        {
            XamlRoot = View.XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ObservableCollection<AssetNode> target = parentNode is null ? Assets : parentNode.Children;

            string folderName = dialog.NewFolderName;
            if (!target.Any(node => node.DisplayName == folderName))
            {
                target.Add(new FolderNode(folderName, parentNode));
                CommonValues.IsProjectSaved = false;
            }
        }
    }

    [RelayCommand]
    private void RemoveAssetFolder(FolderNode node)
    {
        if (node.Parent is null)
        {
            Assets.Remove(node);
        }
        else
        {
            node.Parent?.Children.Remove(node);
        }
        CommonValues.IsProjectSaved = false;
    }

    [RelayCommand]
    private void ArticleGoUp(MarkdownWrapper wrapper)
    {
        int currentItemIndex = Articles.IndexOf(wrapper);
        int upperIndex = currentItemIndex - 1;

        if (currentItemIndex == -1 || upperIndex < 0)
        {
            return;
        }
        Articles.Move(currentItemIndex, upperIndex);
    }

    [RelayCommand]
    private void ArticleGoDown(MarkdownWrapper wrapper)
    {
        int currentItemIndex = Articles.IndexOf(wrapper);
        int downIndex = currentItemIndex + 1;

        if (currentItemIndex == -1 || downIndex + 1 > Articles.Count)
        {
            return;
        }
        Articles.Move(currentItemIndex, downIndex);
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

        if (Articles.Any(wrapper => wrapper.Type == MarkdownWrapperType.Intro))
        {
            builder.AppendLine("- [**卷首语**](intro.html)");
        }

        IEnumerable<IGrouping<PredefinedCategory, MarkdownWrapper>> articleGroups = Articles
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

        if (Articles.Any(wrapper => wrapper.Type == MarkdownWrapperType.Interview))
        {
            builder.AppendLine("- **创作者访谈**");
            foreach (MarkdownWrapper item in Articles.Where(wrapper => wrapper.Type == MarkdownWrapperType.Interview))
            {
                string title = GetTitleInMarkdown(item.Markdown) ?? item.DisplayName;
                builder.AppendLine($"  - [{title}](interview.html)");
            }
        }

        if (Articles.Any(wrapper => wrapper.Type == MarkdownWrapperType.OperatorSecret))
        {
            builder.AppendLine("- **干员秘闻**");
            foreach (MarkdownWrapper item in Articles.Where(wrapper => wrapper.Type == MarkdownWrapperType.OperatorSecret))
            {
                string title = GetTitleInMarkdown(item.Markdown) ?? item.DisplayName;
                builder.AppendLine($"  - [{title}](ope_sec.html)");
            }
        }

        foreach (MarkdownWrapper item in Articles.Where(item => item.Type == MarkdownWrapperType.Others))
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

        if (FrontMatter.TryParse(yaml, out FrontMatter result))
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
        Dictionary<MarkdownWrapper, string> pairs = new(Articles.Count);

        int articleIndex = 1;
        int comicIndex = 1;

        foreach (MarkdownWrapper wrapper in Articles)
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
}
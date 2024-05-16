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

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class ContentPageViewModel : ObservableValidator
{
    public ContentPageViewModel()
    {
        WordFiles.CollectionChanged += OnWordFilesCollectionChanged;
        ImageFiles.CollectionChanged += OnImagesFilesCollectionChanged;
        InitializeImageFiles();
    }

    [RelayCommand]
    private async Task SaveVolume()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            string message = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));
            await ShowDialogAsync("无法保存，存在错误", message);
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

            StorageFolder volumeFolder = await folder.CreateFolderAsync(VolumeFolderName, CreationCollisionOption.ReplaceExisting);

            await CreateResourcesFolder(volumeFolder);
            await SaveMarkdownContent(volumeFolder);
        }
    }

    private async Task SaveMarkdownContent(StorageFolder volumeFolder)
    {
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

            StorageFile file = await volumeFolder.CreateFileAsync(saveFileName, CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteTextAsync(file, wrapper.Markdown);
        }

        static string DetermineSaveName(MarkdownWrapper wrapper, string alterFileName)
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
    }

    private async Task CreateResourcesFolder(StorageFolder volumeFolder)
    {
        StorageFolder resourceFolder = await volumeFolder.CreateFolderAsync("res", CreationCollisionOption.ReplaceExisting);

        await CoverFile!.CopyAsync(resourceFolder);

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
        MarkdownWrapper emptyMarkdownFile = new(null, string.Empty, MarkdownWrapperType.Others);
        WordFiles.Add(emptyMarkdownFile);
    }

    [RelayCommand]
    private void AddPaintingWordFileItem()
    {
        MarkdownWrapper paintingMarkdownFile = new(null, string.Empty, MarkdownWrapperType.Paintings);
        WordFiles.Add(paintingMarkdownFile);
    }

    public async Task AddSingleWordFileItem(StorageFile file)
    {
        if (WordFiles.Any(wrapper => wrapper?.File?.Path == file.Path))
        {
            return;
        }

        MarkdownWrapper toMarkdownFile;

        try
        {
            string markdown = await Task.Run(() => WordToMarkdownService.GetMarkdown(file.Path));
            toMarkdownFile = new(file, markdown);
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
            Model = (wrapper, ImageFiles),
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
}

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

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class ContentPageViewModel : ObservableValidator
{
    private StorageFolder? _targetFolder = null;

    public ContentPageViewModel()
    {
        WordFiles.CollectionChanged += OnWordFilesCollectionChanged;
        ImageFiles.CollectionChanged += OnImagesFilesCollectionChanged;
        InitializeImageFiles();
    }

    [RelayCommand]
    private async Task SaveVolume()
    {
        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);
        FolderPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        StorageFolder folder = await picker.PickSingleFolderAsync();

        if (folder != null)
        {
            _targetFolder = folder;
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
            await SetCoverByStream(stream);
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

    public async Task AddSingleWordFileItem(StorageFile file)
    {
        if (WordFiles.Any(wrapper => wrapper.File.Path == file.Path))
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
    private static void ViewWordFileItem(MarkdownWrapper wrapper)
    {
        MarkdownEditWindow window = new()
        {
            Model = wrapper,
            Title = $"{wrapper.File.DisplayName} - Markdown 编辑窗口"
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

    internal async Task SetCoverByStream(IRandomAccessStream stream)
    {
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
            Content = message,
            PrimaryButtonText = primaryText,
            SecondaryButtonText = secondaryText,
            CloseButtonText = closeText,
            XamlRoot = (Application.Current as App)?.Window.Content.XamlRoot
        };

        return await dialog.ShowAsync();
    }
}

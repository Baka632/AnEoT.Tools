﻿using WinRT.Interop;
using Windows.Storage.Pickers;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.Shared;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class ContentPageViewModel : ObservableValidator
{
    private StorageFolder? _targetFolder = null;

    public ContentPageViewModel()
    {
        WordFiles.CollectionChanged += OnWordFilesCollectionChanged;
    }

    [RelayCommand]
    private async Task OpenTargetFolder()
    {
        nint hwnd = WindowNative.GetWindowHandle((Application.Current as App)?.Window);
        FolderPicker picker = new();

        InitializeWithWindow.Initialize(picker, hwnd);

        StorageFolder folder = await picker.PickSingleFolderAsync();

        if (folder != null)
        {
            _targetFolder = folder;
            ShowContent = true;
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
                string markdown = await Task.Run(() => WordToMarkdownService.GetMarkdown(file.Path));
                MarkdownWarpper toMarkdownFile = new(file, markdown);
                WordFiles.Add(toMarkdownFile);
            }
        }
    }

    [RelayCommand]
    private void RemoveWordFileItem(MarkdownWarpper target)
    {
        WordFiles.Remove(target);
    }
    
    [RelayCommand]
    private static async Task ViewWordFileItem(MarkdownWarpper target)
    {
        await ShowDialogAsync(target.File.DisplayName, target.Markdown[..20]);
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
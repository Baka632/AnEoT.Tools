using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.UI.Xaml.Media.Imaging;
using AnEoT.Tools.VolumeCreator.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

partial class ContentPageViewModel
{
    public bool IsVolumeCoverNotExist => VolumeCover is null;
    public VerticalAlignment CoverImageVerticalAlignmentMode => VolumeCover is null ? VerticalAlignment.Stretch : VerticalAlignment.Top;
    public bool ShowNotifyAddWordFile => WordFiles.Count <= 0;
    public bool ShowNotifyAddImagesFile => ImageFiles.Count <= 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVolumeCoverNotExist))]
    [NotifyPropertyChangedFor(nameof(CoverImageVerticalAlignmentMode))]
    private BitmapImage? _volumeCover;
    [ObservableProperty]
    private bool _isVolumeCoverError;

    [ObservableProperty]
    [Required, NotifyDataErrorInfo]
    [CustomValidation(typeof(ContentPageViewModel), nameof(ValidateVolumeFolderName))]
    private string _volumeFolderName = string.Empty;
    [ObservableProperty]
    [Required, NotifyDataErrorInfo]
    private string _volumeName = string.Empty;
    [ObservableProperty]
    [Required, NotifyDataErrorInfo]
    [CustomValidation(typeof(ContentPageViewModel), nameof(ValidateWordFiles))]
    private ObservableCollection<MarkdownWrapper> wordFiles = [];
    [ObservableProperty]
    [Required]
    private ObservableCollection<ImageListNode> imageFiles = [];

    private void InitializeImageFiles()
    {
        FolderNode root = new("res", null);
        root.Children.Add(new FolderNode("comic", root));
        root.Children.Add(new FolderNode("illustration", root));
        root.Children.Add(new FolderNode("ope_sec", root));
        root.Children.Add(new FolderNode("interview", root));

        ImageFiles.Add(root);
    }

    private void OnWordFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowNotifyAddWordFile));
    }

    private void OnImagesFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowNotifyAddImagesFile));
    }

    public static ValidationResult ValidateVolumeFolderName(string folderName)
    {
        if (DateOnly.TryParse(folderName, CultureInfo.InvariantCulture, out _))
        {
            return ValidationResult.Success!;
        }
        else
        {
            return new ValidationResult("期刊名称格式无效");
        }
    }

    public static ValidationResult ValidateWordFiles(ObservableCollection<MarkdownWrapper> files)
    {
        if (files.Count > 0)
        {
            return ValidationResult.Success!;
        }
        else
        {
            return new ValidationResult("没有导入 Word 文件");
        }
    }
}

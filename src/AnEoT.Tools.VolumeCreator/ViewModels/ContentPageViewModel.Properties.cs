using Microsoft.UI.Xaml.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AnEoT.Tools.VolumeCreator.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

partial class ContentPageViewModel
{
    public bool IsVolumeCoverNotExist => VolumeCover is null;
    public VerticalAlignment CoverImageVerticalAlignmentMode => VolumeCover is null ? VerticalAlignment.Stretch : VerticalAlignment.Top;
    public bool ShowNotifyAddWordFile => WordFiles.Count <= 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVolumeCoverNotExist))]
    [NotifyPropertyChangedFor(nameof(CoverImageVerticalAlignmentMode))]
    private BitmapImage? _volumeCover;
    [ObservableProperty]
    private bool _showContent;
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
    private ObservableCollection<MarkdownWarpper> wordFiles = [];

    private void OnWordFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowNotifyAddWordFile));
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

    public static ValidationResult ValidateWordFiles(ObservableCollection<MarkdownWarpper> files)
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

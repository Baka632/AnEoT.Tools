using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.UI.Xaml.Media.Imaging;
using AnEoT.Tools.VolumeCreator.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Windows.Storage;

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
    private bool convertToWebp = true;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ContentPageViewModel), nameof(ValidateVolumeFolderName))]
    private string _volumeFolderName = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(ContentPageViewModel), nameof(ValidateVolumeDisplayName))]
    private string _volumeName = string.Empty;
    [ObservableProperty, NotifyDataErrorInfo]
    [CustomValidation(typeof(ContentPageViewModel), nameof(ValidateCoverFile))]
    private StorageFile? _coverFile;
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

    public static ValidationResult ValidateVolumeDisplayName(string volumeName)
    {
        if (string.IsNullOrWhiteSpace(volumeName))
        {
            return new ValidationResult("【期刊正式名称】未填写期刊名称。");
        }
        else if (volumeName.Length < 3)
        {
            return new ValidationResult("【期刊正式名称】期刊名称太短。");
        }
        else
        {
            return ValidationResult.Success!;
        }
    }

    public static ValidationResult ValidateVolumeFolderName(string folderName)
    {
        if (DateOnly.TryParse(folderName, CultureInfo.InvariantCulture, out _))
        {
            return ValidationResult.Success!;
        }
        else
        {
            return new ValidationResult("【期刊文件夹名称】期刊名称格式无效，正确的格式形如“2024-05”。");
        }
    }

    public static ValidationResult ValidateWordFiles(ObservableCollection<MarkdownWrapper> files)
    {
        if (files.Count > 0)
        {
            if (files.Any(wrapper => wrapper.Type == MarkdownWrapperType.Others && string.IsNullOrWhiteSpace(wrapper.OutputTitle)))
            {
                return new ValidationResult("【DOCX 文件列表】列表中的自定义项应当填写导出文件名。");
            }

            IEnumerable<MarkdownWrapper> fileWithOutputNames = files.Where(IsWrapperHasOutputTitle);
            IEnumerable<MarkdownWrapper> distincedSequence = fileWithOutputNames.DistinctBy(wrapper => wrapper.OutputTitle);

            if (fileWithOutputNames.SequenceEqual(distincedSequence) != true)
            {
                return new ValidationResult("【DOCX 文件列表】存在导出文件名重复的项。");
            }

            return ValidationResult.Success!;
        }
        else
        {
            return new ValidationResult("【DOCX 文件列表】没有导入任何文件，无法进行操作。");
        }

        static bool IsWrapperHasOutputTitle(MarkdownWrapper wrapper)
        {
            return (wrapper.Type == MarkdownWrapperType.Others)
                || (wrapper.Type != MarkdownWrapperType.Others && !string.IsNullOrWhiteSpace(wrapper.OutputTitle));
        }
    }
    
    public static ValidationResult ValidateCoverFile(StorageFile file)
    {
        if (file is null)
        {
            return new ValidationResult("【期刊封面图片】没有添加封面图片，无法进行操作。");
        }
        else
        {
            return ValidationResult.Success!;
        }
    }
}

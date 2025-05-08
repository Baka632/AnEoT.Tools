using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.UI.Xaml.Media.Imaging;
using AnEoT.Tools.VolumeCreator.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using AnEoT.Tools.VolumeCreator.Models.Resources;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

partial class VolumeCreationPageViewModel
{
    public bool IsVolumeCoverNotExist => VolumeCover is null;
    public VerticalAlignment CoverImageVerticalAlignmentMode => VolumeCover is null ? VerticalAlignment.Stretch : VerticalAlignment.Top;
    public bool ShowNotifyAddArticles => Articles.Count <= 0;
    public bool ShowNotifyAddAssets => Assets.Count <= 0;
    public bool ShowNotifyGenerateIndex => IndexMarkdown.Count <= 0;
    [Required]
    [CustomValidation(typeof(VolumeCreationPageViewModel), nameof(ValidateResourcesHelper))]
    public IVolumeResourcesHelper ResourcesHelper
    {
        get => resourcesHelper;
        set
        {
            resourcesHelper = value;
            ResourcesHelperForValidation = value;
        }
    }
    public static IVolumeResourcesHelper ResourcesHelperForValidation { get; set; } = new MemoryResourcesHelper();

    private IVolumeResourcesHelper resourcesHelper;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVolumeCoverNotExist))]
    [NotifyPropertyChangedFor(nameof(CoverImageVerticalAlignmentMode))]
    private BitmapImage? _volumeCover;
    [ObservableProperty]
    private bool _isVolumeCoverError;

    [ObservableProperty]
    private bool isShowTeachingTip;
    [ObservableProperty]
    private bool isTeachingTipLightDismissEnabled;
    [ObservableProperty]
    private TeachingTipPlacementMode teachingTipPreferredPlacement;
    [ObservableProperty]
    private string? teachingTipTitle;
    [ObservableProperty]
    private string? teachingTipSubtitle;
    [ObservableProperty]
    private IconSource? teachingTipIconSource;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(VolumeCreationPageViewModel), nameof(ValidateVolumeFolderName))]
    private string _volumeFolderName = string.Empty;
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(VolumeCreationPageViewModel), nameof(ValidateVolumeDisplayName))]
    private string _volumeName = string.Empty;
    [ObservableProperty]
    private bool convertToWebp = true;
    [ObservableProperty]
    private bool isCoverSizeFixed = true;
    [ObservableProperty]
    [Required, NotifyDataErrorInfo]
    [CustomValidation(typeof(VolumeCreationPageViewModel), nameof(ValidateArticles))]
    private ObservableCollection<MarkdownWrapper> articles = [];
    [ObservableProperty, Required, NotifyDataErrorInfo]
    [CustomValidation(typeof(VolumeCreationPageViewModel), nameof(ValidateAssets))]
    private ObservableCollection<AssetNode> assets = [];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNotifyGenerateIndex))]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(VolumeCreationPageViewModel), nameof(ValidateIndexMarkdown))]
    private ObservableCollection<MarkdownWrapper> indexMarkdown = [];

    private void InitializeAssets()
    {
        FolderNode root = new("res", null);
        root.Children.Add(new FolderNode("comic", root));
        root.Children.Add(new FolderNode("illustration", root));
        root.Children.Add(new FolderNode("ope_sec", root));
        root.Children.Add(new FolderNode("interview", root));

        Assets.Add(root);
    }

    #region Save State Changer
    partial void OnVolumeCoverChanged(BitmapImage? value)
    {
        CommonValues.IsProjectSaved = false;
    }

    partial void OnConvertToWebpChanged(bool value)
    {
        CommonValues.IsProjectSaved = false;
    }

    partial void OnIsCoverSizeFixedChanged(bool value)
    {
        CommonValues.IsProjectSaved = false;
    }

    partial void OnVolumeFolderNameChanged(string value)
    {
        CommonValues.IsProjectSaved = false;
    }

    partial void OnVolumeNameChanged(string value)
    {
        CommonValues.IsProjectSaved = false;
    }

    partial void OnArticlesChanged(ObservableCollection<MarkdownWrapper> value)
    {
        CommonValues.IsProjectSaved = false;
    }

    partial void OnAssetsChanged(ObservableCollection<AssetNode> value)
    {
        CommonValues.IsProjectSaved = false;
    }

    partial void OnIndexMarkdownChanged(ObservableCollection<MarkdownWrapper> value)
    {
        CommonValues.IsProjectSaved = false;
    }

    private void OnWordFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CommonValues.IsProjectSaved = false;
        OnPropertyChanged(nameof(ShowNotifyAddArticles));
    }

    private void OnImagesFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CommonValues.IsProjectSaved = false;
        OnPropertyChanged(nameof(ShowNotifyAddAssets));
    }

    private void OnIndexMarkdownCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CommonValues.IsProjectSaved = false;
        OnPropertyChanged(nameof(ShowNotifyGenerateIndex));
    }
    #endregion

    public static ValidationResult ValidateResourcesHelper(IVolumeResourcesHelper helper)
    {
        if (!helper.HasCover)
        {
            return new ValidationResult("【期刊封面图片】没有添加封面图片，无法进行操作。");
        }
        else
        {
            return ValidationResult.Success!;
        }
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

    public static ValidationResult ValidateArticles(ObservableCollection<MarkdownWrapper> files)
    {
        if (files.Count > 0)
        {
            if (files.Any(wrapper => wrapper.Type == MarkdownWrapperType.Others && string.IsNullOrWhiteSpace(wrapper.OutputTitle)))
            {
                return new ValidationResult("【文章列表】列表中的自定义项应当填写导出文件名。");
            }

            IEnumerable<MarkdownWrapper> fileWithOutputNames = files.Where(IsWrapperHasOutputTitle);
            IEnumerable<MarkdownWrapper> distincedSequence = fileWithOutputNames.DistinctBy(wrapper => wrapper.OutputTitle);

            if (fileWithOutputNames.SequenceEqual(distincedSequence) != true)
            {
                return new ValidationResult("【文章列表】存在导出文件名重复的项。");
            }

            return ValidationResult.Success!;
        }
        else
        {
            return new ValidationResult("【文章列表】没有导入任何文件，无法进行操作。");
        }

        static bool IsWrapperHasOutputTitle(MarkdownWrapper wrapper)
        {
            return (wrapper.Type == MarkdownWrapperType.Others)
                || (wrapper.Type != MarkdownWrapperType.Others && !string.IsNullOrWhiteSpace(wrapper.OutputTitle));
        }
    }

    public static ValidationResult ValidateAssets(ObservableCollection<AssetNode> nodes)
    {
        if (ResourcesHelperForValidation.ValidateAssets(nodes, out string? message))
        {
            return ValidationResult.Success!;
        }
        else
        {
            return new ValidationResult($"【资源文件】以下文件不存在\n{message.Trim()}");
        }
    }

    public static ValidationResult ValidateIndexMarkdown(ObservableCollection<MarkdownWrapper> wrapper)
    {
#if DEBUG
        if (wrapper.Count > 1)
        {
            System.Diagnostics.Debugger.Break();
        }
#endif

        if (wrapper.Count <= 0)
        {
            return new ValidationResult("【目录页】尚未生成目录页。");
        }
        else
        {
            return ValidationResult.Success!;
        }
    }
}

using Microsoft.UI.Xaml.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

partial class ContentPageViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVolumeCoverNotExist))]
    [NotifyPropertyChangedFor(nameof(CoverImageVerticalAlignmentMode))]
    private BitmapImage? _volumeCover;
    [ObservableProperty]
    private bool _showContent;
    [ObservableProperty]
    private bool _isVolumeCoverError;

    public bool IsVolumeCoverNotExist => VolumeCover is null;
    public VerticalAlignment CoverImageVerticalAlignmentMode => VolumeCover is null ? VerticalAlignment.Stretch : VerticalAlignment.Top;
}

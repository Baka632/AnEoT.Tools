using AnEoT.Tools.VolumeCreator.Models.Lofter;
using AnEoT.Tools.VolumeCreator.Views.LofterDownload;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public partial class LofterDownloadViewModel : ObservableObject
{
    [ObservableProperty]
    private bool enableForward;
    [ObservableProperty]
    private bool enablePrevious;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowForward))]
    private bool showComplete;

    public bool ShowForward { get => !ShowComplete; }

    public LofterDownloadData DownloadData { get; set; } = new(null, null, null, default);
    public LofterDownloadWindowAccessor WindowAccessor { get; }

    public LofterDownloadViewModel(LofterDownloadWindow view)
    {
        WindowAccessor = new(this, view);
    }
}
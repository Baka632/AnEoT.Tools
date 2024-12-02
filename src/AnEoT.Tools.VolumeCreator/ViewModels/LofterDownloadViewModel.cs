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

    public LofterDownloadData DownloadData { get; } = new();
    public LofterDownloadWindowAccessor WindowAccessor { get; }

    public LofterDownloadViewModel()
    {
        WindowAccessor = new(this);
    }
}

public sealed class LofterDownloadWindowAccessor(LofterDownloadViewModel viewModel)
{
    public bool EnableForward { get => viewModel.EnableForward; set => viewModel.EnableForward = value; }
    //public bool EnablePrevious { get => viewModel.EnablePrevious; set => viewModel.EnablePrevious = value; }
}

public sealed class LofterDownloadData
{
    public Uri? PageUri { get; set; }
    public string? LofterCookie { get; set; }
}
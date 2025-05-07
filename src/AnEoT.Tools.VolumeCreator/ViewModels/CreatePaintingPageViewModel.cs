using AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;
using AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public partial class CreatePaintingPageViewModel : ObservableObject
{
    [ObservableProperty]
    private bool enableForward;
    [ObservableProperty]
    private bool enablePrevious;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowForward))]
    private bool showComplete;

    public bool ShowForward { get => !ShowComplete; }

    public CreatePaintingPageData PaintingPageData { get; set; } = new([], [], null);

    public CreatePaintingPageWindowAccessor WindowAccessor { get; }

    public CreatePaintingPageViewModel(CreatePaintingPageWindow view)
    {
        WindowAccessor = new(this, view);
    }
}

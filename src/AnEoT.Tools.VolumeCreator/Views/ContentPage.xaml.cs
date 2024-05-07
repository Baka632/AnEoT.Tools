using AnEoT.Tools.VolumeCreator.ViewModels;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ContentPage : Page
{
    public ContentPageViewModel ViewModel { get; } = new ContentPageViewModel();

    public ContentPage()
    {
        this.InitializeComponent();
    }
}

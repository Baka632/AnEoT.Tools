#pragma warning disable CS8618

using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.ViewModels;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MarkdownEditPage : Page
{
    public MarkdownEditViewModel ViewModel { get; private set; }

    public MarkdownEditPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is MarkdownWrapper wrapper)
        {
            ViewModel = new MarkdownEditViewModel(wrapper, this);
        }

        base.OnNavigatedTo(e);
    }
}

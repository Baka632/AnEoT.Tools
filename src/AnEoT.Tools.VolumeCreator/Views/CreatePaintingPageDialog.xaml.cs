using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.Views;

[INotifyPropertyChanged]
public sealed partial class CreatePaintingPageDialog : ContentDialog
{
    public CreatePaintingPageDialog()
    {
        this.InitializeComponent();
    }

    private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // TODO
    }
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.Views;

[INotifyPropertyChanged]
public sealed partial class NewAssetFolderDialog : ContentDialog
{
    [ObservableProperty]
    private string newFolderName = string.Empty;
    [ObservableProperty]
    private bool showEmptyFolderNameWarning;

    partial void OnNewFolderNameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            ShowEmptyFolderNameWarning = false;
        }
    }

    public NewAssetFolderDialog()
    {
        this.InitializeComponent();
    }

    private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(NewFolderName))
        {
            ShowEmptyFolderNameWarning = true;
            args.Cancel = true;
        }
    }
}

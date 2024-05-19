using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        this.InitializeComponent();

        if (MicaController.IsSupported())
        {
            MicaBackdrop micaBackdrop = new();
            SystemBackdrop = micaBackdrop;
        }
        else if (DesktopAcrylicController.IsSupported())
        {
            DesktopAcrylicBackdrop acrylicBackdrop = new();
            SystemBackdrop = acrylicBackdrop;
        }

        AppWindow.Closing += OnClosing;
    }

    private async void OnClosing(object sender, AppWindowClosingEventArgs e)
    {
        e.Cancel = true;

        ContentDialog dialog = new()
        {
            Title = "要关闭应用吗？",
            PrimaryButtonText = "关闭",
            CloseButtonText = "取消",
            XamlRoot = Content.XamlRoot
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            Close();
        }
    }
}

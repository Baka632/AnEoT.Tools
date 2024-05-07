using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;

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
    }
}

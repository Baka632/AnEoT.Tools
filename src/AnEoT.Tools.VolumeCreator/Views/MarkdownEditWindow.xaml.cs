using System.Collections.ObjectModel;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Models.Resources;
using Microsoft.UI.Composition.SystemBackdrops;
using WinUIEx;

namespace AnEoT.Tools.VolumeCreator.Views;

public sealed partial class MarkdownEditWindow : WindowEx
{
    public (MarkdownWrapper? Markdown, ObservableCollection<AssetNode> Assets, IVolumeResourcesHelper ResourceHelper, bool ConvertWebP) Model { get; set; }

    public MarkdownEditWindow()
    {
        ExtendsContentIntoTitleBar = true;
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

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(MarkdownEditPage), Model);
    }
}

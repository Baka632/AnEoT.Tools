using System.Collections.ObjectModel;
using AnEoT.Tools.VolumeCreator.Models;
using Microsoft.UI.Composition.SystemBackdrops;
using WinUIEx;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MarkdownEditWindow : WindowEx
{
    public (MarkdownWrapper?, ObservableCollection<ImageListNode>) Model { get; set; }

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

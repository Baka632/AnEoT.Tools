using AnEoT.Tools.VolumeCreator.Views.LofterDownload;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnEoT.Tools.VolumeCreator.ViewModels;

public sealed partial class UtilitiesViewModel : ObservableObject
{
    [RelayCommand]
    private static void OpenLofterDownloadWindow()
    {
        LofterDownloadWindow window = new();
        window.Activate();
    }

    [RelayCommand]
    private static void OpenConvertWebPWindow()
    {
        // TODO: 
        // ;)
    }
}
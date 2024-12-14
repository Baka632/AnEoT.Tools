using AnEoT.Tools.VolumeCreator.Views.ImageConvert;
using AnEoT.Tools.VolumeCreator.Views.LofterDownload;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

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
    private static void OpenImageConvertWindow()
    {
        ImageConvertWindow window = new();
        window.Activate();
    }
}
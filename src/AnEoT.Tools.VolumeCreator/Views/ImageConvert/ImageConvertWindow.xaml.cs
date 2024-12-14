using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;
using CommunityToolkit.Mvvm.ComponentModel;
using AnEoT.Tools.VolumeCreator.Models.ImageConvert;

namespace AnEoT.Tools.VolumeCreator.Views.ImageConvert;

[INotifyPropertyChanged]
public sealed partial class ImageConvertWindow : WindowEx
{
    private readonly ImageConvertWindowAccessor accessor;

    [ObservableProperty]
    private bool showCompleted;
    [ObservableProperty]
    private bool enableStart = true;

    public ImageConvertWindow()
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

        accessor = new(this);
        ContentFrame.Navigate(typeof(ImageConvertPage), accessor);
    }

    private void OnCompletedButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnTickStartButtonClick(object sender, RoutedEventArgs e)
    {
        accessor.TickConvertStarted();
    }

    public static Visibility ReverseBooleanToVisibility(bool value)
    {
        return !value switch
        {
            true => Visibility.Visible,
            false => Visibility.Collapsed
        };
    }
}
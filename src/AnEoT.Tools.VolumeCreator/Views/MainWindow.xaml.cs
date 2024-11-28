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

    //private async void SetTaskBarIcon()
    //{
    //    StorageFile iconFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/AppIcon.ico"));
    //    TaskBarIcon = Icon.FromFile(iconFile.Path);
    //}

    private async void OnClosing(object sender, AppWindowClosingEventArgs e)
    {
        if (!CommonValues.IsProjectSaved)
        {
            e.Cancel = true;

            ContentDialog dialog = new()
            {
                Title = "Ҫ�ر�Ӧ���𣿹����ļ���û�б��档",
                PrimaryButtonText = "�ر�",
                CloseButtonText = "ȡ��",
                XamlRoot = Content.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Close();
            }
        }
    }

    public async void InitalizeWindow(object? parameter = null)
    {
        if (MainFrame.Content is VolumeCreationPage page && parameter is not null)
        {
            await page.ParseParameter(parameter);
        }
        else
        {
            if (parameter is null)
            {
                MainFrame.Navigate(typeof(VolumeCreationPage));
            }
            else
            {
                MainFrame.Navigate(typeof(VolumeCreationPage), parameter);
            }
        }
    }
}

using WinUIEx;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Imaging;

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
                Title = "要关闭应用吗？工程文件还没有保存。",
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

        UtilitiesFrame.Navigate(typeof(UtilitiesPage));
    }

    private void OnAboutImageLoaded(object sender, RoutedEventArgs e)
    {
        Image image = (Image)sender;
        Uri aboutImageUri;

#if DEBUG
        aboutImageUri = new Uri("ms-appx:///Assets/App-Logo/Logo-Dev.png");
#else
        aboutImageUri = App.Current.RequestedTheme == ApplicationTheme.Dark
            ? new Uri("ms-appx:///Assets/App-Logo/Logo-White.png")
            : new Uri("ms-appx:///Assets/App-Logo/Logo-Black.png");
#endif

        image.Source = new BitmapImage(aboutImageUri);
    }

    private void OnQuoteRichTextBlockPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        QuoteRichTextBlock.Opacity = 1;
    }

    private void OnQuoteRichTextBlockPointerExited(object sender, PointerRoutedEventArgs e)
    {
        QuoteRichTextBlock.Opacity = 0;
    }
}

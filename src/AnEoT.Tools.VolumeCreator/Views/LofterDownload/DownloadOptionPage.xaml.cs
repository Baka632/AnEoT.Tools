using System.Diagnostics.CodeAnalysis;
using AnEoT.Tools.VolumeCreator.Models.Lofter;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

[INotifyPropertyChanged]
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DownloadOptionPage : Page
{
    private LofterDownloadWindowAccessor windowAccessor;

    [ObservableProperty]
    private bool trimQueryArgs = true;
    [ObservableProperty]
    private bool convertWebP;
    [ObservableProperty]
    private StorageFolder? saveFolder;

    partial void OnSaveFolderChanged(StorageFolder? value)
    {
        windowAccessor.EnableForward = CheckSaveFolder();
    }

#pragma warning disable CS8618 // OnNavigatedTo »á³öÊÖ
    public DownloadOptionPage()
#pragma warning restore CS8618
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is LofterDownloadWindowAccessor accessor)
        {
            windowAccessor = accessor;
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        if (CheckSaveFolder())
        {
            windowAccessor.DownloadData = windowAccessor.DownloadData with
            {
                DownloadOptions = new(SaveFolder.Path, ConvertWebP, TrimQueryArgs)
            };
        }
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        windowAccessor.EnableForward = CheckSaveFolder();
    }

    [RelayCommand]
    private async Task SelectSaveFolder()
    {
        FolderPicker folderPicker = new()
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };

        nint hWnd = windowAccessor.GetWindowHandle();
        InitializeWithWindow.Initialize(folderPicker, hWnd);

        StorageFolder folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            SaveFolder = folder;
        }
    }

    [MemberNotNullWhen(true, nameof(SaveFolder))]
    private bool CheckSaveFolder()
    {
        return SaveFolder is not null && Directory.Exists(SaveFolder.Path);
    }
}

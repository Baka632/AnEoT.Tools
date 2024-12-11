using WinRT.Interop;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using AnEoT.Tools.VolumeCreator.Models.ImageConvert;

namespace AnEoT.Tools.VolumeCreator.Views.ImageConvert;

[INotifyPropertyChanged]
public sealed partial class ImageConvertPage : Page
{
    public ImageFormatType[] ImageFormats = [
            ImageFormatType.Webp,
            ImageFormatType.Jpg,
            ImageFormatType.Png,
     ];

    public ImageConvertSaveMethod[] SaveMethods = [
        ImageConvertSaveMethod.DifferentExtension,
        ImageConvertSaveMethod.CreateInnerFolder,
        ImageConvertSaveMethod.SelectOtherFolder,
     ];

    public ImageConvertWindowAccessor WindowAccessor { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedImageFormat))]
    private int selectedImageFormatIndex = 0;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedSaveMethod))]
    private int selectedSaveMethodIndex;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllowFutherAddition))]
    private bool showConverting;
    [ObservableProperty]
    private bool showSelectOtherFolder;
    [ObservableProperty]
    private string? selectedSaveFolderPath;
    [ObservableProperty]
    private bool imagesHasItems;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllowFutherAddition))]
    private bool isConvertCompleted;

    public bool AllowFutherAddition { get => !ShowConverting && !IsConvertCompleted; }

    partial void OnSelectedSaveMethodIndexChanged(int value)
    {
        ShowSelectOtherFolder = SelectedSaveMethod == ImageConvertSaveMethod.SelectOtherFolder;
        if (ShowSelectOtherFolder && !Directory.Exists(SelectedSaveFolderPath))
        {
            WindowAccessor.EnableStart = false;
        }
        else
        {
            WindowAccessor.EnableStart = true;
        }
    }

    partial void OnSelectedSaveFolderPathChanged(string? value)
    {
        if (ShowSelectOtherFolder)
        {
            WindowAccessor.EnableStart = Directory.Exists(SelectedSaveFolderPath);
        }
    }

    public ImageFormatType SelectedImageFormat { get => ImageFormats[SelectedImageFormatIndex]; }
    public ImageConvertSaveMethod SelectedSaveMethod { get => SaveMethods[SelectedSaveMethodIndex]; }
    public ObservableCollection<ImageConvertItem> Images { get; } = [];

#pragma warning disable CS8618 // OnNavigatedTo£¬Æô¶¯£¡
    public ImageConvertPage()
#pragma warning restore CS8618
    {
        this.InitializeComponent();
        Images.CollectionChanged += OnImagesCollectionChanged;
    }

    private void OnImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ImagesHasItems = Images.Count > 0;
        if (ShowConverting && !Images.Any(item => item.State is ImageConvertItemState.None
            or ImageConvertItemState.Converting
            or ImageConvertItemState.Completed))
        {
            ShowConverting = false;
            WindowAccessor.ShowComplete = true;
            IsConvertCompleted = true;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ImageConvertWindowAccessor accessor)
        {
            WindowAccessor = accessor;
            accessor.ConvertStarted += OnConvertStarted;
        }
    }

    private void OnConvertStarted()
    {
        ShowConverting = true;
        WindowAccessor.EnableStart = false;
        try
        {
            foreach (ImageConvertItem item in Images.ToArray())
            {
                item.PropertyChanged += OnImageConvertItemPropertyChanged;
                _ = item.ConvertItemAsync(SelectedImageFormat, SelectedSaveMethod, SelectedSaveFolderPath);
            }
        }
        finally
        {
            WindowAccessor.EnableStart = true;
        }

        void OnImageConvertItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ImageConvertItem item = (ImageConvertItem)sender!;
            if (item.State == ImageConvertItemState.Completed)
            {
                Images.Remove(item);
                item.PropertyChanged -= OnImageConvertItemPropertyChanged;
            }
        }
    }

    private async void OnSelectImageSettingsCardClick(object sender, RoutedEventArgs e)
    {
        FileOpenPicker picker = new()
        {
            FileTypeFilter = { ".jpg", ".png", ".webp", ".bmp" },
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
        };
        InitializeWithWindow.Initialize(picker, WindowAccessor.GetWindowHandle());

        IReadOnlyList<StorageFile>? files = await picker.PickMultipleFilesAsync();
        if (files is not null && files.Count > 0)
        {
            foreach (StorageFile file in files)
            {
                ImageConvertItem item = new(file.Path);
                Images.Add(item);
            }
        }
    }

    private async void OnSelectSaveFolderSettingsCardClick(object sender, RoutedEventArgs e)
    {
        FolderPicker folderPicker = new()
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
        };

        InitializeWithWindow.Initialize(folderPicker, WindowAccessor.GetWindowHandle());

        StorageFolder folder = await folderPicker.PickSingleFolderAsync();

        if (folder is not null)
        {
            SelectedSaveFolderPath = folder.Path;
        }
    }

    private void OnRemoveImageConvertItemButtonClick(object sender, RoutedEventArgs e)
    {
        ImageConvertItem item = (ImageConvertItem)((Button)sender).DataContext;
        Images.Remove(item);
    }

    public static bool IsConverting(ImageConvertItemState state)
    {
        return state == ImageConvertItemState.Converting;
    }

    public static bool IsNoneState(ImageConvertItemState state)
    {
        return state == ImageConvertItemState.None;
    }

    public static Visibility AllowRemovalToVisibility(ImageConvertItemState state)
    {
        return (state is not ImageConvertItemState.Converting) switch
        {
            true => Visibility.Visible,
            false => Visibility.Collapsed,
        };
    }

    public static bool IsError(ImageConvertItemState state)
    {
        return state == ImageConvertItemState.Error;
    }

    public static Visibility IsErrorToVisibility(ImageConvertItemState state)
    {
        return (state == ImageConvertItemState.Error) switch
        {
            true => Visibility.Visible,
            false => Visibility.Collapsed,
        };
    }
}
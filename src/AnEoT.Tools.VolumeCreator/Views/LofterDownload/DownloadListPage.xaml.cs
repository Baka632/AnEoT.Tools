using System.Collections.ObjectModel;
using System.ComponentModel;
using AnEoT.Tools.VolumeCreator.Models.Lofter;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

[INotifyPropertyChanged]
public sealed partial class DownloadListPage : Page
{
    private LofterDownloadWindowAccessor windowAccessor;
    private LofterDownloadData data;

    public ObservableCollection<LofterDownloadItem> Downloads { get; } = [];

    [ObservableProperty]
    private bool showEmptyList;

#pragma warning disable CS8618 // OnNavigatedTo »á³öÊÖ
    public DownloadListPage()
#pragma warning restore CS8618
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
        Downloads.CollectionChanged += OnDownloadsCollectionChanged;
    }

    private void OnDownloadsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        ShowEmptyList = Downloads.Count == 0;

        if (!Downloads.Where(item => item.State == LofterDownloadItemState.Completed).Any())
        {
            windowAccessor.EnableForward = true;
            windowAccessor.ShowComplete = true;
        }
        else
        {
            windowAccessor.EnableForward = false;
            windowAccessor.ShowComplete = false;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is LofterDownloadWindowAccessor accessor)
        {
            windowAccessor = accessor;
            data = windowAccessor.DownloadData;
        }
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        windowAccessor.EnableForward = !Downloads.Any();

        if (data.ImageInfos is null)
        {
            windowAccessor.EnableForward = false;
            return;
        }

        foreach (LofterImageInfo info in data.ImageInfos)
        {
            LofterDownloadItem item = new(data.DownloadOptions, info);
            item.PropertyChanged += OnDownloadItemPropertyChanged;
            _ = item.DownloadAsync();
            Downloads.Add(item);
        }

        void OnDownloadItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            LofterDownloadItem item = (LofterDownloadItem)sender!;
            if (item.State == LofterDownloadItemState.Completed)
            {
                Downloads.Remove(item);
                item.PropertyChanged -= OnDownloadItemPropertyChanged;
            }
        }
    }

    internal static bool IsDownloading(LofterDownloadItemState state)
    {
        return state == LofterDownloadItemState.Downloading;
    }

    internal static bool IsError(LofterDownloadItemState state)
    {
        return state == LofterDownloadItemState.Error;
    }

    internal static bool IsPaused(LofterDownloadItemState state)
    {
        return state == LofterDownloadItemState.Paused;
    }

    internal static Visibility ShowDownloadingBar(LofterDownloadItemState state)
    {
        return state switch
        {
            LofterDownloadItemState.Downloading => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    internal static Visibility ShowErrorButton(LofterDownloadItemState state)
    {
        return state switch
        {
            LofterDownloadItemState.Error => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    internal static Visibility ShowRemoveButton(LofterDownloadItemState state)
    {
        return state switch
        {
            LofterDownloadItemState.Error or LofterDownloadItemState.Completed => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    private void OnRemoveItemButtonClicked(object sender, RoutedEventArgs e)
    {
        LofterDownloadItem item = (LofterDownloadItem)((Button)sender).DataContext;
        Downloads.Remove(item);
    }
}

using System.ComponentModel;
using System.Net;
using System.Windows.Input;
using AnEoT.Tools.VolumeCreator.Helpers;
using AnEoT.Tools.VolumeCreator.Models.Lofter;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

[INotifyPropertyChanged]
public sealed partial class SelectTargetImagePage : Page
{
    private LofterDownloadWindowAccessor windowAccessor;
    private LofterDownloadData data;
    private LofterDownloadData formerData;

    [ObservableProperty]
    private string infoBarTitle;
    [ObservableProperty]
    private string infoBarMessage;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCopyrightInfoBar))]
    private bool showInfoBar;
    [ObservableProperty]
    private ICommand infoBarActionCommand;
    [ObservableProperty]
    private string infoBarActionText;
    [ObservableProperty]
    private bool showInfoBarActionButton;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCopyrightInfoBar))]
    private bool isLoading;
    [ObservableProperty]
    private IEnumerable<LofterImageInfo> imageInfos;

    public bool ShowCopyrightInfoBar { get => !ShowInfoBar && !IsLoading; }

#pragma warning disable CS8618 // OnNavigatedTo 会出手
    public SelectTargetImagePage()
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
            data = accessor.DownloadData;
        }
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        windowAccessor.EnableForward = false;

        if (data?.PageUri is null || data?.LofterCookie is null)
        {
            SetInfoBar("目标地址或者 Lofter 登陆 Cookie 为空",
                       "你是怎么到这里来的？滚回去！（车尔尼音）");
        }
        else
        {
            if (formerData?.PageUri == data.PageUri && formerData?.LofterCookie == data.LofterCookie)
            {
                return;
            }
            else
            {
                ShowInfoBar = false;
                ImageInfos = [];
                formerData = data;
            }

            await GetTargetPageImage(data.PageUri, data.LofterCookie);
        }
    }

    private async Task GetTargetPageImage(Uri pageUri, string lofterCookie)
    {
        IsLoading = true;

        try
        {
            string html = await LofterDownloadHelper.GetPageHtml(pageUri, lofterCookie);

            HtmlParser parser = new();
            IConfiguration configuration = Configuration.Default;
            using IBrowsingContext context = BrowsingContext.New(configuration);
            using IDocument document = await context.OpenAsync(res => res.Content(html).Address(pageUri));

            IEnumerable<LofterImageInfo> images = document.Body?.Descendants<IHtmlImageElement>()
                .Where(image => !image.ClassList.Contains("avatar") && !image.ClassList.Contains("itag"))
                .Select(image =>
                {
                    string? rawSource = image.Source;
                    string? rawFileName = Path.GetFileName(rawSource);

                    string title = string.IsNullOrWhiteSpace(image.AlternativeText)
                        ? WebUtility.UrlDecode(rawFileName) ?? "未知图像"
                        : Path.ChangeExtension(image.AlternativeText, Path.GetExtension(rawFileName));

                    return Uri.TryCreate(rawSource, UriKind.Absolute, out Uri? uri)
                        ? new LofterImageInfo(uri.ToString(), title, pageUri)
                        : null;
                })
                .Where(info => info is not null)
                .Select(info => info!)
                .DistinctBy(info => info.ImageUri) ?? [];
            ImageInfos = images;
        }
        catch (HttpRequestException ex)
        {
            SetInfoBar("无法获取目标网页",
                       $"请检查你的网络连接。\n异常信息：{ex}",
                       true,
                       "重试",
                       RetryGetImageCommand);
            return;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RetryGetImage()
    {
        if (data.PageUri is not null && data.LofterCookie is not null)
        {
            ShowInfoBar = false;
            await GetTargetPageImage(data.PageUri, data.LofterCookie);
        }
    }

    private void SetInfoBar(string title, string message, bool showActionButton = false, string actionText = "", ICommand? actionCommand = null)
    {
        InfoBarTitle = title;
        InfoBarMessage = message;

        if (showActionButton)
        {
            InfoBarActionText = actionText;
            if (actionCommand is not null)
            {
                InfoBarActionCommand = actionCommand;
            }

            ShowInfoBarActionButton = true;
        }
        else
        {
            ShowInfoBarActionButton = false;
        }

        ShowInfoBar = true;
    }

    private async void OnImageLoaded(object sender, RoutedEventArgs e)
    {
        Image img = (Image)sender;
        LofterImageInfo info = (LofterImageInfo)img.DataContext;

        try
        {
            using Stream imageStream = await LofterDownloadHelper.GetImage(new Uri(info.ImageUri, UriKind.Absolute), info.SourcePageUri);
            using IRandomAccessStream winrtStream = imageStream.AsRandomAccessStream();

            BitmapImage bitmapImage = new()
            {
                DecodePixelType = DecodePixelType.Logical,
                DecodePixelWidth = 250
            };
            await bitmapImage.SetSourceAsync(winrtStream);

            img.Source = bitmapImage;
        }
        catch (Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(ex);
#endif
        }
    }

    private void OnImagesGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ImagesGridView.SelectedItems.Count > 0)
        {
            windowAccessor.DownloadData = windowAccessor.DownloadData with
            {
                ImageInfos = ImagesGridView.SelectedItems.Cast<LofterImageInfo>()
            };
            windowAccessor.EnableForward = true;
        }
        else
        {
            windowAccessor.EnableForward = false;
        }
    }
}
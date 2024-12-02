using AnEoT.Tools.VolumeCreator.ViewModels;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SelectTargetImagePage : Page
{
    private LofterDownloadWindowAccessor windowAccessor;
    private LofterDownloadData data;

#pragma warning disable CS8618 // OnNavigatedTo »á³öÊÖ
    public SelectTargetImagePage()
#pragma warning restore CS8618
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ValueTuple<LofterDownloadWindowAccessor, LofterDownloadData> tuple)
        {
            (windowAccessor, data) = tuple;
        }
    }
}

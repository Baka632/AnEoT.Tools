using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;

[INotifyPropertyChanged]
public sealed partial class SelectImagePage : Page
{
    private CreatePaintingPageWindowAccessor windowAccessor;
    private CreatePaintingPageData data;

    public IEnumerable<AssetNode> OriginalAssets { get; private set; }

#pragma warning disable CS8618 // OnNavigatedTo 会出手
    public SelectImagePage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }
#pragma warning restore CS8618

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is CreatePaintingPageWindowAccessor accessor)
        {
            windowAccessor = accessor;
            data = windowAccessor.PaintingPageData;

            OriginalAssets = data.OriginalAssets;
        }
    }

    private void OnAssetsTreeViewSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
    {
        IEnumerable<FileNode> targetFileAssets = sender.SelectedItems
            .Where(obj => obj is FileNode)
            .Cast<FileNode>();
        if (targetFileAssets.Any())
        {
            windowAccessor.PaintingPageData = windowAccessor.PaintingPageData with
            {
                FileAssets = [.. targetFileAssets]
            };
            windowAccessor.EnableForward = true;
        }
        else
        {
            windowAccessor.EnableForward = false;
        }
    }
}

using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;

namespace AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;
public sealed partial class OrderAndRenameImagePage : Page
{
    private CreatePaintingPageWindowAccessor windowAccessor;
    private CreatePaintingPageData data;

    // FileNode 中包含 required 成员，而自动生成的代码不能处理这种情况，因此这里的泛型参数仍然是 AssetNode
    public List<AssetNode> FileNodes { get; private set; }

#pragma warning disable CS8618 // OnNavigatedTo 会出手
    public OrderAndRenameImagePage()
    {
        this.InitializeComponent();
    }
#pragma warning restore CS8618

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is CreatePaintingPageWindowAccessor accessor)
        {
            windowAccessor = accessor;
            data = windowAccessor.PaintingPageData;

            FileNodes = [.. data.FileAssets];
        }
    }
}

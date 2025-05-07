using AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;

namespace AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;

public sealed partial class CompletePage : Page
{
    private CreatePaintingPageWindowAccessor windowAccessor;
    private CreatePaintingPageData data;

#pragma warning disable CS8618 // OnNavigatedTo 会出手
    public CompletePage()
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

            accessor.ShowComplete = true;
        }
    }
}

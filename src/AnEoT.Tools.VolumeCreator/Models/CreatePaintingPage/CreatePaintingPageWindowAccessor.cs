using AnEoT.Tools.VolumeCreator.ViewModels;
using AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;
using WinRT.Interop;

namespace AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;

public sealed class CreatePaintingPageWindowAccessor(CreatePaintingPageViewModel viewModel, CreatePaintingPageWindow window)
{
    public bool EnableForward
    {
        get => viewModel.EnableForward;
        set => viewModel.EnableForward = value;
    }

    public CreatePaintingPageData PaintingPageData
    {
        get => viewModel.PaintingPageData;
        set => viewModel.PaintingPageData = value;
    }

    public bool ShowComplete
    {
        get => viewModel.ShowComplete;
        set => viewModel.ShowComplete = value;
    }

    public nint GetWindowHandle()
    {
        return WindowNative.GetWindowHandle(window);
    }
}

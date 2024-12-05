using AnEoT.Tools.VolumeCreator.ViewModels;
using AnEoT.Tools.VolumeCreator.Views.LofterDownload;
using WinRT.Interop;

namespace AnEoT.Tools.VolumeCreator.Models.Lofter;

public sealed class LofterDownloadWindowAccessor(LofterDownloadViewModel viewModel, LofterDownloadWindow window)
{
    public bool EnableForward
    {
        get => viewModel.EnableForward;
        set => viewModel.EnableForward = value;
    }

    public LofterDownloadData DownloadData
    {
        get => viewModel.DownloadData;
        set => viewModel.DownloadData = value;
    }

    public nint GetWindowHandle()
    {
        return WindowNative.GetWindowHandle(window);
    }
}

using AnEoT.Tools.VolumeCreator.ViewModels;

namespace AnEoT.Tools.VolumeCreator.Models.Lofter;

public sealed class LofterDownloadWindowAccessor(LofterDownloadViewModel viewModel)
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
}

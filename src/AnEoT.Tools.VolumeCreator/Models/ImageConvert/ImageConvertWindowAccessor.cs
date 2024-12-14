using WinRT.Interop;
using AnEoT.Tools.VolumeCreator.Views.ImageConvert;

namespace AnEoT.Tools.VolumeCreator.Models.ImageConvert;

public sealed class ImageConvertWindowAccessor(ImageConvertWindow window)
{
    public event Action? ConvertStarted;

    public bool EnableStart
    {
        get => window.EnableStart;
        set => window.EnableStart = value;
    }

    public bool ShowComplete
    {
        get => window.ShowCompleted;
        set => window.ShowCompleted = value;
    }

    public nint GetWindowHandle()
    {
        return WindowNative.GetWindowHandle(window);
    }

    public void TickConvertStarted()
    {
        ConvertStarted?.Invoke();
    }
}

#pragma warning disable CS8618

using AnEoT.Tools.VolumeCreator.Views;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AnEoT.Tools.VolumeCreator;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public Window Window { get; private set; }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        Exception exception = e.Exception;

        if (exception is null)
        {
            return;
        }

        try
        {
            new ToastContentBuilder().AddText("出现未处理的异常，欢迎来拷打作者@.@")
                                 .AddText($"出错方法：{exception.TargetSite?.Name ?? "<未知>"}")
                                 .AddText($"异常名称：{exception.GetType().Name}")
                                 .Show();
        }
        catch (InvalidOperationException)
        {
            // 不要在处理异常时又一次抛出异常...
        }
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow
        {
            ExtendsContentIntoTitleBar = true,
            Title = "AnEoT Volume Creator",
        };
        Window.Activate();
    }
}

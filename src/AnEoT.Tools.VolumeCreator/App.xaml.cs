#pragma warning disable CS8618

using AnEoT.Tools.VolumeCreator.Views;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Windows.AppLifecycle;
using WASDKAppInstance = Microsoft.Windows.AppLifecycle.AppInstance;
using Windows.ApplicationModel;
using Microsoft.UI.Dispatching;

namespace AnEoT.Tools.VolumeCreator;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 获取应用程序版本
    /// </summary>
    public static string AppVersion => $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

    public static string AppName { get; }

    public Window Window { get; private set; }

    static App()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        AppName = now switch
        {
            { Month: 1, Day: 4 } => $"{Package.Current.DisplayName} - 尤里卡生日快乐！",
            { Month: 5, Day: 17 } => $"{Package.Current.DisplayName} - 回归线生日快乐！",
            _ => Package.Current.DisplayName
        };
    }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    public void RunOnUIThread(DispatcherQueueHandler handler)
    {
        Window.DispatcherQueue.TryEnqueue(handler);
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
        AppActivationArguments activatedArgs = WASDKAppInstance.GetCurrent().GetActivatedEventArgs();

        MainWindow mainWindow = new()
        {
            ExtendsContentIntoTitleBar = true,
            Title = AppName,
        };
        Window = mainWindow;

        if (activatedArgs.Kind == ExtendedActivationKind.File)
        {
            var file = (Windows.ApplicationModel.Activation.FileActivatedEventArgs)activatedArgs.Data;
            mainWindow.InitalizeWindow(file.Files);
        }
        else
        {
            mainWindow.InitalizeWindow();
        }

        Window.Activate();
    }
}

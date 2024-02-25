using System.Windows;
using AnEoT.Tools.WordToMarkdown.Views;
using CommandLine;

namespace AnEoT.Tools.WordToMarkdown;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        if (e.Args.Length == 0)
        {
            HideConsole();
            MainWindow mainWindow = new();
            mainWindow.Show();
        }
        else
        {
            Parser.Default.ParseArguments<GuiOptions, CliOptions>(e.Args).WithParsed<CommonOptions>(async options =>
            {
                await RunByOptions(options);
            })
            .WithNotParsed(errors =>
            {
                Environment.Exit(1);
            });
        }
    }
}
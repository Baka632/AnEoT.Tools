using AnEoT.Tools.VolumeCreator.ViewModels;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media.Animation;
using WinUIEx;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LofterDownloadWindow : WindowEx
{
    private int previousSelectIndex;

    public LofterDownloadViewModel ViewModel { get; } = new LofterDownloadViewModel();

    public LofterDownloadWindow()
    {
        ExtendsContentIntoTitleBar = true;
        this.InitializeComponent();

        if (MicaController.IsSupported())
        {
            MicaBackdrop micaBackdrop = new();
            SystemBackdrop = micaBackdrop;
        }
        else if (DesktopAcrylicController.IsSupported())
        {
            DesktopAcrylicBackdrop acrylicBackdrop = new();
            SystemBackdrop = acrylicBackdrop;
        }
    }

    private void ForwardClick(object sender, RoutedEventArgs e)
    {
        int currentIndex = StepSelectorBar.Items.IndexOf(StepSelectorBar.SelectedItem);

        if (currentIndex != -1 && currentIndex + 1 < StepSelectorBar.Items.Count)
        {
            StepSelectorBar.SelectedItem = StepSelectorBar.Items[currentIndex + 1];
        }
    }

    private void PreviousClick(object sender, RoutedEventArgs e)
    {
        int currentIndex = StepSelectorBar.Items.IndexOf(StepSelectorBar.SelectedItem);

        if (currentIndex != -1 && currentIndex - 1 >= 0)
        {
            StepSelectorBar.SelectedItem = StepSelectorBar.Items[currentIndex - 1];
        }
    }

    private void OnStepSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        int currentIndex = sender.Items.IndexOf(StepSelectorBar.SelectedItem);
        int count = sender.Items.Count;

        Type targetPage = sender.SelectedItem.Tag switch
        {
            "LoginAndAddress" => typeof(LofterLoginPage),
            "TargetImage" => typeof(SelectTargetImagePage),
            "DownloadOption" => typeof(DownloadOptionPage),
            "Complete" => typeof(DownloadCompletePage),
            _ => throw new InvalidOperationException("ÎÞÐ§µÄ SelectorBarItem Tag¡£")
        };

        SlideNavigationTransitionInfo transition = new()
        {
            Effect = currentIndex - previousSelectIndex > 0
                ? SlideNavigationTransitionEffect.FromRight
                : SlideNavigationTransitionEffect.FromLeft
        };

        ContentFrame.Navigate(targetPage, (ViewModel.WindowAccessor, ViewModel.DownloadData), transition);

        if (currentIndex + 1 >= count)
        {
            ViewModel.EnableForward = false;
            ViewModel.EnablePrevious = ViewModel.ShowComplete = true;
        }
        else if (currentIndex - 1 < 0)
        {
            ViewModel.EnableForward = true;
            ViewModel.EnablePrevious = ViewModel.ShowComplete = false;
        }
        else
        {
            ViewModel.EnableForward = true;
            ViewModel.EnablePrevious = true;
            ViewModel.ShowComplete = false;
        }

        previousSelectIndex = currentIndex;
    }
}
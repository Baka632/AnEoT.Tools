using AnEoT.Tools.VolumeCreator.ViewModels;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media.Animation;
using WinUIEx;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

public sealed partial class LofterDownloadWindow : WindowEx
{
    private int previousSelectIndex;

    public LofterDownloadViewModel ViewModel { get; }

    public LofterDownloadWindow()
    {
        ExtendsContentIntoTitleBar = true;
        this.InitializeComponent();

        ViewModel = new LofterDownloadViewModel(this);

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
            "List" => typeof(DownloadListPage),
            _ => throw new InvalidOperationException("无效的 SelectorBarItem Tag。")
        };

        SlideNavigationTransitionInfo transition = new()
        {
            Effect = currentIndex - previousSelectIndex > 0
                ? SlideNavigationTransitionEffect.FromRight
                : SlideNavigationTransitionEffect.FromLeft
        };

        ContentFrame.Navigate(targetPage, ViewModel.WindowAccessor, transition);

        if (currentIndex + 1 >= count)
        {
            ViewModel.EnableForward = false;
            ViewModel.EnablePrevious = true;
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

    private void CloseWindowClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void RestartDownloadClick(object sender, RoutedEventArgs e)
    {
        ViewModel.DownloadData = ViewModel.DownloadData with { ImageInfos = null, PageUri = null };
        StepSelectorBar.SelectedItem = StepSelectorBar.Items[0];
    }
}
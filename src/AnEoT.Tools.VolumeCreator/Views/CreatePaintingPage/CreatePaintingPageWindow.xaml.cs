using AnEoT.Tools.VolumeCreator.ViewModels;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media.Animation;
using WinUIEx;

namespace AnEoT.Tools.VolumeCreator.Views.CreatePaintingPage;

public sealed partial class CreatePaintingPageWindow : WindowEx
{
    private int previousSelectIndex;

    public CreatePaintingPageViewModel ViewModel { get; }

    public CreatePaintingPageWindow()
    {
        ExtendsContentIntoTitleBar = true;
        this.InitializeComponent();
        ViewModel = new(this);

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

    private void OnStepSelectorBarSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        int currentIndex = sender.Items.IndexOf(StepSelectorBar.SelectedItem);
        int count = sender.Items.Count;

        Type targetPage = sender.SelectedItem.Tag switch
        {
            "SelectTargetImage" => typeof(SelectImagePage),
            "OrderAndRename" => typeof(OrderAndRenameImagePage),
            "Complete" => typeof(CompletePage),
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
}

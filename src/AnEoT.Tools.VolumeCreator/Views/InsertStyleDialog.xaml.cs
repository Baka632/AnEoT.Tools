using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AnEoT.Tools.VolumeCreator.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnEoT.Tools.VolumeCreator.Views;

[INotifyPropertyChanged]
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class InsertStyleDialog : ContentDialog
{
    public bool ShowNotifyAddStyle { get => TargetStyles.Count <= 0; }

    [ObservableProperty]
    private string styleString = string.Empty;
    [ObservableProperty]
    private ObservableCollection<StringView> targetStyles = [];

    public InsertStyleDialog()
    {
        this.InitializeComponent();
        TargetStyles.CollectionChanged += OnStylesCollectionChanged;
    }

    [RelayCommand]
    private void AddCustomStyle()
    {
        TargetStyles.Add(new StringView("style=\"\""));
    }

    [RelayCommand]
    private void AddAlignLeftStyle()
    {
        TargetStyles.Add(new StringView(".aleft"));
    }

    [RelayCommand]
    private void AddAlignRightStyle()
    {
        TargetStyles.Add(new StringView(".aright"));
    }

    [RelayCommand]
    private void AddAlignCenterStyle()
    {
        TargetStyles.Add(new StringView(".centering"));
    }

    [RelayCommand]
    private void AddTextKaiStyle()
    {
        TargetStyles.Add(new StringView(".textkai"));
    }

    [RelayCommand]
    private void RemoveStyleItem(StringView item)
    {
        TargetStyles.Remove(item);
    }

    private void OnStylesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowNotifyAddStyle));
    }

    private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (TargetStyles.Count > 0)
        {
            StyleString = $"{{{string.Join(' ', TargetStyles)}}}";
        }
    }
}

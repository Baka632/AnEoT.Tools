using AnEoT.Tools.Shared.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AnEoT.Tools.VolumeCreator.Models;

namespace AnEoT.Tools.VolumeCreator.Views;

[INotifyPropertyChanged]
public sealed partial class EditorsInfoDialog : ContentDialog
{
    public EditorsInfo EditorsInfo { get; private set; }

    [ObservableProperty]
    private string editorString = string.Empty;
    [ObservableProperty]
    public string websiteLayoutDesigner = string.Empty;
    [ObservableProperty]
    private string illustrator = string.Empty;
    [ObservableProperty]
    private ObservableCollection<AdditionPart> additionalParts = [];

    public EditorsInfoDialog()
    {
        this.InitializeComponent();
    }

    [RelayCommand]
    private void AddAdditionalPart()
    {
        AdditionalParts.Add(new AdditionPart(string.Empty, string.Empty));
    }

    [RelayCommand]
    private void RemoveAdditionalPart(AdditionPart part)
    {
        AdditionalParts.Remove(part);
    }

    private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        EditorsInfo = new(EditorString, WebsiteLayoutDesigner, Illustrator, AdditionalParts.Select(part => part.ToTuple()));
    }
}

public record struct AdditionPart(StringView Item1, StringView Item2)
{
    public readonly (string, string) ToTuple()
    {
        return (Item1, Item2);
    }

    public static implicit operator (string, string)(AdditionPart value)
    {
        return (value.Item1, value.Item2);
    }

    public static implicit operator AdditionPart((string, string) value)
    {
        return new AdditionPart(value.Item1, value.Item2);
    }
}
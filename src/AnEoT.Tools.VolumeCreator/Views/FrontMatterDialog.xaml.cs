using AnEoT.Tools.Shared.Models;
using AnEoT.Tools.VolumeCreator.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace AnEoT.Tools.VolumeCreator.Views;

[INotifyPropertyChanged]
public sealed partial class FrontMatterDialog : ContentDialog
{
    public FrontMatter FrontMatter { get; private set; }

    [ObservableProperty]
    private string? articleTitle;
    [ObservableProperty]
    private string? articleShortTitle;
    [ObservableProperty]
    private string? iconString = "article";
    [ObservableProperty]
    private string? author;
    [ObservableProperty]
    private DateTimeOffset? articleDate = DateTimeOffset.UtcNow;
    [ObservableProperty]
    private ObservableCollection<StringView> categories = ["ÇëÊäÈëÄÚÈÝ"];
    [ObservableProperty]
    private ObservableCollection<StringView> tags = ["ÇëÊäÈëÄÚÈÝ"];
    [ObservableProperty]
    private int order = 1;
    [ObservableProperty]
    private string? description;

    public FrontMatterDialog()
    {
        this.InitializeComponent();
    }

    [RelayCommand]
    private void AddCategoryItem()
    {
        Categories.Add(new StringView());
    }

    [RelayCommand]
    private void RemoveCategoryItem(StringView view)
    {
        Categories.Remove(view);
    }

    [RelayCommand]
    private void AddTagItem()
    {
        Tags.Add(new StringView());
    }

    [RelayCommand]
    private void RemoveTagItem(StringView view)
    {
        Tags.Remove(view);
    }

    private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        FrontMatter = new FrontMatter()
        {
            Title = ArticleTitle ?? string.Empty,
            ShortTitle = ArticleShortTitle,
            Icon = IconString ?? string.Empty,
            Category = Categories.Select(view => view.StringContent),
            Tag = Tags.Select(view => view.StringContent),
            Date = (ArticleDate ?? DateTimeOffset.UtcNow).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Author = this.Author,
            Order = this.Order,
            Description = this.Description,
        };
    }
}

using AnEoT.Tools.Shared.Models;
using AnEoT.Tools.VolumeCreator.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using System.Collections.Specialized;
using Windows.Globalization.NumberFormatting;

namespace AnEoT.Tools.VolumeCreator.Views;

[INotifyPropertyChanged]
public sealed partial class FrontMatterDialog : ContentDialog
{
    public (FrontMatter, PredefinedCategory?) Result { get; private set; }
    public static readonly List<PredefinedCategoryWrapper> AvailablePredefinedCategories =
    [
        null,
        PredefinedCategory.News,
        PredefinedCategory.RhodesIslandJournal,
        PredefinedCategory.BeyondHere,
        PredefinedCategory.StoriesOfAfternoon,
        PredefinedCategory.QianHangYuanGe,
        PredefinedCategory.Paintings,
        PredefinedCategory.RhineLaboratory,
        PredefinedCategory.Intelligence,
    ];
    public static readonly string[] SuggestedIconStrings =
    [
        "community", "article", "palette", "note", "repo"
    ];

    public bool ShowNotifyAddCategory => Categories.Count <= 0;
    public bool ShowNotifyAddTags => Tags.Count <= 0;

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
    private ObservableCollection<StringView> categories = [];
    [ObservableProperty]
    private ObservableCollection<StringView> tags = [];
    [ObservableProperty]
    private int order = 1;
    [ObservableProperty]
    private string? description;
    [ObservableProperty]
    private PredefinedCategory? predefinedCategoryValue;
    [ObservableProperty]
    private int predefinedCategoryIndex = 0;
    [ObservableProperty]
    private bool isArticle = true;

    private void OnTagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowNotifyAddTags));
    }

    private void OnCategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowNotifyAddCategory));
    }

    partial void OnPredefinedCategoryIndexChanged(int oldValue, int newValue)
    {
        PredefinedCategoryWrapper oldWrapper = AvailablePredefinedCategories[oldValue == -1 ? 0 : oldValue];
        if (oldWrapper.PredefinedCategory.HasValue)
        {
            Categories.Remove(oldWrapper.PredefinedCategory.Value.AsCategoryString());
        }

        PredefinedCategoryValue = AvailablePredefinedCategories[newValue];
        if (PredefinedCategoryValue.HasValue)
        {
            Categories.Add(PredefinedCategoryValue.Value.AsCategoryString());
        }
    }

    public FrontMatterDialog()
    {
        this.InitializeComponent();
        Categories.CollectionChanged += OnCategoriesCollectionChanged;
        Tags.CollectionChanged += OnTagsCollectionChanged;

        DecimalFormatter formatter = new()
        {
            FractionDigits = 0,
            IsDecimalPointAlwaysDisplayed = false
        };

        OrderNumberBox.NumberFormatter = formatter;
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
    private void AddAuthorTagItem()
    {
        if (!string.IsNullOrWhiteSpace(Author) && !Tags.Contains(Author))
        {
            Tags.Add(new StringView(Author));
        }
    }

    [RelayCommand]
    private void RemoveTagItem(StringView view)
    {
        Tags.Remove(view);
    }

    private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        FrontMatter frontMatter = new()
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
            Article = IsArticle,
        };

        Result = (frontMatter, PredefinedCategoryValue);
    }

    private void OnIconStringAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        IconString = sender.Text;

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            SelectAutoSuggestBoxItemSource(sender);
        }
    }

    private void OnIconStringAutoSuggestBoxGotFocus(object sender, RoutedEventArgs e)
    {
        SelectAutoSuggestBoxItemSource((AutoSuggestBox)sender);
    }

    private static void SelectAutoSuggestBoxItemSource(AutoSuggestBox sender)
    {
        if (string.IsNullOrWhiteSpace(sender.Text))
        {
            sender.ItemsSource = SuggestedIconStrings;
        }
        else
        {
            sender.ItemsSource = SuggestedIconStrings.Where(suggestion => suggestion.StartsWith(sender.Text));
        }
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using AnEoT.Tools.Shared.Models;
using AnEoT.Tools.WordToMarkdown.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnEoT.Tools.WordToMarkdown.Views;

[INotifyPropertyChanged]
/// <summary>
/// FrontMatterInfoDialog.xaml 的交互逻辑
/// </summary>
public partial class FrontMatterInfoDialog : Window
{
    public FrontMatter FrontMatter { get; internal set; }

    [ObservableProperty]
    private string? articleTitle;
    [ObservableProperty]
    private string? articleShortTitle;
    [ObservableProperty]
    private string? iconString = "article";
    [ObservableProperty]
    private string? author;
    [ObservableProperty]
    private DateTime articleDate = DateTime.UtcNow;
    [ObservableProperty]
    private ObservableCollection<StringView> categories = ["请输入内容"];
    [ObservableProperty]
    private ObservableCollection<StringView> tags = ["请输入内容"];
    [ObservableProperty]
    private int order = 1;
    [ObservableProperty]
    private string? description;

    public FrontMatterInfoDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void OnOkButtonClicked(object sender, RoutedEventArgs e)
    {
        FrontMatter = new FrontMatter()
        {
            Title = ArticleTitle ?? string.Empty,
            ShortTitle = ArticleShortTitle,
            Icon = IconString ?? string.Empty,
            Category = Categories.Select(view => view.StringContent),
            Tag = Tags.Select(view => view.StringContent),
            Date = ArticleDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Author = this.Author,
            Order = this.Order,
            Description = this.Description,
        };

        DialogResult = true;
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
}

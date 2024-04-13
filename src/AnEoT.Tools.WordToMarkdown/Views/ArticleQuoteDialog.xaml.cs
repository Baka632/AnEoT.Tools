using System.Windows;
using AnEoT.Tools.WordToMarkdown.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.WordToMarkdown.Views;

[INotifyPropertyChanged]
/// <summary>
/// ArticleQuoteDialog.xaml 的交互逻辑
/// </summary>
public partial class ArticleQuoteDialog : Window
{
    public string? ArticleQuote { get => ArticleQuoteForBinding; }

    [ObservableProperty]
    private string? articleQuoteForBinding;

    public ArticleQuoteDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void OnOkButtonClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}

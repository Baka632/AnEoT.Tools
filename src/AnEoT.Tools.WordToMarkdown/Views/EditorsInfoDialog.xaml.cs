using System.Windows;
using AnEoT.Tools.WordToMarkdown.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.WordToMarkdown.Views;

[INotifyPropertyChanged]
/// <summary>
/// EditorsInfoDialog.xaml 的交互逻辑
/// </summary>
public partial class EditorsInfoDialog : Window
{
    public EditorsInfo EditorsInfo { get; internal set; }

    [ObservableProperty]
    private string editorString = string.Empty;
    [ObservableProperty]
    public string websiteLayoutDesigner = string.Empty;
    [ObservableProperty]
    private string illustrator = string.Empty;

    public EditorsInfoDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void OnOkButtonClicked(object sender, RoutedEventArgs e)
    {
        EditorsInfo = new EditorsInfo(EditorString, WebsiteLayoutDesigner, Illustrator);
        DialogResult = true;
    }
}

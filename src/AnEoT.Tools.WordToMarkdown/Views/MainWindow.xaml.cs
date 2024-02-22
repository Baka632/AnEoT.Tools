using System.Windows;
using AnEoT.Tools.WordToMarkdown.ViewModels;

namespace AnEoT.Tools.WordToMarkdown.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new MainViewModel();

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }
}
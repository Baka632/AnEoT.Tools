using System.Windows.Input;
using AnEoT.Tools.VolumeCreator.ViewModels;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class UtilitiesPage : Page
{
    public UtilitiyItem[] UtilitiyItems { get; }

    public UtilitiesViewModel ViewModel { get; } = new UtilitiesViewModel();

    public UtilitiesPage()
    {
        this.InitializeComponent();
        UtilitiyItems = [
            new("Lofter ͼ������", "\xEBD3", ViewModel.OpenLofterDownloadWindowCommand),
            new("ͼ��ת WebP ��ʽ", "\xE91B", ViewModel.OpenConvertWebPWindowCommand),
        ];
    }
}

public record struct UtilitiyItem(string Title, string IconGlyph, ICommand? Command = null);
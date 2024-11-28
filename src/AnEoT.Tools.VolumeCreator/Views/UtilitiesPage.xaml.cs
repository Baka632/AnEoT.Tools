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
            new("Lofter Í¼ÏñÏÂÔØ", "\xEBD3"),
            new("Í¼Ïñ×ª WebP", "\xE91B"),
        ];
    }
}

public record struct UtilitiyItem(string Title, string IconGlyph, ICommand? Command = null);
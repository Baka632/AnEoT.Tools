using System.Windows.Input;
using AnEoT.Tools.VolumeCreator.ViewModels;

namespace AnEoT.Tools.VolumeCreator.Views;

public sealed partial class UtilitiesPage : Page
{
    public UtilitiyItem[] UtilitiyItems { get; }

    public UtilitiesViewModel ViewModel { get; } = new UtilitiesViewModel();

    public UtilitiesPage()
    {
        this.InitializeComponent();
        UtilitiyItems = [
            new("图像下载", "\xEBD3", ViewModel.OpenLofterDownloadWindowCommand),
            new("图像格式转换", "\xE91B", ViewModel.OpenImageConvertWindowCommand),
        ];
    }
}

public record struct UtilitiyItem(string Title, string IconGlyph, ICommand? Command = null);
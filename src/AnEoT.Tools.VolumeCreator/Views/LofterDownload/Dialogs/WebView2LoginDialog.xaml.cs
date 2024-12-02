using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload.Dialogs;

[INotifyPropertyChanged]
public sealed partial class WebView2LoginDialog : ContentDialog
{
    private const string LofterUri = "https://www.lofter.com/";

    [ObservableProperty]
    private string? cookie;

    public WebView2LoginDialog()
    {
        this.InitializeComponent();
    }

    partial void OnCookieChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Contains("NTES_PASSPORT"))
        {
            Hide();
        }
    }

    private async void OnWebViewNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        StringBuilder stringBuilder = new();

        IReadOnlyList<CoreWebView2Cookie> cookies = await sender.CoreWebView2.CookieManager.GetCookiesAsync(LofterUri);
        foreach (CoreWebView2Cookie cookie in cookies)
        {
            stringBuilder.Append($"{cookie.Name}={cookie.Value}; ");
        }

        Cookie = stringBuilder.ToString().TrimEnd();
    }

    private void OnDialogUnloaded(object sender, RoutedEventArgs e)
    {
        WebView?.Close();
    }
}
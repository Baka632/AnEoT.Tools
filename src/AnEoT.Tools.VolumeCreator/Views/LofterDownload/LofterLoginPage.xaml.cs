using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AnEoT.Tools.VolumeCreator.Models.Lofter;
using AnEoT.Tools.VolumeCreator.Views.LofterDownload.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Web.WebView2.Core;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

[INotifyPropertyChanged]
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LofterLoginPage : Page
{
    private LofterDownloadWindowAccessor windowAccessor;
    private LofterCookieProvider cookieProvider;

    private bool blockTextBoxCookieProvider;
    private bool triedCookieInitialization;

    [ObservableProperty]
    private bool isInitializingCookie = true;
    [ObservableProperty]
    private bool targetUriSelected;
    [ObservableProperty]
    private bool cookieSelected;

#pragma warning disable CS8618 // OnNavigatedTo »á³öÊÖ
    public LofterLoginPage()
#pragma warning restore CS8618
    {
        this.InitializeComponent();
        cookieProvider = new TextBoxCookieProvider(CookieTextBox);
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    partial void OnCookieSelectedChanged(bool value)
    {
        DetermineCanMoveNext();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is LofterDownloadWindowAccessor accessor)
        {
            windowAccessor = accessor;
        }
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        DetermineCanMoveNext();
        await TryLoadCookie();
    }

    private void OnWebsiteAddressTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (ValidateWebsiteAddress(out Uri? websiteAddress))
        {
            windowAccessor.DownloadData = windowAccessor.DownloadData with { PageUri = websiteAddress };
            TargetUriSelected = true;
            WarningInfoBar.IsOpen = false;
        }
        else
        {
            TargetUriSelected = false;
            WarningInfoBar.IsOpen = true;
        }

        DetermineCanMoveNext();
    }

    private bool ValidateWebsiteAddress([NotNullWhen(true)] out Uri? websiteAddress)
    {
        string uriString = WebsiteAddressTextBox.Text.Trim();
        return Uri.TryCreate(uriString, UriKind.Absolute, out websiteAddress)
                    && (uriString.StartsWith("http://") || uriString.StartsWith("https://"));
    }

    private void DetermineCanMoveNext()
    {
        windowAccessor.EnableForward = TargetUriSelected && CookieSelected;
    }

    private async void OnCookieTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (blockTextBoxCookieProvider)
        {
            return;
        }

        if (cookieProvider is not TextBoxCookieProvider textBoxCookieProvider || textBoxCookieProvider.TextBox != CookieTextBox)
        {
            cookieProvider = new TextBoxCookieProvider(CookieTextBox);
        }

        await cookieProvider.InitalizeCookieAsync();

        if (cookieProvider.VerfiyCookie())
        {
            windowAccessor.DownloadData = windowAccessor.DownloadData with { LofterCookie = cookieProvider.Cookie };
            CookieSelected = true;
        }
        else
        {
            CookieSelected = false;
        }
    }

    private async void OnInAppLoginSettingsCardClick(object sender, RoutedEventArgs e)
    {
        WebView2LoginDialog dialog = new()
        {
            XamlRoot = XamlRoot
        };

        cookieProvider = new WebView2CookieProvider(dialog);

        await cookieProvider.InitalizeCookieAsync();

        if (cookieProvider.VerfiyCookie())
        {
            blockTextBoxCookieProvider = true;
            CookieTextBox.Text = string.Empty;
            blockTextBoxCookieProvider = false;
            windowAccessor.DownloadData = windowAccessor.DownloadData with { LofterCookie = cookieProvider.Cookie };
            CookieSelected = true;
        }
        else
        {
            CookieSelected = false;
        }
    }

    private async Task TryLoadCookie()
    {
        if (triedCookieInitialization)
        {
            return;
        }

        IsInitializingCookie = true;

        StringBuilder stringBuilder = new();
        WebView2 webView2 = new();
        try
        {
            await webView2.EnsureCoreWebView2Async();
            IReadOnlyList<CoreWebView2Cookie> cookies = await webView2.CoreWebView2.CookieManager.GetCookiesAsync("https://www.lofter.com/");
            foreach (CoreWebView2Cookie cookie in cookies)
            {
                stringBuilder.Append($"{cookie.Name}={cookie.Value}; ");
            }

            string cookieString = stringBuilder.ToString().TrimEnd();

            if (LofterCookieProvider.VerfiyCookieCore(cookieString))
            {
                windowAccessor.DownloadData = windowAccessor.DownloadData with { LofterCookie = cookieString };
                CookieSelected = true;
            }
        }
        finally
        {
            webView2.Close();
            IsInitializingCookie = false;
            triedCookieInitialization = true;
        }
    }

    private async void OnClearLofterLoginButtonClick(object sender, RoutedEventArgs e)
    {
        await ClearWebViewCookieAsync();
        CookieTextBox.Text = string.Empty;
        CookieSelected = false;
    }

    private static async Task ClearWebViewCookieAsync()
    {
        WebView2 webView2 = new();

        try
        {
            await webView2.EnsureCoreWebView2Async();
            webView2.CoreWebView2.CookieManager.DeleteAllCookies();
        }
        finally
        {
            webView2.Close();
        }
    }
}

internal abstract class LofterCookieProvider
{
    public string? Cookie { get; set; }

    public abstract Task InitalizeCookieAsync();

    public bool VerfiyCookie()
    {
        return VerfiyCookieCore(Cookie);
    }

    public static bool VerfiyCookieCore(string? cookie)
    {
        return !string.IsNullOrWhiteSpace(cookie) && cookie.Contains("NTES_PASSPORT");
    }
}

internal sealed class WebView2CookieProvider(WebView2LoginDialog dialog) : LofterCookieProvider
{
    public async override Task InitalizeCookieAsync()
    {
        await dialog.ShowAsync();
        Cookie = dialog.Cookie;
    }
}

internal sealed class TextBoxCookieProvider(TextBox textBox) : LofterCookieProvider
{
    public TextBox TextBox { get; } = textBox;

    public override Task InitalizeCookieAsync()
    {
        Cookie = TextBox.Text;
        return Task.CompletedTask;
    }
}

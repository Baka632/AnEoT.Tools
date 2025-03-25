using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using AnEoT.Tools.VolumeCreator.Models.Lofter;
using AnEoT.Tools.VolumeCreator.Views.LofterDownload.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnEoT.Tools.VolumeCreator.Views.LofterDownload;

[INotifyPropertyChanged]
public sealed partial class LofterLoginPage : Page
{
    private LofterDownloadWindowAccessor windowAccessor;
    private LofterCookieProvider cookieProvider;

    private bool blockTextBoxCookieProvider;
    private bool allowWithoutCookie;

    [ObservableProperty]
    private bool targetUriSelected;
    [ObservableProperty]
    private bool cookieSelected;
    [ObservableProperty]
    private bool showLofterLogin;

#pragma warning disable CS8618 // OnNavigatedTo 会出手
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

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        DetermineCanMoveNext();
    }

    private void OnWebsiteAddressTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (ValidateWebsiteAddress(out Uri? websiteAddress))
        {
            windowAccessor.DownloadData = windowAccessor.DownloadData with { PageUri = websiteAddress };
            TargetUriSelected = true;
            UriErrorInfoBar.IsOpen = false;

            if (!CommonValues.GetLofterDomainVerifyRegex().IsMatch(websiteAddress.Host))
            {
                allowWithoutCookie = true;
                ShowLofterLogin = false;
            }
            else
            {
                // 是 Lofter
                allowWithoutCookie = false;
                ShowLofterLogin = true;
            }
        }
        else
        {
            TargetUriSelected = false;
            UriErrorInfoBar.IsOpen = true;
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
        windowAccessor.EnableForward = TargetUriSelected && (CookieSelected || allowWithoutCookie);
    }

    private void SetCookie(string? cookie)
    {
        windowAccessor.DownloadData = windowAccessor.DownloadData with { LofterCookie = cookie };
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
            SetCookie(cookieProvider.Cookie);
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
            SetCookie(cookieProvider.Cookie);
            CookieSelected = true;
        }
        else
        {
            CookieSelected = false;
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

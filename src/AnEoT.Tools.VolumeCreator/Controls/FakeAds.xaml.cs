using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.System;
using Microsoft.UI.Xaml.Markup;

namespace AnEoT.Tools.VolumeCreator.Controls;

[INotifyPropertyChanged]
public sealed partial class FakeAds : UserControl
{
    private static readonly Uri AnEoTWebsiteUri = new("https://aneot.terrach.net/");
    private static readonly Uri FakeAdImageUri = new("ms-appx:///Assets/fake-ads/");

    [ObservableProperty]
    private bool showCustomAdXaml;
    [ObservableProperty]
    private string? adTips = string.Empty;
    [ObservableProperty]
    private string? adAbout = string.Empty;
    [ObservableProperty]
    private Uri? adImageUri = null;
    private Uri? adUrl = null;
    private Uri? adAboutUrl = null;

    public FakeAds()
    {
        this.InitializeComponent();
    }

    private async void OnUserControlLoaded(object sender, RoutedEventArgs e)
    {
        await RollAd();
    }

    private async Task RollAd()
    {
        bool predicate = Random.Shared.NextDouble() > 0.3;
        if (predicate)
        {
            ShowCustomAdXaml = false;

            FakeAdInfo info = await FakeAdHelper.RollFakeAdAsync();
            AdTips = info.AdText;
            AdAbout = info.AdAbout;

            if (string.IsNullOrWhiteSpace(info.AboutLink))
            {
                adAboutUrl = null;
            }
            else
            {
                Uri aboutUri = new(info.AboutLink, UriKind.RelativeOrAbsolute);
                adAboutUrl = aboutUri.IsAbsoluteUri
                    ? aboutUri
                    : new Uri(AnEoTWebsiteUri, aboutUri);
            }

            if (string.IsNullOrWhiteSpace(info.AdLink))
            {
                adUrl = null;
            }
            else
            {
                Uri adUri = new(info.AdLink, UriKind.RelativeOrAbsolute);
                adUrl = adUri.IsAbsoluteUri
                    ? adUri
                    : new Uri(AnEoTWebsiteUri, adUri);
            }

            if (string.IsNullOrWhiteSpace(info.AdImageLink))
            {
                AdImageUri = null;
            }
            else
            {
                Uri adImageUri = new(info.AdImageLink, UriKind.RelativeOrAbsolute);
                AdImageUri = adImageUri.IsAbsoluteUri
                    ? adImageUri
                    : new Uri(FakeAdImageUri, adImageUri);
            }
        }
        else
        {
            ShowCustomAdXaml = true;
            bool select1Predicate = Random.Shared.NextDouble() > 0.5;

            if (select1Predicate)
            {
                AdTips = "此处可能展示催更宣言";
                AdAbout = "快点更新啦！";
            }
            else
            {
                AdTips = "此处可能...额，临时广告？";
                AdAbout = "查看更多（但是点了也没有用）";
            }

            adUrl = null;
            adAboutUrl = null;

            string target = select1Predicate ? CustomAdXaml1 : CustomAdXaml2;
            UIElement element = (UIElement)XamlReader.Load(target);
            CustomXamlAdViewbox.Child = element;
        }
    }

    private async void OnAdContentGridTapped(object sender, TappedRoutedEventArgs e)
    {
        if (adUrl is not null)
        {
            await Launcher.LaunchUriAsync(adUrl);
        }
    }

    private async void OnHyperlinkClicked(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
    {
        if (adAboutUrl is not null)
        {
            await Launcher.LaunchUriAsync(adAboutUrl);
        }
    }

    private const string CustomAdXaml1 = """
        <Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
            <Grid.RowDefinitions>
                <RowDefinition />
                <RowDefinition />
            </Grid.RowDefinitions>
            <Grid.ColumnDefinitions>
                <ColumnDefinition />
                <ColumnDefinition Width="200" />
                <ColumnDefinition />
            </Grid.ColumnDefinitions>
            <Grid.Resources>
                <Style TargetType="TextBlock">
                    <Setter Property="FontSize" Value="20" />
                    <Setter Property="FontWeight" Value="Bold" />
                    <Setter Property="FontStyle" Value="Italic" />
                    <Setter Property="Foreground" Value="OrangeRed" />
                </Style>
            </Grid.Resources>
        
            <TextBlock Grid.Row="0" Grid.Column="0" Text="我现在就想" />
            <TextBlock Grid.Row="1" Grid.Column="2" Text="看到回归线的新刊啊！" />
        </Grid> 
        """;
    
    private const string CustomAdXaml2 = """
        <TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" FontSize="20" FontWeight="Bold" Foreground="Red" TextDecorations="Underline">
            <Run>你们一定要看回归线啊！！！</Run>
            <LineBreak />
            <Span Foreground="AliceBlue">一定要看</Span>
            <Span Foreground="SkyBlue">泰拉最大</Span>
            <Span Foreground="Orange">的杂志社出版的</Span>
            <Span Foreground="LightCoral">回↗归↘线↗啊！！！↝</Span>
        </TextBlock>
        """;
}
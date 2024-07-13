#pragma warning disable CS8618

using System.Collections.ObjectModel;
using AnEoT.Tools.VolumeCreator.Helpers;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MarkdownEditPage : Page
{
    public MarkdownEditViewModel ViewModel { get; private set; }

    public MarkdownEditPage()
    {
        this.InitializeComponent();
        MarkdownRenderTextBlock.SetRenderer<AnEoTMarkdownRenderer>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is ValueTuple<MarkdownWrapper, ObservableCollection<ImageListNode>, StorageFile?> tuple)
        {
            ViewModel = new MarkdownEditViewModel(tuple.Item1, this, tuple.Item2, tuple.Item3);
        }

        base.OnNavigatedTo(e);
    }

    private void OnCloseArticleQuoteFlyoutButtonClicked(object sender, RoutedEventArgs e)
    {
        ArticleQuoteFlyout.Hide();
    }

    private void OnFileTreeViewItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is FileNode node)
        {
            ViewModel.InsertImageToText(MarkdownTextBox, node);
        }
    }

    private async void OnMarkdownRenderTextBlockImageResolving(object sender, CommunityToolkit.WinUI.UI.Controls.ImageResolvingEventArgs e)
    {
        if (ViewModel.MarkdownImageUriToFileMapping.TryGetValue(e.Url, out StorageFile? file))
        {
            Deferral deferral = e.GetDeferral();
            BitmapImage image = new();

            IRandomAccessStreamWithContentType source = await file.OpenReadAsync();
            await image.SetSourceAsync(source);

            e.Image = image;
            e.Handled = true;

            deferral.Complete();
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debugger.Break();
        }
#endif
    }

    private void OnMarkdownTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        MarkdownTextBox.ContextFlyout.Opening += OnMarkdownTextBoxFlyoutOpening;
    }

    private void OnMarkdownTextBoxUnloaded(object sender, RoutedEventArgs e)
    {
        MarkdownTextBox.ContextFlyout.Opening -= OnMarkdownTextBoxFlyoutOpening;
    }

    private void OnMarkdownTextBoxFlyoutOpening(object? sender, object e)
    {
        CommandBarFlyout? flyout = sender as CommandBarFlyout;
        if (flyout?.Target == MarkdownTextBox)
        {
            PathIcon eodIcon = (PathIcon)XamlReader.Load("""<PathIcon xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Data="M 161.03 154.06 305.81 154.06 305.81 240.52 371.61 240.52 371.61 154.06 483.87 154.06 516.39 92.13 53.27 93.53 339.1 603.1 681.29 0 0 0 34.21 60.53 569.03 61.16 371.61 411.87 370.84 278.71 305.81 279.23 305.81 412.39 161.03 154.06 Z" HorizontalAlignment="Center" />""");

            AppBarButton addEod = new()
            {
                Command = ViewModel.AddEodTagToTextCommand,
                CommandParameter = MarkdownTextBox,
                Label = "插入 <eod /> 标签",
                Icon = eodIcon,
            };

            AppBarButton addStyle = new()
            {
                Label = "插入常用样式",
                Icon = new FontIcon() { Glyph = "\uE943" },
                Flyout = new MenuBarItemFlyout
                {
                    Items =
                    {
                        new MenuFlyoutItem()
                        {
                            Command = ViewModel.InsertAlignLeftCommand,
                            CommandParameter = MarkdownTextBox,
                            Icon = new SymbolIcon(Symbol.AlignLeft),
                            Text = "居左"
                        },
                        new MenuFlyoutItem()
                        {
                            Command = ViewModel.InsertAlignCenterCommand,
                            CommandParameter = MarkdownTextBox,
                            Icon = new SymbolIcon(Symbol.AlignCenter),
                            Text = "居中"
                        },
                        new MenuFlyoutItem()
                        {
                            Command = ViewModel.InsertAlignRightCommand,
                            CommandParameter = MarkdownTextBox,
                            Icon = new SymbolIcon(Symbol.AlignRight),
                            Text = "居右"
                        },
                        new MenuFlyoutSeparator(),
                        new MenuFlyoutItem()
                        {
                            Command = ViewModel.InsertTextKaiCommand,
                            CommandParameter = MarkdownTextBox,
                            Icon = new FontIcon() { Glyph = "\uF17F" },
                            Text = "楷体字体"
                        },
                    }
                }
            };

            flyout.SecondaryCommands.Add(new AppBarSeparator());
            flyout.SecondaryCommands.Add(addEod);
            flyout.SecondaryCommands.Add(addStyle);
        }
    }
}

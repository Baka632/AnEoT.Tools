#pragma warning disable CS8618

using System.Collections.ObjectModel;
using AnEoT.Tools.VolumeCreator.Helpers;
using AnEoT.Tools.VolumeCreator.Models;
using AnEoT.Tools.VolumeCreator.ViewModels;
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
        if (e.Parameter is ValueTuple<MarkdownWrapper, ObservableCollection<ImageListNode>> tuple)
        {
            ViewModel = new MarkdownEditViewModel(tuple.Item1, this, tuple.Item2);
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
    }
}

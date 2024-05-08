using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel.DataTransfer;
using AnEoT.Tools.VolumeCreator.ViewModels;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ContentPage : Page
{
    public ContentPageViewModel ViewModel { get; } = new ContentPageViewModel();

    public ContentPage()
    {
        this.InitializeComponent();
    }

    private void OnCoverButtonDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.Bitmap) || e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = "设置期刊封面";
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void OnCoverButtonDrop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.Bitmap))
        {
            RandomAccessStreamReference reference = await e.DataView.GetBitmapAsync();
            IRandomAccessStreamWithContentType stream = await reference.OpenReadAsync();
            await ViewModel.SetCoverByStream(stream);
            return;
        }
        else if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0 && items.All(item => item.IsOfType(StorageItemTypes.File)))
            {
                StorageFile storageFile = (StorageFile)items[0];

                if (storageFile.ContentType.Contains("image", StringComparison.OrdinalIgnoreCase))
                {
                    await ViewModel.SetCoverByFile(storageFile);
                    return;
                }
            }
        }

        ViewModel.IsVolumeCoverError = true;
    }

    private void OnWordFileListDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = "添加 DOCX 文件";
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void OnWordFileListDrop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
            foreach (StorageFile file in items.Where(item => item.IsOfType(StorageItemTypes.File))
                                              .Select(item => (StorageFile)item))
            {
                await ViewModel.AddSingleWordFileItem(file);
            }
        }
    }
}

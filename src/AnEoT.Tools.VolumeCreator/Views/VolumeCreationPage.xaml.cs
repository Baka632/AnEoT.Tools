using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using AnEoT.Tools.VolumeCreator.ViewModels;
using AnEoT.Tools.VolumeCreator.Models;

namespace AnEoT.Tools.VolumeCreator.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class VolumeCreationPage : Page
{
    public VolumeCreationPageViewModel ViewModel { get; }

    public VolumeCreationPage()
    {
        ViewModel = new VolumeCreationPageViewModel(this);
        this.InitializeComponent();
    }

    private void OnCoverButtonDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
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
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
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

    private void OnArticlesListDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = "导入 DOCX 文件";
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void OnArticlesListDrop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
            foreach (StorageFile file in items.Where(item => item.IsOfType(StorageItemTypes.File))
                                              .Select(item => (StorageFile)item))
            {
                await ViewModel.ImportSingleWordFileItem(file);
            }
        }
    }

    private void OnFolderNodeItemDragOver(object sender, DragEventArgs e)
    {
        TreeViewItem item = (TreeViewItem)sender;
        FolderNode node = (FolderNode)item.DataContext;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = $"添加到 {node.DisplayName} 中";
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void OnFolderNodeItemDrop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            TreeViewItem item = (TreeViewItem)sender;
            FolderNode node = (FolderNode)item.DataContext;

            IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
            foreach (StorageFile file in items.Where(item => item.IsOfType(StorageItemTypes.File))
                                              .Select(item => (StorageFile)item))
            {
                ViewModel.Assets.Add(new FileNode(file, node));
            }
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await ParseParameter(e.Parameter);
    }

    public async Task ParseParameter(object? parameter)
    {
        if (parameter is IReadOnlyList<IStorageItem> files && files.Count > 0)
        {
            // 只要第一个
            StorageFile? file = files[0] as StorageFile;
            await ViewModel.LoadProject(file);
        }
    }

    private void OnAddNewAssetMenuFlyoutItemClick(object sender, RoutedEventArgs e)
    {
        FolderNode folderNode = (FolderNode)((MenuFlyoutItem)sender).DataContext;
        ViewModel.AddAssetCommand.Execute(folderNode);
    }

    private void OnRepairAssetButtonClick(object sender, RoutedEventArgs e)
    {
        FileNode fileNode = (FileNode)((Button)sender).DataContext;
        ViewModel.RepairAssetCommand.Execute(fileNode);
    }

    private void OnRemoveAssetButtonClick(object sender, RoutedEventArgs e)
    {
        FileNode fileNode = (FileNode)((Button)sender).DataContext;
        ViewModel.RemoveAssetCommand.Execute(fileNode);
    }

    private void OnAddNewAssetFolderMenuFlyoutItemClick(object sender, RoutedEventArgs e)
    {
        FolderNode folderNode = (FolderNode)((MenuFlyoutItem)sender).DataContext;
        ViewModel.AddAssetFolderCommand.Execute(folderNode);
    }

    private void OnRemoveAssetFolderButtonClick(object sender, RoutedEventArgs e)
    {
        FolderNode folderNode = (FolderNode)((Button)sender).DataContext;
        ViewModel.RemoveAssetFolderCommand.Execute(folderNode);
    }
}

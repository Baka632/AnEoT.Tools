using AnEoT.Tools.VolumeCreator.Models;

namespace AnEoT.Tools.VolumeCreator.Helpers;

public sealed class AssetNodeDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FileTemplate { get; set; }
    public DataTemplate? FolderTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        AssetNode model = (AssetNode)item;

        return model.Type switch
        {
            ImageListNodeType.File => FileTemplate!,
            ImageListNodeType.Folder => FolderTemplate!,
            _ => throw new NotImplementedException(),
        };
    }
}

using AnEoT.Tools.VolumeCreator.Models;

namespace AnEoT.Tools.VolumeCreator.Helpers;

public sealed class ImageNodeDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FileTemplate { get; set; }
    public DataTemplate? FolderTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        ImageListNode model = (ImageListNode)item;

        return model.Type switch
        {
            ImageListNodeType.File => FileTemplate!,
            ImageListNodeType.Folder => FolderTemplate!,
            _ => throw new NotImplementedException(),
        };
    }
}

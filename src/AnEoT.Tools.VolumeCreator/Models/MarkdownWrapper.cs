using Windows.Storage;

namespace AnEoT.Tools.VolumeCreator.Models;

public sealed class MarkdownWrapper(StorageFile file, string markdown)
{
    public StorageFile File { get; } = file;

    public string Markdown { get; set; } = markdown;
}
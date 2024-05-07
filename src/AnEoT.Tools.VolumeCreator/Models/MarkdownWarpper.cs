using Windows.Storage;

namespace AnEoT.Tools.VolumeCreator.Models;

public sealed record MarkdownWarpper(StorageFile File, string Markdown);
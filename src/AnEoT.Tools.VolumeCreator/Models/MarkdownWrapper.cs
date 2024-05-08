using Windows.Storage;

namespace AnEoT.Tools.VolumeCreator.Models;

public sealed record MarkdownWrapper(StorageFile File, string Markdown);
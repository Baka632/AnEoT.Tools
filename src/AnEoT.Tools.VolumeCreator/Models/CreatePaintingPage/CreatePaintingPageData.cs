namespace AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;

public sealed record CreatePaintingPageData(
    IEnumerable<AssetNode> OriginalAssets,
    IEnumerable<FileNode> FileAssets,
    Action<string>? SetGeneratedPaintingPageMarkdown);
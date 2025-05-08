namespace AnEoT.Tools.VolumeCreator.Models.CreatePaintingPage;

public sealed record CreatePaintingPageData(
    IEnumerable<AssetNode> OriginalAssets,
    Action<string>? SetGeneratedPaintingPageMarkdown,
    bool ConvertWebP,
    IEnumerable<FileNode> FileAssets,
    IEnumerable<PaintingInfo> PaintingInfos,
    string GeneratedMarkdown);
namespace AnEoT.Tools.VolumeCreator.Models.Lofter;

public sealed record LofterDownloadData(
    Uri? PageUri,
    string? LofterCookie,
    IEnumerable<LofterImageInfo>? ImageInfos,
    LofterDownloadOptions DownloadOptions);

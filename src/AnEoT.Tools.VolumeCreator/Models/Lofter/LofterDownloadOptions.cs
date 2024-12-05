namespace AnEoT.Tools.VolumeCreator.Models.Lofter;

public readonly record struct LofterDownloadOptions(
    string SavePath,
    bool ConvertWebP,
    bool TrimUriQueryPart);
namespace AnEoT.Tools.VolumeCreator.Models.Lofter;

public record struct LofterDownloadOptions(
    string SavePath,
    bool ConvertWebP,
    bool TrimUriQueryPart);
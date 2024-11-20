using System.Diagnostics.CodeAnalysis;
using Windows.Storage;

namespace AnEoT.Tools.VolumeCreator.Models.Resources;

public interface IVoulmeResourcesHelper
{
    public bool ConvertWebP { get; set; }
    public bool HasCover { get; }

    Task<Stream?> GetCoverAsync();

    Task SetCoverAsync(string coverPath);

    Task<Stream?> GetAssetsAsync(FileNode fileNode);

    Task ExportAssetsAsync(IEnumerable<ImageListNode> files, StorageFolder outputFolder);

    bool ValidateAssets(IEnumerable<ImageListNode> files, [NotNullWhen(false)] out string? errorMessage);
}
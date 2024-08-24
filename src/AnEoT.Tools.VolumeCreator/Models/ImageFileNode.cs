using Windows.Storage;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace AnEoT.Tools.VolumeCreator.Models;

[JsonDerivedType(typeof(FileNode), "file")]
[JsonDerivedType(typeof(FolderNode), "folder")]
public abstract class ImageListNode
{
    public string DisplayName { get; set; } = string.Empty;

    public ObservableCollection<ImageListNode> Children { get; set; } = [];

    public ImageListNode? Parent { get; set; } = null;

    public ImageListNodeType Type { get; set; }

    public override string ToString() => DisplayName;
}

public sealed class FileNode : ImageListNode
{
    public required StorageFile File { get; set; }

    [SetsRequiredMembers]
    public FileNode(StorageFile file, ImageListNode? parent)
    {
        File = file;
        DisplayName = file.Name;
        Type = ImageListNodeType.File;
        Parent = parent;
    }

    public FileNode()
    {
        Type = ImageListNodeType.File;
    }
}

public sealed class FolderNode : ImageListNode
{
    public FolderNode(string folderName, ImageListNode? parent)
    {
        DisplayName = folderName;
        Type = ImageListNodeType.Folder;
        Parent = parent;
    }

    public FolderNode()
    {
        Type = ImageListNodeType.Folder;
    }
}

public enum ImageListNodeType
{
    /// <summary>
    /// 文件节点
    /// </summary>
    File,
    /// <summary>
    /// 文件夹节点
    /// </summary>
    Folder,
}

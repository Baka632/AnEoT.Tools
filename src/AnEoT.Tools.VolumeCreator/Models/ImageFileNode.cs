﻿using Windows.Storage;
using System.Collections.ObjectModel;

namespace AnEoT.Tools.VolumeCreator.Models;

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
    public StorageFile File { get; }

    public FileNode(StorageFile file, ImageListNode? parent)
    {
        File = file;
        DisplayName = file.Name;
        Type = ImageListNodeType.File;
        Parent = parent;
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
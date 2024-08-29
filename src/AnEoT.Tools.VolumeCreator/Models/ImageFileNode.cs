using Windows.Storage;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnEoT.Tools.VolumeCreator.Models;

[JsonDerivedType(typeof(FileNode), "file")]
[JsonDerivedType(typeof(FolderNode), "folder")]
public abstract class ImageListNode : INotifyPropertyChanged
{
    private string _displayName = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            OnPropertiesChanged();
        }
    }

    public ObservableCollection<ImageListNode> Children { get; set; } = [];

    public ImageListNode? Parent { get; set; } = null;

    public ImageListNodeType Type { get; set; }

    public override string ToString() => DisplayName;

    /// <summary>
    /// 通知运行时属性已经发生更改
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        ((App)Application.Current).RunOnUIThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }
}

public sealed class FileNode : ImageListNode
{
    private string _filePath = string.Empty;

    public required string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertiesChanged();
            OnPropertiesChanged(nameof(IsFileExist));
        }
    }

    [JsonIgnore]
    public bool IsFileExist { get => File.Exists(FilePath); }

    [SetsRequiredMembers]
    public FileNode(StorageFile file, ImageListNode? parent) : this(file.Path, file.Name, parent)
    {
    }
    
    [SetsRequiredMembers]
    public FileNode(string filePath, string displayName, ImageListNode? parent)
    {
        FilePath = filePath;
        DisplayName = displayName;
        Type = ImageListNodeType.File;
        Parent = parent;
    }

    public FileNode()
    {
        Type = ImageListNodeType.File;
    }

    public void EnsurePathExist()
    {
        OnPropertiesChanged(nameof(IsFileExist));
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

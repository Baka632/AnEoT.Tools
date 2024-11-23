using Windows.Storage;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnEoT.Tools.VolumeCreator.Models;

[JsonDerivedType(typeof(FileNode), "file")]
[JsonDerivedType(typeof(FolderNode), "folder")]
public abstract class AssetNode : INotifyPropertyChanged
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

    public ObservableCollection<AssetNode> Children { get; set; } = [];

    public AssetNode? Parent { get; set; } = null;

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

public sealed class FileNode : AssetNode
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

    /// <summary>
    /// 确定文件路径（<see cref="FilePath"/>）是否为相对路径。
    /// </summary>
    public bool IsRelativePath { get; set; }

    /// <summary>
    /// 确定 <see cref="FilePath"/> 指向的路径是否存在。
    /// </summary>
    /// <remarks>
    /// 如果 <see cref="IsRelativePath"/> 为 <see langword="true"/>，则此属性始终返回 <see langword="true"/>。
    /// </remarks>
    [JsonIgnore]
    public bool IsFileExist { get => IsRelativePath || File.Exists(FilePath); }

    [SetsRequiredMembers]
    public FileNode(StorageFile file, AssetNode? parent) : this(file.Path, false, file.Name, parent)
    {
    }
    
    [SetsRequiredMembers]
    public FileNode(string filePath, bool isRelativePath, string displayName, AssetNode? parent) : this()
    {
        FilePath = filePath;
        IsRelativePath = isRelativePath;
        DisplayName = displayName;
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

public sealed class FolderNode : AssetNode
{
    public FolderNode(string folderName, AssetNode? parent)
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

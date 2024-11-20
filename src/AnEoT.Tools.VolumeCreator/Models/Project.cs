using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace AnEoT.Tools.VolumeCreator.Models;

[Obsolete("请使用 ProjectPackage。")]
public sealed class Project
{
    public Project()
    {
    }

    [SetsRequiredMembers]
    public Project(string coverImagePath,
                        bool imageConvertToWebp,
                        bool isCoverSizeFixed,
                        string volumeFolderName,
                        string volumeName,
                        ObservableCollection<MarkdownWrapper> wordFiles,
                        ObservableCollection<ImageListNode> imageFiles,
                        MarkdownWrapper? indexMarkdown)
    {
        CoverImagePath = coverImagePath;
        ImageConvertToWebp = imageConvertToWebp;
        IsCoverSizeFixed = isCoverSizeFixed;
        VolumeFolderName = volumeFolderName;
        VolumeName = volumeName;
        WordFiles = wordFiles;
        ImageFiles = imageFiles;
        IndexMarkdown = indexMarkdown;
    }

    /// <summary>
    /// 期刊封面图像路径
    /// </summary>
    public required string CoverImagePath { get; set; }
    
    /// <summary>
    /// 指示图像是否转换为 WEBP 图像的值
    /// </summary>
    public required bool ImageConvertToWebp { get; set; }

    /// <summary>
    /// 指示封面图像大小是否固定的值
    /// </summary>
    public required bool IsCoverSizeFixed { get; set; }

    /// <summary>
    /// 期刊文件夹名称
    /// </summary>
    public required string VolumeFolderName { get; set; }

    /// <summary>
    /// 期刊名称
    /// </summary>
    public required string VolumeName { get; set; }

    /// <summary>
    /// DOCX 文件列表
    /// </summary>
    public required ObservableCollection<MarkdownWrapper> WordFiles { get; set; }

    /// <summary>
    /// 图像文件列表
    /// </summary>
    public required ObservableCollection<ImageListNode> ImageFiles { get; set; }

    /// <summary>
    /// 目录页
    /// </summary>
    public required MarkdownWrapper? IndexMarkdown { get; set; }

    public static Project CreateEmpty()
    {
        Project project = new()
        {
            VolumeName = string.Empty,
            VolumeFolderName = string.Empty,
            CoverImagePath = string.Empty,
            ImageConvertToWebp = true,
            IsCoverSizeFixed = true,
            WordFiles = [],
            ImageFiles = [],
            IndexMarkdown = null
        };

        return project;
    }
}

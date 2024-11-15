namespace AnEoT.Tools.VolumeCreator.Models;

/// <summary>
/// 描述项目包信息的结构。
/// </summary>
/// <param name="VolumeName">期刊名称。</param>
/// <param name="VolumeFolderName">期刊文件夹名称。</param>
/// <param name="IsCoverSizeFixed">指示封面图像大小是否固定的值。</param>
/// <param name="ImageConvertToWebp">指示图像是否转换为 WebP 图像的值。</param>
/// <param name="CoverImageName">封面图像的文件名。</param>
public record struct ProjectInfo(string VolumeName,
                                        string VolumeFolderName,
                                        bool IsCoverSizeFixed,
                                        bool ImageConvertToWebp,
                                        string? CoverImageName);
using System.Text.Json.Serialization;

namespace AnEoT.Tools.VolumeCreator.Models;

/// <summary>
/// 为「泰拉广告」提供信息的结构
/// </summary>
/// <param name="AdText"> 广告显示文本 </param>
/// <param name="AdAbout"> 广告帮助信息文本 </param>
/// <param name="AdLink"> 广告链接 </param>
/// <param name="AboutLink"> 广告帮助信息链接 </param>
/// <param name="AdImageLink"> 广告图像链接 </param>
public readonly record struct FakeAdInfo(
    string? AdText,
    string? AdAbout,
    string? AdLink,
    string? AboutLink,
    string? AdImageLink);

/// <summary>
/// 记录「泰拉广告」相关参数的结构
/// </summary>
/// <param name="Probability">选中这个「泰拉广告」的概率</param>
/// <param name="Files"></param>
public record struct FakeAdConfiguration(
        [property: JsonPropertyName("probability")] float Probability,
        [property: JsonPropertyName("files")] string[]? Files);
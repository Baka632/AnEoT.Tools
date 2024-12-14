
using AnEoT.Tools.VolumeCreator.Models.Lofter;

namespace AnEoT.Tools.VolumeCreator.Helpers.Converters;

public sealed partial class LofterDownloadItemStateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            LofterDownloadItemState.Paused => "已暂停",
            LofterDownloadItemState.Downloading => "正在下载",
            LofterDownloadItemState.Completed => "已完成",
            LofterDownloadItemState.Error => "错误",
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

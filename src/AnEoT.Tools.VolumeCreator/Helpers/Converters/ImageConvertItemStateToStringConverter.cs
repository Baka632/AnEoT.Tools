using AnEoT.Tools.VolumeCreator.Models.ImageConvert;

namespace AnEoT.Tools.VolumeCreator.Helpers.Converters;

public sealed partial class ImageConvertItemStateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            ImageConvertItemState.None => "未开始",
            ImageConvertItemState.Converting => "正在转换",
            ImageConvertItemState.Completed => "已完成",
            ImageConvertItemState.Error => "错误",
            _ => string.Empty,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

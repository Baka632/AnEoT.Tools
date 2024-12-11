using AnEoT.Tools.VolumeCreator.Models.ImageConvert;

namespace AnEoT.Tools.VolumeCreator.Helpers.Converters;

public sealed partial class ImageFormatTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            ImageFormatType.Png => "PNG 格式",
            ImageFormatType.Jpg => "JPG 格式",
            ImageFormatType.Webp => "WebP 格式",
            _ => ""
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

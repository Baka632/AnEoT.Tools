using AnEoT.Tools.VolumeCreator.Models.ImageConvert;

namespace AnEoT.Tools.VolumeCreator.Helpers.Converters;

public sealed partial class ImageConvertSaveMethodConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            ImageConvertSaveMethod.DifferentExtension => "使用不同的文件名就地保存",
            ImageConvertSaveMethod.CreateInnerFolder => "在自动创建的文件夹里保存",
            ImageConvertSaveMethod.SelectOtherFolder => "在指定的文件夹里保存",
            _ => string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

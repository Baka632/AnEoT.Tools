
using AnEoT.Tools.VolumeCreator.Models;

namespace AnEoT.Tools.VolumeCreator.Helpers.Converters;

public sealed class MarkdownWrapperTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MarkdownWrapperType type)
        {
            return type switch
            {
                MarkdownWrapperType.Intro => "卷首语",
                MarkdownWrapperType.Article => "文章",
                MarkdownWrapperType.Interview => "专访",
                MarkdownWrapperType.Comic => "漫画",
                MarkdownWrapperType.OperatorSecret => "干员秘闻",
                MarkdownWrapperType.Paintings => "画中秘境",
                MarkdownWrapperType.Others => "其他（请设置导出文件名）",
                _ => throw new NotImplementedException()
            };
        }
        else
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

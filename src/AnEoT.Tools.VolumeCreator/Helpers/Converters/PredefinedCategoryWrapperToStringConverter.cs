using AnEoT.Tools.VolumeCreator.Models;

namespace AnEoT.Tools.VolumeCreator.Helpers.Converters;

public sealed partial class PredefinedCategoryWrapperToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
        {
            return "<未设置>";
        }
        else if (value is PredefinedCategory predefinedCategory)
        {
            return predefinedCategory.AsCategoryString();
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnEoT.Tools.WordToMarkdown.Converters;

public sealed class Int32ToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is int integer
            ? integer.ToString()
            : DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string str && int.TryParse(str, out int integer)
            ? integer
            : DependencyProperty.UnsetValue;
    }
}

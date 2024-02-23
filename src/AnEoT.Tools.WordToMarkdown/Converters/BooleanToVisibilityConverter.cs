using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnEoT.Tools.WordToMarkdown.Converters;

public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return bool.TryParse(parameter as string, out bool isReserve) && isReserve
                ? (boolean ? Visibility.Collapsed : Visibility.Visible)
                : (boolean ? Visibility.Visible : Visibility.Collapsed);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

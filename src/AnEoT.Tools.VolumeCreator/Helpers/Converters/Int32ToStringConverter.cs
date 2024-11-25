namespace AnEoT.Tools.VolumeCreator.Helpers.Converters;

public sealed partial class Int32ToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is int integer
            ? integer.ToString()
            : DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is string str && int.TryParse(str, out int integer)
            ? integer
            : -1;
    }
}

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProxyManager.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            var invert = parameter?.ToString() == "Inverse";
            var result = invert ? !boolValue : boolValue;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            var invert = parameter?.ToString() == "Inverse";
            var result = visibility == Visibility.Visible;
            return invert ? !result : result;
        }
        return false;
    }
}

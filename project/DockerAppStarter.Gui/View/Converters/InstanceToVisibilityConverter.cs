using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DockerAppStarter.Gui.View.Converters
{
    internal class InstanceToVisibilityConverter
        : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                return Visibility.Visible;
            }

            return value != null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

// ReSharper disable ConvertIfStatementToReturnStatement

#pragma warning disable IDE0046 // Convert to conditional expression

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DockerAppStarter.Gui
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

            if (value != null)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore IDE0046 // Convert to conditional expression

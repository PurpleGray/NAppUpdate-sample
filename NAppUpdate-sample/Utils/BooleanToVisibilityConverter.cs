using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NAppUpdate_sample.Utils
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>Converts a value.</summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof(Visibility))
            {
                if ((bool)value)
                    return (object)Visibility.Visible;
                if (parameter != null && parameter is Visibility)
                    return parameter;
                return (object)Visibility.Collapsed;
            }
            if (value != null)
                return (object)Visibility.Visible;
            if (parameter != null && parameter is Visibility)
                return parameter;
            return (object)Visibility.Collapsed;
        }

        /// <summary>Converts a value.</summary>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility) || !(targetType == typeof(bool)))
                throw new ArgumentException(
                    "Invalid argument/return type. Expected argument: Visibility and return type: bool");
            if ((Visibility)value == Visibility.Visible)
                return (object)true;
            return (object)false;
        }
    }
}
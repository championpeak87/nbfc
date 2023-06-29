using System;
using System.Windows.Data;

namespace NbfcClient.ValueConverters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToStringConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return bool.Parse(value.ToString());
        }
    }
}

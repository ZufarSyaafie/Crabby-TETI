using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CrabbyTETI.Converters
{
    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isError)
            {
                return isError 
                    ? new SolidColorBrush(Colors.Red) 
                    : new SolidColorBrush(Color.FromRgb(0, 150, 0));
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

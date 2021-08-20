using System;
using System.Globalization;

namespace Querty.WPF
{
    public class WidthToMinWidthConverter : BaseValueConverter<WidthToMinWidthConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value / 2) - 10;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

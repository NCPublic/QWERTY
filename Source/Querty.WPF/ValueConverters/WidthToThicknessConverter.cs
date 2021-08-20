using System;
using System.Globalization;
using System.Windows;

namespace Querty.WPF
{
    public class WidthToThicknessConverter : BaseValueConverter<WidthToThicknessConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(((double)value / 2) - 10, 0, 0, 0);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

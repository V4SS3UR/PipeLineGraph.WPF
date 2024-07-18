using System;
using System.Globalization;
using System.Windows.Data;

namespace PipeLine.Core.Converter
{
    public class EnumFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            Enum en = value as Enum;
            return en.HasFlag((Enum)parameter);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null; 
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PipeLineGraph.Core.Converter
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object enumValue, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = GetEnumValue(enumValue);

            return strValue.Equals(parameter) || strValue.Equals(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            return DependencyProperty.UnsetValue;
        }


        public string GetEnumValue(object enumValue)
        {
            Type enumType = enumValue.GetType();
            if (!enumType.IsEnum)
            {
                throw new Exception("T must be an Enumeration type.");
            }
            
            return Enum.GetName(enumType, enumValue);
        }
    }
}

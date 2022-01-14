using System;
using System.Globalization;
using System.Windows.Data;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var result = ((DateTime)value).ToString("yyyy/MM/dd HH:mm");

                return result;
            }
            catch
            {
                return new DateTime(1970, 1, 1, 0, 0, 0).ToString("yyyy/MM/dd HH:mm");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

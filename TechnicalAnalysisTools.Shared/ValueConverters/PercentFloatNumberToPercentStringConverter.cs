using System;
using System.Globalization;
using System.Windows.Data;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class PercentFloatNumberToPercentStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var result = "";

                var floatValue = Math.Round((float)value, 2);

                if (floatValue >= 0)
                {
                    result = "  " + floatValue.ToString();
                }
                else
                {
                    result = " " + floatValue.ToString();
                }

                return result;
            }
            catch
            {
                return "Error";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

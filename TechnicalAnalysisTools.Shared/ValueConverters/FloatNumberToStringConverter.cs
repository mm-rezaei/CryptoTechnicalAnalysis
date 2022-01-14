using System;
using System.Globalization;
using System.Windows.Data;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class FloatNumberToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var floatValue = (float)value;

                var result = (floatValue >= 0 ? floatValue : (-1 * floatValue)).ToString("F16");

                while (result.Contains(".") && result[result.Length - 1] == '0')
                {
                    result = result.Substring(0, result.Length - 1);

                    if (result[result.Length - 1] == '.')
                    {
                        result = result.Substring(0, result.Length - 1);
                    }
                }

                var part1 = "";
                var part2 = "";

                if (result.Contains("."))
                {
                    var parts = result.Split('.');

                    part1 = parts[0];
                    part2 = parts[1];
                }
                else
                {
                    part1 = result;
                }

                result = System.Convert.ToInt64(part1).ToString("N0");

                if (part2 != "")
                {
                    result = result + "." + part2;
                }

                if (floatValue < 0)
                {
                    result = "-" + result;
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

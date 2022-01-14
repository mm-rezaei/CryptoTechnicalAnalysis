using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class SymbolIsAscendingValueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (((bool?)value).HasValue)
                {
                    if (((bool?)value).Value)
                    {
                        return Brushes.LightGreen;
                    }
                    else
                    {
                        return Brushes.Red;
                    }
                }
                else
                {
                    return Brushes.LightGray;
                }
            }
            catch
            {
                return Brushes.LightBlue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

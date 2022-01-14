using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class TimeFrameIsAscendingValueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (((bool?)value).HasValue)
                {
                    if (((bool?)value).Value)
                    {
                        return Brushes.Green;
                    }
                    else
                    {
                        return Brushes.Red;
                    }
                }
                else
                {
                    return Brushes.Gray;
                }
            }
            catch
            {
                return Brushes.Blue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

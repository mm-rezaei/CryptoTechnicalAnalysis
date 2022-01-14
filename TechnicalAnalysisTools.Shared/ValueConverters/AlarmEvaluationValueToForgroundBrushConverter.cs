using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class AlarmEvaluationValueToForgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.Black;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

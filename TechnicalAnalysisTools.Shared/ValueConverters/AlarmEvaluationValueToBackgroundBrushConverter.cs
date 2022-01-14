using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class AlarmEvaluationValueToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.Pink;
            }
            else
            {
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.ValueConverters
{
    public class DivergenceValueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var result = Brushes.Transparent;

                var direction = (DivergenceDirectionTypes)value;

                switch (direction)
                {
                    case DivergenceDirectionTypes.Ascending: result = Brushes.Green; break;
                    case DivergenceDirectionTypes.Descending: result = Brushes.Red; break;
                    case DivergenceDirectionTypes.Unknown: result = Brushes.Gray; break;
                }

                return result;
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

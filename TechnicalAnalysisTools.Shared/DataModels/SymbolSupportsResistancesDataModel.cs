using System.ComponentModel;
using System.Windows.Media;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    public class SymbolSupportsResistancesDataModel : INotifyPropertyChanged
    {
        private string _Name;
        private string _TimeFrame;
        private float _Percent;
        private float _Price;

        public static string CurrentPriceName
        {
            get { return "LastPrice"; }
        }

        public string Name
        {
            get { return _Name; }
            set { if (_Name != value) { _Name = value; OnPropertyChanged(nameof(Name)); } }
        }

        public string TimeFrame
        {
            get { return _TimeFrame; }
            set { if (_TimeFrame != value) { _TimeFrame = value; OnPropertyChanged(nameof(TimeFrame)); } }
        }

        public float Percent
        {
            get { return _Percent; }
            set { if (_Percent != value) { _Percent = value; OnPropertyChanged(nameof(Percent)); } }
        }

        public float Price
        {
            get { return _Price; }
            set { if (_Price != value) { _Price = value; OnPropertyChanged(nameof(Price)); } }
        }

        public SolidColorBrush SupportResistanceForgroundColor
        {
            get
            {
                if (Name == CurrentPriceName)
                {
                    return Brushes.White;
                }
                else
                {
                    return Brushes.Black;
                }
            }
        }

        public SolidColorBrush SupportResistanceBackgroundColor
        {
            get
            {
                if (Name == CurrentPriceName)
                {
                    return Brushes.Green;
                }
                else
                {
                    return Brushes.White;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

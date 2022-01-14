using System;
using System.ComponentModel;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class TrailingOrderDataModel : INotifyPropertyChanged
    {
        private float _TolerantPercentForLoss;

        public float TolerantPercentForLoss
        {
            get { return _TolerantPercentForLoss; }
            set
            {
                if (_TolerantPercentForLoss != value)
                {
                    _TolerantPercentForLoss = value;

                    OnPropertyChanged(nameof(TolerantPercentForLoss));
                }
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Validation()
        {
            var result = "";

            if (TolerantPercentForLoss <= 0 || TolerantPercentForLoss > 100)
            {
                result = "The {0} trailing order validation was failed.";
            }

            return result;
        }

        public override string ToString()
        {
            var result = "";

            result += "TrailingOrder" + Environment.NewLine;
            result += "{" + Environment.NewLine;
            result += string.Format("   {0} = {1}", nameof(TolerantPercentForLoss), TolerantPercentForLoss) + Environment.NewLine;
            result += "}";

            return result;
        }
    }
}

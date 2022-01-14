using System;
using System.ComponentModel;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class GridOrderDataModel : INotifyPropertyChanged
    {
        private float _Percent;

        private byte _StepCount;

        public float Percent
        {
            get { return _Percent; }
            set
            {
                if (_Percent != value)
                {
                    _Percent = value;

                    OnPropertyChanged(nameof(Percent));
                }
            }
        }

        public byte StepCount
        {
            get { return _StepCount; }
            set
            {
                if (_StepCount != value)
                {
                    _StepCount = value;

                    OnPropertyChanged(nameof(StepCount));
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

            if (Percent <= 0 || Percent > 100 || StepCount <= 0)
            {
                result = "The {0} grid order validation was failed.";
            }

            return result;
        }

        public override string ToString()
        {
            var result = "";

            result += "GridOrder" + Environment.NewLine;
            result += "{" + Environment.NewLine;
            result += string.Format("   {0} = {1}", nameof(Percent), Percent) + Environment.NewLine;
            result += string.Format("   {0} = {1}", nameof(StepCount), StepCount) + Environment.NewLine;
            result += "}";

            return result;
        }
    }
}

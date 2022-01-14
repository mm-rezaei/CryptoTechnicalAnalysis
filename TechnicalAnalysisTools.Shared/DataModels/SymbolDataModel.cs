using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class SymbolDataModel : INotifyPropertyChanged, ICloneable
    {
        public SymbolDataModel()
        {
            _Id = Guid.NewGuid();
        }

        private Guid _Id;
        private SymbolTypes _Symbol;
        private DateTime _LastMinuteCandle;
        private CandleType _CandleType;
        private float _Open;
        private float _High;
        private float _Low;
        private float _Close;
        private float _Volume;
        private float _QuoteVolume;
        private float _NumberOfTrades;

        public Guid Id
        {
            get { return _Id; }
        }

        public bool? IsAscending
        {
            get { return Open == Close ? (bool?)null : Open < Close; }
        }

        public SymbolTypes Symbol
        {
            get { return _Symbol; }
            set { if (_Symbol != value) { _Symbol = value; OnPropertyChanged(nameof(Symbol)); } }
        }

        public DateTime LastMinuteCandle
        {
            get { return _LastMinuteCandle; }
            set { if (_LastMinuteCandle != value) { _LastMinuteCandle = value; OnPropertyChanged(nameof(LastMinuteCandle)); } }
        }

        public CandleType CandleType
        {
            get { return _CandleType; }
            set { if (_CandleType != value) { _CandleType = value; OnPropertyChanged(nameof(CandleType)); } }
        }

        public float Open
        {
            get { return _Open; }
            set { if (_Open != value) { _Open = value; OnPropertyChanged(nameof(Open)); OnPropertyChanged(nameof(IsAscending)); OnPropertyChanged(nameof(ChangedPercent)); } }
        }

        public float High
        {
            get { return _High; }
            set { if (_High != value) { _High = value; OnPropertyChanged(nameof(High)); } }
        }

        public float Low
        {
            get { return _Low; }
            set { if (_Low != value) { _Low = value; OnPropertyChanged(nameof(Low)); } }
        }

        public float Close
        {
            get { return _Close; }
            set { if (_Close != value) { _Close = value; OnPropertyChanged(nameof(Close)); OnPropertyChanged(nameof(IsAscending)); OnPropertyChanged(nameof(ChangedPercent)); } }
        }

        public float ChangedPercent
        {
            get
            {
                float result = 0f;

                if (Open < Close)
                {
                    var changed = Close - Open;

                    result = changed / Open;
                }
                else if (Close < Open)
                {
                    var changed = Open - Close;

                    result = -1 * (changed / Open);
                }

                return result * 100f;
            }
        }

        public float Volume
        {
            get { return _Volume; }
            set { if (_Volume != value) { _Volume = value; OnPropertyChanged(nameof(Volume)); } }
        }

        public float QuoteVolume
        {
            get { return _QuoteVolume; }
            set { if (_QuoteVolume != value) { _QuoteVolume = value; OnPropertyChanged(nameof(QuoteVolume)); } }
        }

        public float NumberOfTrades
        {
            get { return _NumberOfTrades; }
            set { if (_NumberOfTrades != value) { _NumberOfTrades = value; OnPropertyChanged(nameof(NumberOfTrades)); } }
        }

        public ObservableCollection<SymbolTimeFrameDataModel> SymbolTimeFrames { get; set; } = new ObservableCollection<SymbolTimeFrameDataModel>();

        public ObservableCollection<SymbolAlarmDataModel> SymbolAlarms { get; set; } = new ObservableCollection<SymbolAlarmDataModel>();

        [field: NonSerialized()]
        public ObservableCollection<SymbolSupportsResistancesDataModel> SupportsResistances { get; set; } = new ObservableCollection<SymbolSupportsResistancesDataModel>();

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            SymbolDataModel result;

            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);

                stream.Seek(0, SeekOrigin.Begin);

                result = (SymbolDataModel)formatter.Deserialize(stream);
            }

            result.SymbolTimeFrames.Clear();
            result.SymbolAlarms.Clear();
            result.SupportsResistances.Clear();

            foreach (var p in SymbolTimeFrames)
            {
                result.SymbolTimeFrames.Add((SymbolTimeFrameDataModel)p.Clone());
            }

            foreach (var p in SymbolAlarms)
            {
                result.SymbolAlarms.Add((SymbolAlarmDataModel)p.Clone());
            }

            return result;
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class SymbolTimeFrameDataModel : INotifyPropertyChanged, ICloneable
    {
        public SymbolTimeFrameDataModel()
        {
            _Id = Guid.NewGuid();
        }

        private Guid _Id;
        private TimeFrames _TimeFrame;
        private float _Open;
        private float _High;
        private float _Low;
        private float _Close;
        private float _Volume;
        private float _QuoteVolume;
        private CandleType _CandleType;
        private float _BollingerBandsBasis;
        private float _BollingerUpper;
        private float _BollingerLower;
        private float _Ema9Value;
        private float _Ema20Value;
        private float _Ema26Value;
        private float _Ema30Value;
        private float _Ema40Value;
        private float _Ema50Value;
        private float _Ema100Value;
        private float _Ema200Value;
        private float _IchimokuTenkanSen;
        private float _IchimokuKijunSen;
        private float _IchimokuSenkouSpanA;
        private float _IchimokuSenkouSpanB;
        private float _IchimokuSenkouSpanA26;
        private float _IchimokuSenkouSpanB26;
        private float _MacdValue;
        private float _MacdSignal;
        private float _MacdHistogram;
        private float _MfiValue;
        private float _RsiValue;
        private float _Sma9Value;
        private float _Sma20Value;
        private float _Sma26Value;
        private float _Sma30Value;
        private float _Sma40Value;
        private float _Sma50Value;
        private float _Sma100Value;
        private float _Sma200Value;
        private float _StochKValue;
        private float _StochDValue;
        private float _StochRsiKValue;
        private float _StochRsiDValue;
        private float _WilliamsRValue;
        private DivergenceDirectionTypes _RegularRsiDivergence;
        private DivergenceDirectionTypes _RegularStochasticKValueDivergence;
        private DivergenceDirectionTypes _RegularStochasticDValueDivergence;
        private DivergenceDirectionTypes _RegularMacdValueDivergence;
        private DivergenceDirectionTypes _RegularMacdSignalDivergence;
        private DivergenceDirectionTypes _RegularMacdHistogramDivergence;
        private DivergenceDirectionTypes _HiddenRsiDivergence;
        private DivergenceDirectionTypes _HiddenStochasticKValueDivergence;
        private DivergenceDirectionTypes _HiddenStochasticDValueDivergence;
        private DivergenceDirectionTypes _HiddenMacdValueDivergence;
        private DivergenceDirectionTypes _HiddenMacdSignalDivergence;
        private DivergenceDirectionTypes _HiddenMacdHistogramDivergence;

        public Guid Id
        {
            get { return _Id; }
        }

        public bool? IsAscending
        {
            get { return Open == Close ? (bool?)null : Open < Close; }
        }

        public TimeFrames TimeFrame
        {
            get { return _TimeFrame; }
            set { if (_TimeFrame != value) { _TimeFrame = value; OnPropertyChanged(nameof(TimeFrame)); } }
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

        public CandleType CandleType
        {
            get { return _CandleType; }
            set { if (_CandleType != value) { _CandleType = value; OnPropertyChanged(nameof(CandleType)); } }
        }

        public float BollingerBandsBasis
        {
            get { return _BollingerBandsBasis; }
            set { if (_BollingerBandsBasis != value) { _BollingerBandsBasis = value; OnPropertyChanged(nameof(BollingerBandsBasis)); } }
        }

        public float BollingerUpper
        {
            get { return _BollingerUpper; }
            set { if (_BollingerUpper != value) { _BollingerUpper = value; OnPropertyChanged(nameof(BollingerUpper)); } }
        }

        public float BollingerLower
        {
            get { return _BollingerLower; }
            set { if (_BollingerLower != value) { _BollingerLower = value; OnPropertyChanged(nameof(BollingerLower)); } }
        }

        public float Ema9Value
        {
            get { return _Ema9Value; }
            set { if (_Ema9Value != value) { _Ema9Value = value; OnPropertyChanged(nameof(Ema9Value)); } }
        }

        public float Ema20Value
        {
            get { return _Ema20Value; }
            set { if (_Ema20Value != value) { _Ema20Value = value; OnPropertyChanged(nameof(Ema20Value)); } }
        }

        public float Ema26Value
        {
            get { return _Ema26Value; }
            set { if (_Ema26Value != value) { _Ema26Value = value; OnPropertyChanged(nameof(Ema26Value)); } }
        }

        public float Ema30Value
        {
            get { return _Ema30Value; }
            set { if (_Ema30Value != value) { _Ema30Value = value; OnPropertyChanged(nameof(Ema30Value)); } }
        }

        public float Ema40Value
        {
            get { return _Ema40Value; }
            set { if (_Ema40Value != value) { _Ema40Value = value; OnPropertyChanged(nameof(Ema40Value)); } }
        }

        public float Ema50Value
        {
            get { return _Ema50Value; }
            set { if (_Ema50Value != value) { _Ema50Value = value; OnPropertyChanged(nameof(Ema50Value)); } }
        }

        public float Ema100Value
        {
            get { return _Ema100Value; }
            set { if (_Ema100Value != value) { _Ema100Value = value; OnPropertyChanged(nameof(Ema100Value)); } }
        }

        public float Ema200Value
        {
            get { return _Ema200Value; }
            set { if (_Ema200Value != value) { _Ema200Value = value; OnPropertyChanged(nameof(Ema200Value)); } }
        }

        public float IchimokuTenkanSen
        {
            get { return _IchimokuTenkanSen; }
            set { if (_IchimokuTenkanSen != value) { _IchimokuTenkanSen = value; OnPropertyChanged(nameof(IchimokuTenkanSen)); } }
        }

        public float IchimokuKijunSen
        {
            get { return _IchimokuKijunSen; }
            set { if (_IchimokuKijunSen != value) { _IchimokuKijunSen = value; OnPropertyChanged(nameof(IchimokuKijunSen)); } }
        }

        public float IchimokuSenkouSpanA
        {
            get { return _IchimokuSenkouSpanA; }
            set { if (_IchimokuSenkouSpanA != value) { _IchimokuSenkouSpanA = value; OnPropertyChanged(nameof(IchimokuSenkouSpanA)); } }
        }

        public float IchimokuSenkouSpanB
        {
            get { return _IchimokuSenkouSpanB; }
            set { if (_IchimokuSenkouSpanB != value) { _IchimokuSenkouSpanB = value; OnPropertyChanged(nameof(IchimokuSenkouSpanB)); } }
        }

        public float IchimokuSenkouSpanA26
        {
            get { return _IchimokuSenkouSpanA26; }
            set { if (_IchimokuSenkouSpanA26 != value) { _IchimokuSenkouSpanA26 = value; OnPropertyChanged(nameof(IchimokuSenkouSpanA26)); } }
        }

        public float IchimokuSenkouSpanB26
        {
            get { return _IchimokuSenkouSpanB26; }
            set { if (_IchimokuSenkouSpanB26 != value) { _IchimokuSenkouSpanB26 = value; OnPropertyChanged(nameof(IchimokuSenkouSpanB26)); } }
        }

        public float MacdValue
        {
            get { return _MacdValue; }
            set { if (_MacdValue != value) { _MacdValue = value; OnPropertyChanged(nameof(MacdValue)); } }
        }

        public float MacdSignal
        {
            get { return _MacdSignal; }
            set { if (_MacdSignal != value) { _MacdSignal = value; OnPropertyChanged(nameof(MacdSignal)); } }
        }

        public float MacdHistogram
        {
            get { return _MacdHistogram; }
            set { if (_MacdHistogram != value) { _MacdHistogram = value; OnPropertyChanged(nameof(MacdHistogram)); } }
        }

        public float MfiValue
        {
            get { return _MfiValue; }
            set { if (_MfiValue != value) { _MfiValue = value; OnPropertyChanged(nameof(MfiValue)); } }
        }

        public float RsiValue
        {
            get { return _RsiValue; }
            set { if (_RsiValue != value) { _RsiValue = value; OnPropertyChanged(nameof(RsiValue)); } }
        }

        public float Sma9Value
        {
            get { return _Sma9Value; }
            set { if (_Sma9Value != value) { _Sma9Value = value; OnPropertyChanged(nameof(Sma9Value)); } }
        }

        public float Sma20Value
        {
            get { return _Sma20Value; }
            set { if (_Sma20Value != value) { _Sma20Value = value; OnPropertyChanged(nameof(Sma20Value)); } }
        }

        public float Sma26Value
        {
            get { return _Sma26Value; }
            set { if (_Sma26Value != value) { _Sma26Value = value; OnPropertyChanged(nameof(Sma26Value)); } }
        }

        public float Sma30Value
        {
            get { return _Sma30Value; }
            set { if (_Sma30Value != value) { _Sma30Value = value; OnPropertyChanged(nameof(Sma30Value)); } }
        }

        public float Sma40Value
        {
            get { return _Sma40Value; }
            set { if (_Sma40Value != value) { _Sma40Value = value; OnPropertyChanged(nameof(Sma40Value)); } }
        }

        public float Sma50Value
        {
            get { return _Sma50Value; }
            set { if (_Sma50Value != value) { _Sma50Value = value; OnPropertyChanged(nameof(Sma50Value)); } }
        }

        public float Sma100Value
        {
            get { return _Sma100Value; }
            set { if (_Sma100Value != value) { _Sma100Value = value; OnPropertyChanged(nameof(Sma100Value)); } }
        }

        public float Sma200Value
        {
            get { return _Sma200Value; }
            set { if (_Sma200Value != value) { _Sma200Value = value; OnPropertyChanged(nameof(Sma200Value)); } }
        }

        public float StochKValue
        {
            get { return _StochKValue; }
            set { if (_StochKValue != value) { _StochKValue = value; OnPropertyChanged(nameof(StochKValue)); } }
        }

        public float StochDValue
        {
            get { return _StochDValue; }
            set { if (_StochDValue != value) { _StochDValue = value; OnPropertyChanged(nameof(StochDValue)); } }
        }

        public float StochRsiKValue
        {
            get { return _StochRsiKValue; }
            set { if (_StochRsiKValue != value) { _StochRsiKValue = value; OnPropertyChanged(nameof(StochRsiKValue)); } }
        }

        public float StochRsiDValue
        {
            get { return _StochRsiDValue; }
            set { if (_StochRsiDValue != value) { _StochRsiDValue = value; OnPropertyChanged(nameof(StochRsiDValue)); } }
        }

        public float WilliamsRValue
        {
            get { return _WilliamsRValue; }
            set { if (_WilliamsRValue != value) { _WilliamsRValue = value; OnPropertyChanged(nameof(WilliamsRValue)); } }
        }

        public DivergenceDirectionTypes RegularRsiDivergence
        {
            get { return _RegularRsiDivergence; }
            set { if (_RegularRsiDivergence != value) { _RegularRsiDivergence = value; OnPropertyChanged(nameof(RegularRsiDivergence)); } }
        }

        public DivergenceDirectionTypes RegularStochasticKValueDivergence
        {
            get { return _RegularStochasticKValueDivergence; }
            set { if (_RegularStochasticKValueDivergence != value) { _RegularStochasticKValueDivergence = value; OnPropertyChanged(nameof(RegularStochasticKValueDivergence)); } }
        }

        public DivergenceDirectionTypes RegularStochasticDValueDivergence
        {
            get { return _RegularStochasticDValueDivergence; }
            set { if (_RegularStochasticDValueDivergence != value) { _RegularStochasticDValueDivergence = value; OnPropertyChanged(nameof(RegularStochasticDValueDivergence)); } }
        }

        public DivergenceDirectionTypes RegularMacdValueDivergence
        {
            get { return _RegularMacdValueDivergence; }
            set { if (_RegularMacdValueDivergence != value) { _RegularMacdValueDivergence = value; OnPropertyChanged(nameof(RegularMacdValueDivergence)); } }
        }

        public DivergenceDirectionTypes RegularMacdSignalDivergence
        {
            get { return _RegularMacdSignalDivergence; }
            set { if (_RegularMacdSignalDivergence != value) { _RegularMacdSignalDivergence = value; OnPropertyChanged(nameof(RegularMacdSignalDivergence)); } }
        }

        public DivergenceDirectionTypes RegularMacdHistogramDivergence
        {
            get { return _RegularMacdHistogramDivergence; }
            set { if (_RegularMacdHistogramDivergence != value) { _RegularMacdHistogramDivergence = value; OnPropertyChanged(nameof(RegularMacdHistogramDivergence)); } }
        }

        public DivergenceDirectionTypes HiddenRsiDivergence
        {
            get { return _HiddenRsiDivergence; }
            set { if (_HiddenRsiDivergence != value) { _HiddenRsiDivergence = value; OnPropertyChanged(nameof(HiddenRsiDivergence)); } }
        }

        public DivergenceDirectionTypes HiddenStochasticKValueDivergence
        {
            get { return _HiddenStochasticKValueDivergence; }
            set { if (_HiddenStochasticKValueDivergence != value) { _HiddenStochasticKValueDivergence = value; OnPropertyChanged(nameof(HiddenStochasticKValueDivergence)); } }
        }

        public DivergenceDirectionTypes HiddenStochasticDValueDivergence
        {
            get { return _HiddenStochasticDValueDivergence; }
            set { if (_HiddenStochasticDValueDivergence != value) { _HiddenStochasticDValueDivergence = value; OnPropertyChanged(nameof(HiddenStochasticDValueDivergence)); } }
        }

        public DivergenceDirectionTypes HiddenMacdValueDivergence
        {
            get { return _HiddenMacdValueDivergence; }
            set { if (_HiddenMacdValueDivergence != value) { _HiddenMacdValueDivergence = value; OnPropertyChanged(nameof(HiddenMacdValueDivergence)); } }
        }

        public DivergenceDirectionTypes HiddenMacdSignalDivergence
        {
            get { return _HiddenMacdSignalDivergence; }
            set { if (_HiddenMacdSignalDivergence != value) { _HiddenMacdSignalDivergence = value; OnPropertyChanged(nameof(HiddenMacdSignalDivergence)); } }
        }

        public DivergenceDirectionTypes HiddenMacdHistogramDivergence
        {
            get { return _HiddenMacdHistogramDivergence; }
            set { if (_HiddenMacdHistogramDivergence != value) { _HiddenMacdHistogramDivergence = value; OnPropertyChanged(nameof(HiddenMacdHistogramDivergence)); } }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            SymbolTimeFrameDataModel result;

            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);

                stream.Seek(0, SeekOrigin.Begin);

                result = (SymbolTimeFrameDataModel)formatter.Deserialize(stream);
            }

            return result;
        }
    }
}

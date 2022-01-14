using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class CandleDataModel : ICloneable
    {
        public int MomentaryTimeStamp;
        public int OpenTimeStamp;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public float Volume;
        public float QuoteVolume;
        public float TakerVolume;
        public float TakerQuoteVolume;
        public float NumberOfTrades;
        public CandleType CandleType;
        public float BollingerBandsBasis;
        public float BollingerUpper;
        public float BollingerLower;
        public float Ema9Value;
        public float Ema20Value;
        public float Ema26Value;
        public float Ema30Value;
        public float Ema40Value;
        public float Ema50Value;
        public float Ema100Value;
        public float Ema200Value;
        public float IchimokuTenkanSen;
        public float IchimokuKijunSen;
        public float IchimokuSenkouSpanA;
        public float IchimokuSenkouSpanB;
        public float IchimokuChikouSpan;
        public float IchimokuSenkouSpanA26;
        public float IchimokuSenkouSpanB26;
        public float MacdValue;
        public float MacdSignal;
        public float MacdHistogram;
        public float MfiValue;
        public float RsiValue;
        public float Sma9Value;
        public float Sma20Value;
        public float Sma26Value;
        public float Sma30Value;
        public float Sma40Value;
        public float Sma50Value;
        public float Sma100Value;
        public float Sma200Value;
        public float StochKValue;
        public float StochDValue;
        public float StochRsiKValue;
        public float StochRsiDValue;
        public float WilliamsRValue;
        public byte RegularAscendingRsiDivergence;
        public byte RegularAscendingStochasticKValueDivergence;
        public byte RegularAscendingStochasticDValueDivergence;
        public byte RegularAscendingMacdValueDivergence;
        public byte RegularAscendingMacdSignalDivergence;
        public byte RegularAscendingMacdHistogramDivergence;
        public byte RegularDescendingRsiDivergence;
        public byte RegularDescendingStochasticKValueDivergence;
        public byte RegularDescendingStochasticDValueDivergence;
        public byte RegularDescendingMacdValueDivergence;
        public byte RegularDescendingMacdSignalDivergence;
        public byte RegularDescendingMacdHistogramDivergence;
        public byte HiddenAscendingRsiDivergence;
        public byte HiddenAscendingStochasticKValueDivergence;
        public byte HiddenAscendingStochasticDValueDivergence;
        public byte HiddenAscendingMacdValueDivergence;
        public byte HiddenAscendingMacdSignalDivergence;
        public byte HiddenAscendingMacdHistogramDivergence;
        public byte HiddenDescendingRsiDivergence;
        public byte HiddenDescendingStochasticKValueDivergence;
        public byte HiddenDescendingStochasticDValueDivergence;
        public byte HiddenDescendingMacdValueDivergence;
        public byte HiddenDescendingMacdSignalDivergence;
        public byte HiddenDescendingMacdHistogramDivergence;

        public DateTime MomentaryDateTime { get { return DateTimeHelper.ConvertSecondsToDateTime(MomentaryTimeStamp); } }
        public DateTime OpenDateTime { get { return DateTimeHelper.ConvertSecondsToDateTime(OpenTimeStamp); } }

        public object Clone()
        {
            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);

                stream.Seek(0, SeekOrigin.Begin);

                return formatter.Deserialize(stream);
            }
        }
    }
}

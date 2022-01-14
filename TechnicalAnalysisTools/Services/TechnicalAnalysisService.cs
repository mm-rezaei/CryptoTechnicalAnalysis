using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using StockSharp.Algo.Candles;
using StockSharp.Messages;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.IndicatorCallers;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Services
{
    public class TechnicalAnalysisService
    {
        public TechnicalAnalysisService(SymbolTypes symbol, DateTime lastInitializedCandleOpenDateTime, DateTime? saveCandlesToDatabaseDateTime, DateTime? saveMilestoneToDiskDateTime)
        {
            Symbol = symbol;

            LastInitializedCandleOpenDateTime = lastInitializedCandleOpenDateTime;

            DatabaseDateTime = saveCandlesToDatabaseDateTime;

            MilestoneDateTime = saveMilestoneToDiskDateTime;
        }

        // Base Data
        private DateTime StartDateTime { get; } = DateTime.UtcNow;

        private TimeFrames[] AllTimeFrames
        {
            get
            {
                var result = new TimeFrames[]
                {
                    TimeFrames.Minute1,
                    TimeFrames.Minute3,
                    TimeFrames.Minute5,
                    TimeFrames.Minute15,
                    TimeFrames.Minute30,
                    TimeFrames.Hour1,
                    TimeFrames.Hour2,
                    TimeFrames.Hour4,
                    TimeFrames.Hour6,
                    TimeFrames.Hour8,
                    TimeFrames.Hour12,
                    TimeFrames.Day1,
                    TimeFrames.Day3,
                    TimeFrames.Week1,
                    TimeFrames.Month1
                };

                return result;
            }
        }

        private DateTime LastInitializedCandleOpenDateTime { get; }

        // Milestones Data
        private DateTime? MilestoneDateTime { get; set; }

        private DateTime? LastMilestoneDateTime { get; set; }

        // Database Data
        private DateTime? DatabaseDateTime { get; }

        private Dictionary<TimeFrames, Tuple<string, DateTime?>> TimeFramesDatabaseInfo { get; set; }

        private Dictionary<TimeFrames, List<CandleDataModel>> CandleDataModelsToSaveToDatabase { get; set; }

        private ReaderWriterLockSlim DatabaseLock { get; } = new ReaderWriterLockSlim();

        // Indicators
        private Dictionary<TimeFrames, Dictionary<IndicatorType, IIndicatorCaller>> TimeFramePrimaryIndicators { get; set; }

        private Dictionary<TimeFrames, Dictionary<IndicatorType, IIndicatorCaller>> TimeFrameSecondaryIndicators { get; set; }

        // Candles
        private SymbolTypes Symbol { get; }

        public Dictionary<TimeFrames, IList<CandleDataModel>> TimeFrameCandles { get; set; }

        private string GetSymbolTableName(TimeFrames timeFrame)
        {
            return DatabaseHelper.GetSymbolTableName(Symbol, timeFrame);
        }

        private string GetSymbolMilestoneFilename(DateTime dateTime)
        {
            return string.Format("{0:yyyy-MM-dd-HH-mm}-{1}.mil", dateTime, Symbol);
        }

        private DateTime? GetLastMilestoneDateTime()
        {
            DateTime? result = null;

            var files = Directory.GetFiles(ServerAddressHelper.MilestoneDataFolder).Select(p => new FileInfo(p).Name).ToArray();

            var filePostfix = "-" + Symbol + ".mil";

            Func<string, DateTime> convertStringToDateTime = (text) =>
            {
                var parts = text.Replace(filePostfix, "").Split('-').Select(p => Convert.ToInt32(p)).ToArray();

                return new DateTime(parts[0], parts[1], parts[2], parts[3], parts[4], 0);
            };

            var selectedFiles = files.Where(p => p.EndsWith(filePostfix)).Select(p => convertStringToDateTime(p)).ToArray();

            if (selectedFiles.Length != 0)
            {
                result = selectedFiles.Max();
            }

            return result;
        }

        private Dictionary<IndicatorType, IIndicatorCaller> CreatePrimaryIndicators()
        {
            var result = new Dictionary<IndicatorType, IIndicatorCaller>();

            // BollingerBands
            var bollingerBands = new BollingerBandsIndicator(new SmaIndicator()) { Length = 20 };

            var bollingerBandsCaller = (IIndicatorCaller)new BollingerBandsIndicatorCaller(bollingerBands);

            result.Add(IndicatorType.BollingerBands, bollingerBandsCaller);

            // Ema
            var ema9 = new EmaIndicator() { Length = 9 };
            var ema20 = new EmaIndicator() { Length = 20 };
            var ema26 = new EmaIndicator() { Length = 26 };
            var ema30 = new EmaIndicator() { Length = 30 };
            var ema40 = new EmaIndicator() { Length = 40 };
            var ema50 = new EmaIndicator() { Length = 50 };
            var ema100 = new EmaIndicator() { Length = 100 };
            var ema200 = new EmaIndicator() { Length = 200 };

            var ema9Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema9);
            var ema20Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema20);
            var ema26Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema26);
            var ema30Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema30);
            var ema40Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema40);
            var ema50Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema50);
            var ema100Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema100);
            var ema200Caller = (IIndicatorCaller)new EmaIndicatorCaller(ema200);

            result.Add(IndicatorType.Ema9, ema9Caller);
            result.Add(IndicatorType.Ema20, ema20Caller);
            result.Add(IndicatorType.Ema26, ema26Caller);
            result.Add(IndicatorType.Ema30, ema30Caller);
            result.Add(IndicatorType.Ema40, ema40Caller);
            result.Add(IndicatorType.Ema50, ema50Caller);
            result.Add(IndicatorType.Ema100, ema100Caller);
            result.Add(IndicatorType.Ema200, ema200Caller);

            // Ichimoku
            var ichimoku = new IchimokuIndicator();

            var ichimokuCaller = (IIndicatorCaller)new IchimokuIndicatorCaller(ichimoku);

            result.Add(IndicatorType.Ichimoku, ichimokuCaller);

            // Macd
            var macd = new MacdIndicator();

            var macdCaller = (IIndicatorCaller)new MacdIndicatorCaller(macd);

            result.Add(IndicatorType.Macd, macdCaller);

            // Mfi
            var mfi = new MfiIndicator();

            var mfiCaller = (IIndicatorCaller)new MfiIndicatorCaller(mfi);

            result.Add(IndicatorType.Mfi, mfiCaller);

            // Rsi
            var rsi = new RsiIndicator() { Length = 14 };

            var rsiCaller = (IIndicatorCaller)new RsiIndicatorCaller(rsi);

            result.Add(IndicatorType.Rsi, rsiCaller);

            // Sma
            var sma9 = new SmaIndicator() { Length = 9 };
            var sma20 = new SmaIndicator() { Length = 20 };
            var sma26 = new SmaIndicator() { Length = 26 };
            var sma30 = new SmaIndicator() { Length = 30 };
            var sma40 = new SmaIndicator() { Length = 40 };
            var sma50 = new SmaIndicator() { Length = 50 };
            var sma100 = new SmaIndicator() { Length = 100 };
            var sma200 = new SmaIndicator() { Length = 200 };

            var sma9Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma9);
            var sma20Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma20);
            var sma26Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma26);
            var sma30Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma30);
            var sma40Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma40);
            var sma50Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma50);
            var sma100Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma100);
            var sma200Caller = (IIndicatorCaller)new SmaIndicatorCaller(sma200);

            result.Add(IndicatorType.Sma9, sma9Caller);
            result.Add(IndicatorType.Sma20, sma20Caller);
            result.Add(IndicatorType.Sma26, sma26Caller);
            result.Add(IndicatorType.Sma30, sma30Caller);
            result.Add(IndicatorType.Sma40, sma40Caller);
            result.Add(IndicatorType.Sma50, sma50Caller);
            result.Add(IndicatorType.Sma100, sma100Caller);
            result.Add(IndicatorType.Sma200, sma200Caller);

            // Stoch
            var stoch = new StochasticIndicator();

            var stochCaller = (IIndicatorCaller)new StochasticIndicatorCaller(stoch);

            result.Add(IndicatorType.Stoch, stochCaller);

            // StochRsi
            var stochRsi = new StochasticRsiIndicator();

            var stochRsiCaller = (IIndicatorCaller)new StochasticRsiIndicatorCaller(stochRsi);

            result.Add(IndicatorType.StochRsi, stochRsiCaller);

            // WilliamsR
            var williamsR = new WilliamsRIndicator() { Length = 14 };

            var williamsRCaller = (IIndicatorCaller)new WilliamsRIndicatorCaller(williamsR);

            result.Add(IndicatorType.WilliamsR, williamsRCaller);

            return result;
        }

        private Dictionary<IndicatorType, IIndicatorCaller> CreateSecondaryIndicators(TimeFrames timeFrame)
        {
            var result = new Dictionary<IndicatorType, IIndicatorCaller>();

            // Divergence: Rsi
            var regularAscendingRsiDivergence = new RegularAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.RsiValue; }, null, 0.0001f);
            var regularDescendingRsiDivergence = new RegularDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.RsiValue; }, null, 0.0001f);
            var hiddenAscendingRsiDivergence = new HiddenAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.RsiValue; }, null, 0.0001f);
            var hiddenDescendingRsiDivergence = new HiddenDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.RsiValue; }, null, 0.0001f);

            var regularAscendingRsiDivergenceCaller = (IIndicatorCaller)new RegularAscendingRsiDivergenceIndicatorCaller(regularAscendingRsiDivergence);
            var regularDescendingRsiDivergenceCaller = (IIndicatorCaller)new RegularDescendingRsiDivergenceIndicatorCaller(regularDescendingRsiDivergence);
            var hiddenAscendingRsiDivergenceCaller = (IIndicatorCaller)new HiddenAscendingRsiDivergenceIndicatorCaller(hiddenAscendingRsiDivergence);
            var hiddenDescendingRsiDivergenceCaller = (IIndicatorCaller)new HiddenDescendingRsiDivergenceIndicatorCaller(hiddenDescendingRsiDivergence);

            result.Add(IndicatorType.RegularAscendingRsiDivergence, regularAscendingRsiDivergenceCaller);
            result.Add(IndicatorType.RegularDescendingRsiDivergence, regularDescendingRsiDivergenceCaller);
            result.Add(IndicatorType.HiddenAscendingRsiDivergence, hiddenAscendingRsiDivergenceCaller);
            result.Add(IndicatorType.HiddenDescendingRsiDivergence, hiddenDescendingRsiDivergenceCaller);

            // Divergence: StochasticK
            var regularAscendingStochasticKValueDivergence = new RegularAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochKValue; }, null, 0.0001f);
            var regularDescendingStochasticKValueDivergence = new RegularDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochKValue; }, null, 0.0001f);
            var hiddenAscendingStochasticKValueDivergence = new HiddenAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochKValue; }, null, 0.0001f);
            var hiddenDescendingStochasticKValueDivergence = new HiddenDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochKValue; }, null, 0.0001f);

            var regularAscendingStochasticKValueDivergenceCaller = (IIndicatorCaller)new RegularAscendingStochasticKValueDivergenceIndicatorCaller(regularAscendingStochasticKValueDivergence);
            var regularDescendingStochasticKValueDivergenceCaller = (IIndicatorCaller)new RegularDescendingStochasticKValueDivergenceIndicatorCaller(regularDescendingStochasticKValueDivergence);
            var hiddenAscendingStochasticKValueDivergenceCaller = (IIndicatorCaller)new HiddenAscendingStochasticKValueDivergenceIndicatorCaller(hiddenAscendingStochasticKValueDivergence);
            var hiddenDescendingStochasticKValueDivergenceCaller = (IIndicatorCaller)new HiddenDescendingStochasticKValueDivergenceIndicatorCaller(hiddenDescendingStochasticKValueDivergence);

            result.Add(IndicatorType.RegularAscendingStochasticKValueDivergence, regularAscendingStochasticKValueDivergenceCaller);
            result.Add(IndicatorType.RegularDescendingStochasticKValueDivergence, regularDescendingStochasticKValueDivergenceCaller);
            result.Add(IndicatorType.HiddenAscendingStochasticKValueDivergence, hiddenAscendingStochasticKValueDivergenceCaller);
            result.Add(IndicatorType.HiddenDescendingStochasticKValueDivergence, hiddenDescendingStochasticKValueDivergenceCaller);

            // Divergence: StochasticD
            var regularAscendingStochasticDValueDivergence = new RegularAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochDValue; }, null, 0.0001f);
            var regularDescendingStochasticDValueDivergence = new RegularDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochDValue; }, null, 0.0001f);
            var hiddenAscendingStochasticDValueDivergence = new HiddenAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochDValue; }, null, 0.0001f);
            var hiddenDescendingStochasticDValueDivergence = new HiddenDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.StochDValue; }, null, 0.0001f);

            var regularAscendingStochasticDValueDivergenceCaller = (IIndicatorCaller)new RegularAscendingStochasticDValueDivergenceIndicatorCaller(regularAscendingStochasticDValueDivergence);
            var regularDescendingStochasticDValueDivergenceCaller = (IIndicatorCaller)new RegularDescendingStochasticDValueDivergenceIndicatorCaller(regularDescendingStochasticDValueDivergence);
            var hiddenAscendingStochasticDValueDivergenceCaller = (IIndicatorCaller)new HiddenAscendingStochasticDValueDivergenceIndicatorCaller(hiddenAscendingStochasticDValueDivergence);
            var hiddenDescendingStochasticDValueDivergenceCaller = (IIndicatorCaller)new HiddenDescendingStochasticDValueDivergenceIndicatorCaller(hiddenDescendingStochasticDValueDivergence);

            result.Add(IndicatorType.RegularAscendingStochasticDValueDivergence, regularAscendingStochasticDValueDivergenceCaller);
            result.Add(IndicatorType.RegularDescendingStochasticDValueDivergence, regularDescendingStochasticDValueDivergenceCaller);
            result.Add(IndicatorType.HiddenAscendingStochasticDValueDivergence, hiddenAscendingStochasticDValueDivergenceCaller);
            result.Add(IndicatorType.HiddenDescendingStochasticDValueDivergence, hiddenDescendingStochasticDValueDivergenceCaller);

            // Divergence: MacdValue
            var regularAscendingMacdValueDivergence = new RegularAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdValue; }, 0, 0.0001f);
            var regularDescendingMacdValueDivergence = new RegularDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdValue; }, 0, 0.0001f);
            var hiddenAscendingMacdValueDivergence = new HiddenAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdValue; }, 0, 0.0001f);
            var hiddenDescendingMacdValueDivergence = new HiddenDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdValue; }, 0, 0.0001f);

            var regularAscendingMacdValueDivergenceCaller = (IIndicatorCaller)new RegularAscendingMacdValueDivergenceIndicatorCaller(regularAscendingMacdValueDivergence);
            var regularDescendingMacdValueDivergenceCaller = (IIndicatorCaller)new RegularDescendingMacdValueDivergenceIndicatorCaller(regularDescendingMacdValueDivergence);
            var hiddenAscendingMacdValueDivergenceCaller = (IIndicatorCaller)new HiddenAscendingMacdValueDivergenceIndicatorCaller(hiddenAscendingMacdValueDivergence);
            var hiddenDescendingMacdValueDivergenceCaller = (IIndicatorCaller)new HiddenDescendingMacdValueDivergenceIndicatorCaller(hiddenDescendingMacdValueDivergence);

            result.Add(IndicatorType.RegularAscendingMacdValueDivergence, regularAscendingMacdValueDivergenceCaller);
            result.Add(IndicatorType.RegularDescendingMacdValueDivergence, regularDescendingMacdValueDivergenceCaller);
            result.Add(IndicatorType.HiddenAscendingMacdValueDivergence, hiddenAscendingMacdValueDivergenceCaller);
            result.Add(IndicatorType.HiddenDescendingMacdValueDivergence, hiddenDescendingMacdValueDivergenceCaller);

            // Divergence: MacdSignal
            var regularAscendingMacdSignalDivergence = new RegularAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdSignal; }, 0, 0.0001f);
            var regularDescendingMacdSignalDivergence = new RegularDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdSignal; }, 0, 0.0001f);
            var hiddenAscendingMacdSignalDivergence = new HiddenAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdSignal; }, 0, 0.0001f);
            var hiddenDescendingMacdSignalDivergence = new HiddenDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdSignal; }, 0, 0.0001f);

            var regularAscendingMacdSignalDivergenceCaller = (IIndicatorCaller)new RegularAscendingMacdSignalDivergenceIndicatorCaller(regularAscendingMacdSignalDivergence);
            var regularDescendingMacdSignalDivergenceCaller = (IIndicatorCaller)new RegularDescendingMacdSignalDivergenceIndicatorCaller(regularDescendingMacdSignalDivergence);
            var hiddenAscendingMacdSignalDivergenceCaller = (IIndicatorCaller)new HiddenAscendingMacdSignalDivergenceIndicatorCaller(hiddenAscendingMacdSignalDivergence);
            var hiddenDescendingMacdSignalDivergenceCaller = (IIndicatorCaller)new HiddenDescendingMacdSignalDivergenceIndicatorCaller(hiddenDescendingMacdSignalDivergence);

            result.Add(IndicatorType.RegularAscendingMacdSignalDivergence, regularAscendingMacdSignalDivergenceCaller);
            result.Add(IndicatorType.RegularDescendingMacdSignalDivergence, regularDescendingMacdSignalDivergenceCaller);
            result.Add(IndicatorType.HiddenAscendingMacdSignalDivergence, hiddenAscendingMacdSignalDivergenceCaller);
            result.Add(IndicatorType.HiddenDescendingMacdSignalDivergence, hiddenDescendingMacdSignalDivergenceCaller);

            // Divergence: MacdHistogram
            var regularAscendingMacdHistogramDivergence = new RegularAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdHistogram; }, 0, 0.0001f);
            var regularDescendingMacdHistogramDivergence = new RegularDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdHistogram; }, 0, 0.0001f);
            var hiddenAscendingMacdHistogramDivergence = new HiddenAscendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdHistogram; }, 0, 0.0001f);
            var hiddenDescendingMacdHistogramDivergence = new HiddenDescendingDivergenceIndicator(TimeFrameCandles[timeFrame], (candle) => { return candle.Close; }, (candle) => { return candle.MacdHistogram; }, 0, 0.0001f);

            var regularAscendingMacdHistogramDivergenceCaller = (IIndicatorCaller)new RegularAscendingMacdHistogramDivergenceIndicatorCaller(regularAscendingMacdHistogramDivergence);
            var regularDescendingMacdHistogramDivergenceCaller = (IIndicatorCaller)new RegularDescendingMacdHistogramDivergenceIndicatorCaller(regularDescendingMacdHistogramDivergence);
            var hiddenAscendingMacdHistogramDivergenceCaller = (IIndicatorCaller)new HiddenAscendingMacdHistogramDivergenceIndicatorCaller(hiddenAscendingMacdHistogramDivergence);
            var hiddenDescendingMacdHistogramDivergenceCaller = (IIndicatorCaller)new HiddenDescendingMacdHistogramDivergenceIndicatorCaller(hiddenDescendingMacdHistogramDivergence);

            result.Add(IndicatorType.RegularAscendingMacdHistogramDivergence, regularAscendingMacdHistogramDivergenceCaller);
            result.Add(IndicatorType.RegularDescendingMacdHistogramDivergence, regularDescendingMacdHistogramDivergenceCaller);
            result.Add(IndicatorType.HiddenAscendingMacdHistogramDivergence, hiddenAscendingMacdHistogramDivergenceCaller);
            result.Add(IndicatorType.HiddenDescendingMacdHistogramDivergence, hiddenDescendingMacdHistogramDivergenceCaller);

            return result;
        }

        private void SetCandleType(CandleDataModel candle, bool value, CandleType candleType)
        {
            if (value)
            {
                candle.CandleType = candle.CandleType | candleType;
            }
            else
            {
                candle.CandleType = candle.CandleType & (~candleType);
            }
        }

        private void SetToggleCandleTypes(CandleDataModel candle, bool? value, CandleType first, CandleType second)
        {
            if (value != null)
            {
                if (value.Value)
                {
                    candle.CandleType = candle.CandleType | first;
                    candle.CandleType = candle.CandleType & (~second);
                }
                else
                {
                    candle.CandleType = candle.CandleType & (~first);
                    candle.CandleType = candle.CandleType | second;
                }
            }
            else
            {
                candle.CandleType = candle.CandleType & (~first);
                candle.CandleType = candle.CandleType & (~second);
            }
        }

        private void LoadMilestone(DateTime dateTime)
        {
            var milestoneFilename = Path.Combine(ServerAddressHelper.MilestoneDataFolder, GetSymbolMilestoneFilename(dateTime));

            Dictionary<string, byte[]> milestone;

            using (var memoryStream = new MemoryStream(File.ReadAllBytes(milestoneFilename)))
            {
                var formatter = new BinaryFormatter();

                milestone = (Dictionary<string, byte[]>)formatter.Deserialize(memoryStream);
            }

            // 1-
            using (var memoryStream = new MemoryStream(milestone[nameof(TimeFrameCandles)]))
            {
                var formatter = new BinaryFormatter();

                var timeFrameCandles = (Dictionary<TimeFrames, IList<CandleDataModel>>)formatter.Deserialize(memoryStream);

                foreach (var timeFrame in timeFrameCandles.Keys)
                {
                    TimeFrameCandles[timeFrame].Clear();

                    ((List<CandleDataModel>)TimeFrameCandles[timeFrame]).AddRange(timeFrameCandles[timeFrame].Where(p => p != null));
                }
            }

            // 2-
            foreach (var timeFrame in TimeFramePrimaryIndicators.Keys)
            {
                foreach (var indicatorType in TimeFramePrimaryIndicators[timeFrame].Keys)
                {
                    //
                    var key = timeFrame.ToString() + indicatorType.ToString();

                    var settings = SettingsStorageHelper.FromByteArray(milestone[key]);

                    //
                    var indicatorCaller = TimeFramePrimaryIndicators[timeFrame][indicatorType];

                    indicatorCaller.LoadSetting(settings);
                }
            }

            // 3-
            foreach (var timeFrame in TimeFrameSecondaryIndicators.Keys)
            {
                foreach (var indicatorType in TimeFrameSecondaryIndicators[timeFrame].Keys)
                {
                    //
                    var key = timeFrame.ToString() + indicatorType.ToString();

                    var settings = SettingsStorageHelper.FromByteArray(milestone[key]);

                    //
                    var indicatorCaller = TimeFrameSecondaryIndicators[timeFrame][indicatorType];

                    indicatorCaller.LoadSetting(settings);
                }
            }
        }

        private void SaveMilestone(DateTime dateTime)
        {
            var milestoneFilename = Path.Combine(ServerAddressHelper.MilestoneDataFolder, GetSymbolMilestoneFilename(dateTime));

            var milestone = new Dictionary<string, byte[]>();

            // 1-
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(memoryStream, TimeFrameCandles);

                milestone[nameof(TimeFrameCandles)] = memoryStream.ToArray();
            }

            // 2-
            foreach (var timeFrame in TimeFramePrimaryIndicators.Keys)
            {
                foreach (var indicatorType in TimeFramePrimaryIndicators[timeFrame].Keys)
                {
                    //
                    var indicatorCaller = TimeFramePrimaryIndicators[timeFrame][indicatorType];

                    var setting = indicatorCaller.SaveSetting();

                    //
                    var key = timeFrame.ToString() + indicatorType.ToString();

                    milestone[key] = SettingsStorageHelper.ToByteArray(setting);
                }
            }

            // 3-
            foreach (var timeFrame in TimeFrameSecondaryIndicators.Keys)
            {
                foreach (var indicatorType in TimeFrameSecondaryIndicators[timeFrame].Keys)
                {
                    //
                    var indicatorCaller = TimeFrameSecondaryIndicators[timeFrame][indicatorType];

                    var setting = indicatorCaller.SaveSetting();

                    //
                    var key = timeFrame.ToString() + indicatorType.ToString();

                    milestone[key] = SettingsStorageHelper.ToByteArray(setting);
                }
            }

            //
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(memoryStream, milestone);

                if (File.Exists(milestoneFilename))
                {
                    File.Delete(milestoneFilename);
                }

                File.WriteAllBytes(milestoneFilename, memoryStream.ToArray());
            }
        }

        public void Start()
        {
            //
            CandleDataModelsToSaveToDatabase = new Dictionary<TimeFrames, List<CandleDataModel>>();

            foreach (var timeFrame in AllTimeFrames)
            {
                CandleDataModelsToSaveToDatabase[timeFrame] = new List<CandleDataModel>();
            }

            //
            if (DatabaseDateTime.HasValue)
            {
                var tableNames = DatabaseHelper.GetTablesList();

                TimeFramesDatabaseInfo = new Dictionary<TimeFrames, Tuple<string, DateTime?>>();

                foreach (var timeFrame in AllTimeFrames)
                {
                    var tableName = GetSymbolTableName(timeFrame);

                    if (tableNames.Any(p => p.ToLower() == tableName.ToLower()))
                    {
                        var dateTime = DatabaseHelper.GetLastDateTime(tableName);

                        if (dateTime.HasValue)
                        {
                            TimeFramesDatabaseInfo[timeFrame] = new Tuple<string, DateTime?>(tableName, dateTime.Value);
                        }
                        else
                        {
                            TimeFramesDatabaseInfo[timeFrame] = new Tuple<string, DateTime?>(tableName, null);
                        }
                    }
                    else
                    {
                        DatabaseHelper.CreateCandleDataModelTable(tableName);

                        TimeFramesDatabaseInfo[timeFrame] = new Tuple<string, DateTime?>(tableName, null);
                    }
                }
            }

            //
            TimeFrameCandles = new Dictionary<TimeFrames, IList<CandleDataModel>>();

            foreach (var timeFrame in AllTimeFrames)
            {
                TimeFrameCandles.Add(timeFrame, new List<CandleDataModel>());
            }

            //
            TimeFramePrimaryIndicators = new Dictionary<TimeFrames, Dictionary<IndicatorType, IIndicatorCaller>>();

            foreach (var timeFrame in AllTimeFrames)
            {
                TimeFramePrimaryIndicators.Add(timeFrame, CreatePrimaryIndicators());
            }

            //
            TimeFrameSecondaryIndicators = new Dictionary<TimeFrames, Dictionary<IndicatorType, IIndicatorCaller>>();

            foreach (var timeFrame in AllTimeFrames)
            {
                TimeFrameSecondaryIndicators.Add(timeFrame, CreateSecondaryIndicators(timeFrame));
            }

            //
            LastMilestoneDateTime = GetLastMilestoneDateTime();

            if (LastMilestoneDateTime.HasValue)
            {
                MilestoneDateTime = LastMilestoneDateTime.Value.AddDays(1);
            }
        }

        public void SaveDataToDataBase(CandleDataModel candle, DatabaseSavingDataMode databaseSavingDataMode)
        {
            if (DatabaseDateTime.HasValue)
            {
                var isTimeToSaveData = false;

                if (candle != null && databaseSavingDataMode != DatabaseSavingDataMode.Immediately)
                {
                    if (candle.OpenDateTime >= DatabaseDateTime)
                    {
                        if (databaseSavingDataMode == DatabaseSavingDataMode.Weekly)
                        {
                            if (candle.OpenDateTime.Hour == 23 && candle.OpenDateTime.Minute == 59 && candle.OpenDateTime.DayOfWeek == DayOfWeek.Sunday)
                            {
                                isTimeToSaveData = true;
                            }
                        }

                        if (databaseSavingDataMode == DatabaseSavingDataMode.Hourly)
                        {
                            if (candle.OpenDateTime.Minute == 59)
                            {
                                isTimeToSaveData = true;
                            }
                        }
                    }
                }
                else if (databaseSavingDataMode == DatabaseSavingDataMode.Immediately)
                {
                    isTimeToSaveData = true;
                }
                else
                {
                    throw new Exception("DatabaseSavingDataMode is not valid.");
                }

                if (isTimeToSaveData)
                {
                    DatabaseLock.EnterWriteLock();

                    try
                    {
                        var candlesCount = CandleDataModelsToSaveToDatabase.Keys.Sum(p => CandleDataModelsToSaveToDatabase[p] == null ? 0 : CandleDataModelsToSaveToDatabase[p].Count);

                        if (candlesCount > 0)
                        {
                            ServiceWorkingNotified?.Invoke(true);

                            Parallel.For(0, AllTimeFrames.Length, index =>
                            {
                                var timeFrame = AllTimeFrames[index];

                                var timeFrameDatabaseInfo = TimeFramesDatabaseInfo[timeFrame];

                                if (timeFrameDatabaseInfo.Item2.HasValue)
                                {
                                    DatabaseHelper.SaveCandleDataModelsToDatabase(timeFrameDatabaseInfo.Item1, CandleDataModelsToSaveToDatabase[timeFrame].Where(p => p.MomentaryDateTime > timeFrameDatabaseInfo.Item2).ToList());
                                }
                                else
                                {
                                    DatabaseHelper.SaveCandleDataModelsToDatabase(timeFrameDatabaseInfo.Item1, CandleDataModelsToSaveToDatabase[timeFrame]);
                                }

                                CandleDataModelsToSaveToDatabase[timeFrame].Clear();
                            });

                            ServiceWorkingNotified?.Invoke(false);
                        }
                    }
                    finally
                    {
                        DatabaseLock.ExitWriteLock();
                    }
                }
            }
        }

        public void ApplyMinuteCandle(CandleDataModel candle, DatabaseSavingDataMode databaseSavingDataMode)
        {
            //
            var processNeed = true;

            if (LastMilestoneDateTime.HasValue)
            {
                if (candle.OpenDateTime <= LastMilestoneDateTime)
                {
                    processNeed = false;
                }
            }

            if (processNeed)
            {
                Parallel.For(0, AllTimeFrames.Length, index =>
                {
                    //
                    var timeFrame = AllTimeFrames[index];

                    //
                    var timeFrameCandles = TimeFrameCandles[timeFrame];

                    var isFirstTimeFrameCandle = TimeFrameHelper.IsThisMinuteCandleFirstTimeFrameCandle(candle.OpenDateTime, timeFrame);
                    var isLastTimeFrameCandle = TimeFrameHelper.IsThisMinuteCandleLastTimeFrameCandle(candle.OpenDateTime, timeFrame);

                    var lastTimeFrameCandles = timeFrameCandles.LastOrDefault();

                    //
                    if (lastTimeFrameCandles == null || (lastTimeFrameCandles != null && isFirstTimeFrameCandle))
                    {
                        if (lastTimeFrameCandles == null || isFirstTimeFrameCandle)
                        {
                            lastTimeFrameCandles = new CandleDataModel();

                            lastTimeFrameCandles.OpenTimeStamp = candle.OpenTimeStamp;

                            timeFrameCandles.Add(lastTimeFrameCandles);
                        }

                        lastTimeFrameCandles.MomentaryTimeStamp = candle.OpenTimeStamp;
                        lastTimeFrameCandles.Open = candle.Open;
                        lastTimeFrameCandles.High = candle.High;
                        lastTimeFrameCandles.Low = candle.Low;
                        lastTimeFrameCandles.Close = candle.Close;
                        lastTimeFrameCandles.Volume = candle.Volume;
                        lastTimeFrameCandles.QuoteVolume = candle.QuoteVolume;
                        lastTimeFrameCandles.TakerVolume = candle.TakerVolume;
                        lastTimeFrameCandles.TakerQuoteVolume = candle.TakerQuoteVolume;
                        lastTimeFrameCandles.NumberOfTrades = candle.NumberOfTrades;
                        lastTimeFrameCandles.CandleType = 0;
                    }
                    else
                    {
                        lastTimeFrameCandles.MomentaryTimeStamp = candle.OpenTimeStamp;
                        lastTimeFrameCandles.High = candle.High > lastTimeFrameCandles.High ? candle.High : lastTimeFrameCandles.High;
                        lastTimeFrameCandles.Low = candle.Low < lastTimeFrameCandles.Low ? candle.Low : lastTimeFrameCandles.Low;
                        lastTimeFrameCandles.Close = candle.Close;
                        lastTimeFrameCandles.Volume += candle.Volume;
                        lastTimeFrameCandles.QuoteVolume += candle.QuoteVolume;
                        lastTimeFrameCandles.TakerVolume += candle.TakerVolume;
                        lastTimeFrameCandles.TakerQuoteVolume += candle.TakerQuoteVolume;
                        lastTimeFrameCandles.NumberOfTrades += candle.NumberOfTrades;
                        lastTimeFrameCandles.CandleType = 0;
                    }

                    //
                    var temporaryTimeFrameCandle = new TimeFrameCandle();
                    temporaryTimeFrameCandle.OpenPrice = Convert.ToDecimal(lastTimeFrameCandles.Open);
                    temporaryTimeFrameCandle.HighPrice = Convert.ToDecimal(lastTimeFrameCandles.High);
                    temporaryTimeFrameCandle.LowPrice = Convert.ToDecimal(lastTimeFrameCandles.Low);
                    temporaryTimeFrameCandle.ClosePrice = Convert.ToDecimal(lastTimeFrameCandles.Close);
                    temporaryTimeFrameCandle.TotalVolume = Convert.ToDecimal(lastTimeFrameCandles.Volume);
                    temporaryTimeFrameCandle.State = isLastTimeFrameCandle ? CandleStates.Finished : CandleStates.Active;

                    // Primary Indicators
                    foreach (var indicatorType in TimeFramePrimaryIndicators[timeFrame].Keys)
                    {
                        var indicatorCaller = TimeFramePrimaryIndicators[timeFrame][indicatorType];

                        if (!indicatorCaller.ProcessCandle(temporaryTimeFrameCandle, lastTimeFrameCandles))
                        {
                            //throw new Exception();
                        }
                    }

                    // Secondary Indicators
                    foreach (var indicatorType in TimeFrameSecondaryIndicators[timeFrame].Keys)
                    {
                        var indicatorCaller = TimeFrameSecondaryIndicators[timeFrame][indicatorType];

                        var indicatorShouldProcess = false;

                        if (DatabaseDateTime.HasValue)
                        {
                            if (candle.OpenDateTime >= DatabaseDateTime)
                            {
                                var timeFrameDatabaseInfo = TimeFramesDatabaseInfo[timeFrame];

                                if (timeFrameDatabaseInfo.Item2.HasValue)
                                {
                                    if (candle.OpenDateTime > timeFrameDatabaseInfo.Item2)
                                    {
                                        indicatorShouldProcess = true;
                                    }
                                }
                                else
                                {
                                    indicatorShouldProcess = true;
                                }
                            }
                        }

                        if (!indicatorShouldProcess)
                        {
                            if (candle.OpenDateTime >= StartDateTime || candle.OpenDateTime == LastInitializedCandleOpenDateTime)
                            {
                                indicatorShouldProcess = true;
                            }
                        }

                        if (indicatorShouldProcess)
                        {
                            if (!indicatorCaller.ProcessCandle(temporaryTimeFrameCandle, lastTimeFrameCandles))
                            {
                                //throw new Exception();
                            }
                        }
                    }

                    //
                    var isBullishOrBearish = temporaryTimeFrameCandle.IsBullishOrBearish();
                    var isDragonflyOrGravestone = temporaryTimeFrameCandle.IsDragonflyOrGravestone();
                    var isHammer = temporaryTimeFrameCandle.IsHammer();
                    var isMarubozu = temporaryTimeFrameCandle.IsMarubozu();
                    var isSpinningTop = temporaryTimeFrameCandle.IsSpinningTop();

                    SetToggleCandleTypes(lastTimeFrameCandles, isBullishOrBearish, CandleType.Bullish, CandleType.Bearish);
                    SetToggleCandleTypes(lastTimeFrameCandles, isDragonflyOrGravestone, CandleType.Dragonfly, CandleType.Gravestone);
                    SetCandleType(lastTimeFrameCandles, isHammer, CandleType.Hammer);
                    SetCandleType(lastTimeFrameCandles, isMarubozu, CandleType.Marubozu);
                    SetCandleType(lastTimeFrameCandles, isSpinningTop, CandleType.SpinningTop);

                    // Save to Database Cache
                    if (DatabaseDateTime.HasValue)
                    {
                        if (candle.OpenDateTime >= DatabaseDateTime)
                        {
                            CandleDataModelsToSaveToDatabase[timeFrame].Add((CandleDataModel)lastTimeFrameCandles.Clone());
                        }
                    }

                    // Delete Unnessary Candles
                    if (timeFrameCandles.Count > ServerConstantHelper.MaximumCandlesInMemory)
                    {
                        var selectedIndexToDelete = timeFrameCandles.Count - ServerConstantHelper.MaximumCandlesInMemory - 1;

                        timeFrameCandles[selectedIndexToDelete] = null;

                        timeFrameCandles.RemoveAt(selectedIndexToDelete);
                    }
                });
            }

            //
            if (LastMilestoneDateTime.HasValue)
            {
                if (candle.OpenDateTime == LastMilestoneDateTime)
                {
                    LoadMilestone(candle.OpenDateTime);
                }
            }

            SaveDataToDataBase(candle, databaseSavingDataMode);

            if (MilestoneDateTime.HasValue)
            {
                if (candle.OpenDateTime == MilestoneDateTime)
                {
                    ServiceWorkingNotified?.Invoke(true);

                    if (candle.OpenDateTime >= StartDateTime)
                    {
                        Thread.Sleep(5000);
                    }

                    if (LastMilestoneDateTime.HasValue)
                    {
                        if (MilestoneDateTime != LastMilestoneDateTime)
                        {
                            SaveMilestone(candle.OpenDateTime);

                            LastMilestoneDateTime = candle.OpenDateTime;

                            MilestoneDateTime = MilestoneDateTime.Value.AddDays(1);
                        }
                    }
                    else
                    {
                        SaveMilestone(candle.OpenDateTime);

                        LastMilestoneDateTime = candle.OpenDateTime;

                        MilestoneDateTime = MilestoneDateTime.Value.AddDays(1);
                    }

                    ServiceWorkingNotified?.Invoke(false);
                }
            }
        }

        public event ServiceWorkingNotifiedHandler ServiceWorkingNotified;
    }
}

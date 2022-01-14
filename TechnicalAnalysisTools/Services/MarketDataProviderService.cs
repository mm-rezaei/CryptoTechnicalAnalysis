using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Binance.Net.Objects.Spot.MarketData;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Services
{
    public class MarketDataProviderService
    {
        public IList<SymbolTypes> SupportedSymbols { get; } = new List<SymbolTypes>();

        private Dictionary<SymbolTypes, ConcurrentQueue<CandleDataModel>> SymbolsCandles { get; } = new Dictionary<SymbolTypes, ConcurrentQueue<CandleDataModel>>();

        private Dictionary<SymbolTypes, BinanceSpotKline> LastSymbolsCandles { get; } = new Dictionary<SymbolTypes, BinanceSpotKline>();

        private BinanceService Binance { get; } = new BinanceService();

        private ReaderWriterLockSlim SyncLock { get; } = new ReaderWriterLockSlim();

        private string GetSymbolFileFullPath(SymbolTypes symbol)
        {
            return Path.Combine(ServerAddressHelper.HistoricalDataFolder, symbol.ToString() + ".csv");
        }

        private void Binance_BinanceKlineUpdatesReceived(SymbolTypes symbol, BinanceSpotKline kline)
        {
            SyncLock.EnterReadLock();

            try
            {
                lock (GetSymbolsCandlesForSpesificSymbol(symbol))
                {
                    var lastSymbolsCandle = LastSymbolsCandles[symbol];

                    if (lastSymbolsCandle.OpenTime.AddMinutes(1) == kline.OpenTime && kline.OpenTime > lastSymbolsCandle.CloseTime && kline.OpenTime.Second == 0 && kline.CloseTime.Second == 59)
                    {
                        //
                        var candle = new CandleDataModel();

                        candle.MomentaryTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(kline.OpenTime);
                        candle.OpenTimeStamp = candle.MomentaryTimeStamp;
                        candle.Open = Convert.ToSingle(kline.Open);
                        candle.High = Convert.ToSingle(kline.High);
                        candle.Low = Convert.ToSingle(kline.Low);
                        candle.Close = Convert.ToSingle(kline.Close);
                        candle.Volume = Convert.ToSingle(kline.BaseVolume);
                        candle.QuoteVolume = Convert.ToSingle(kline.QuoteVolume);
                        candle.NumberOfTrades = Convert.ToSingle(kline.TradeCount);
                        candle.TakerVolume = Convert.ToSingle(kline.TakerBuyBaseVolume);
                        candle.TakerQuoteVolume = Convert.ToSingle(kline.TakerBuyQuoteVolume);

                        GetSymbolsCandlesForSpesificSymbol(symbol).Enqueue(candle);

                        LastSymbolsCandles[symbol] = kline;

                        //
                        var appendedTextToFile = string.Format("{0:G29},{1:G29},{2:G29},{3:G29},{4:G29},{5:G29},{6:G29},{7:G29},{8:G29},{9:G29}", kline.Open, kline.High, kline.Low, kline.Close, kline.BaseVolume, kline.QuoteVolume, kline.TradeCount, kline.TakerBuyBaseVolume, kline.TakerBuyQuoteVolume, DateTimeHelper.ConvertDateTimeToSeconds(kline.OpenTime) * 1000000M) + Environment.NewLine;

                        var filename = GetSymbolFileFullPath(symbol);

                        File.AppendAllText(filename, appendedTextToFile);
                    }
                    else
                    {
                        var expectedDateTime = DateTime.UtcNow;

                        if (expectedDateTime.Minute == 0 && expectedDateTime.Second == 0)
                        {
                            expectedDateTime = expectedDateTime.AddMinutes(-1);
                        }
                        else
                        {
                            expectedDateTime = expectedDateTime.AddMinutes(-1);

                            expectedDateTime = new DateTime(expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day, expectedDateTime.Hour, expectedDateTime.Minute, 0);
                        }

                        ReadMinuteUpdates(symbol, ref lastSymbolsCandle, expectedDateTime);

                        LastSymbolsCandles[symbol] = lastSymbolsCandle;
                    }
                }
            }
            finally
            {
                SyncLock.ExitReadLock();
            }
        }

        private void Binance_BinanceConnectionStatus(BinanceConnectionStatusModes status)
        {
            BinanceConnectionStatus?.Invoke(status);
        }

        private void ReadMinuteUpdates(SymbolTypes symbol, ref BinanceSpotKline lastBinanceSpotKline, DateTime? lastExpectedCandleTime = null)
        {
            var OriginLastBinanceSpotKline = lastBinanceSpotKline;

            var klines = Binance.ReadMinuteUpdates(symbol, ref lastBinanceSpotKline);

            if (lastExpectedCandleTime == null)
            {
                if (klines.Count > 1)
                {
                    klines = klines.Take(klines.Count - 1).ToList();

                    lastBinanceSpotKline = klines.Last();
                }
                else if (klines.Count == 1)
                {
                    klines.Clear();

                    lastBinanceSpotKline = OriginLastBinanceSpotKline;
                }
            }
            else
            {
                if (klines.Count > 0)
                {
                    var openTime = klines.Last().OpenTime;

                    openTime = new DateTime(openTime.Year, openTime.Month, openTime.Day, openTime.Hour, openTime.Minute, openTime.Second);

                    if (openTime > lastExpectedCandleTime.Value)
                    {
                        if (klines.Count > 1)
                        {
                            klines = klines.Take(klines.Count - 1).ToList();

                            lastBinanceSpotKline = klines.Last();
                        }
                        else if (klines.Count == 1)
                        {
                            klines.Clear();

                            lastBinanceSpotKline = OriginLastBinanceSpotKline;
                        }
                    }
                }
            }

            var queue = GetSymbolsCandlesForSpesificSymbol(symbol);

            if (klines != null && klines.Count != 0)
            {
                var appendedTextToFile = new StringBuilder();

                foreach (var kline in klines)
                {
                    //
                    var candle = new CandleDataModel();

                    candle.MomentaryTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(kline.OpenTime);
                    candle.OpenTimeStamp = candle.MomentaryTimeStamp;
                    candle.Open = Convert.ToSingle(kline.Open);
                    candle.High = Convert.ToSingle(kline.High);
                    candle.Low = Convert.ToSingle(kline.Low);
                    candle.Close = Convert.ToSingle(kline.Close);
                    candle.Volume = Convert.ToSingle(kline.BaseVolume);
                    candle.QuoteVolume = Convert.ToSingle(kline.QuoteVolume);
                    candle.NumberOfTrades = Convert.ToSingle(kline.TradeCount);
                    candle.TakerVolume = Convert.ToSingle(kline.TakerBuyBaseVolume);
                    candle.TakerQuoteVolume = Convert.ToSingle(kline.TakerBuyQuoteVolume);

                    queue.Enqueue(candle);

                    //
                    appendedTextToFile.Append(string.Format("{0:G29},{1:G29},{2:G29},{3:G29},{4:G29},{5:G29},{6:G29},{7:G29},{8:G29},{9:G29}", kline.Open, kline.High, kline.Low, kline.Close, kline.BaseVolume, kline.QuoteVolume, kline.TradeCount, kline.TakerBuyBaseVolume, kline.TakerBuyQuoteVolume, DateTimeHelper.ConvertDateTimeToSeconds(kline.OpenTime) * 1000000M) + Environment.NewLine);
                }

                var filename = GetSymbolFileFullPath(symbol);

                File.AppendAllText(filename, appendedTextToFile.ToString());
            }
        }

        private ConcurrentQueue<CandleDataModel> GetSymbolsCandlesForSpesificSymbol(SymbolTypes symbol)
        {
            ConcurrentQueue<CandleDataModel> result;

            if (SymbolsCandles.ContainsKey(symbol))
            {
                result = SymbolsCandles[symbol];
            }
            else
            {
                //
                var queue = new ConcurrentQueue<CandleDataModel>();

                SymbolsCandles.Add(symbol, queue);

                var minuteCandles = HistoricalDataHelper.ExcelToMinute1Candles(GetSymbolFileFullPath(symbol));

                for (var index = 0; index < minuteCandles.Count; index++)
                {
                    queue.Enqueue(minuteCandles[index]);
                }

                //
                var lastDataModel = minuteCandles[minuteCandles.Count - 1];

                var lastSymbolsCandle = new BinanceSpotKline()
                {
                    OpenTime = lastDataModel.OpenDateTime,
                    CloseTime = lastDataModel.OpenDateTime.AddSeconds(59),
                    Open = Convert.ToDecimal(lastDataModel.Open),
                    High = Convert.ToDecimal(lastDataModel.High),
                    Low = Convert.ToDecimal(lastDataModel.Low),
                    Close = Convert.ToDecimal(lastDataModel.Close),
                    BaseVolume = Convert.ToDecimal(lastDataModel.Volume),
                    QuoteVolume = Convert.ToDecimal(lastDataModel.QuoteVolume),
                    TradeCount = Convert.ToInt32(lastDataModel.NumberOfTrades),
                    TakerBuyBaseVolume = Convert.ToDecimal(lastDataModel.TakerVolume),
                    TakerBuyQuoteVolume = Convert.ToDecimal(lastDataModel.TakerQuoteVolume)
                };

                ReadMinuteUpdates(symbol, ref lastSymbolsCandle);

                LastSymbolsCandles[symbol] = lastSymbolsCandle;

                //
                Binance.SubscribeToMinuteUpdates(symbol);

                //
                result = SymbolsCandles[symbol];
            }

            return result;
        }

        public void Start()
        {
            Binance.BinanceKlineUpdatesReceived += Binance_BinanceKlineUpdatesReceived;
            Binance.BinanceConnectionStatus += Binance_BinanceConnectionStatus;

            foreach (var symbol in SymbolTypeHelper.SymbolTypesList)
            {
                if (File.Exists(GetSymbolFileFullPath(symbol)))
                {
                    SupportedSymbols.Add(symbol);
                }
            }
        }

        public void Stop()
        {
            Binance.Stop();
        }

        public DateTime GetFirstSupportedDateTime(SymbolTypes symbol)
        {
            if (SupportedSymbols.Contains(symbol))
            {
                if (GetSymbolsCandlesForSpesificSymbol(symbol).Count > 0)
                {
                    CandleDataModel firstCandle;

                    while (!GetSymbolsCandlesForSpesificSymbol(symbol).TryPeek(out firstCandle))
                    {
                        Thread.Sleep(0);
                    }

                    return firstCandle.OpenDateTime;
                }
            }

            return DateTime.UtcNow;
        }

        public CandleDataModel GetNextMinuteCandle(SymbolTypes symbol)
        {
            CandleDataModel result = null;

            if (SupportedSymbols.Contains(symbol))
            {
                if (GetSymbolsCandlesForSpesificSymbol(symbol).Count > 0)
                {
                    while (!GetSymbolsCandlesForSpesificSymbol(symbol).TryDequeue(out result))
                    {
                        Thread.Sleep(0);
                    }
                }
            }

            return result;
        }

        public int GetAvailableSpotMinuteCandleCount(SymbolTypes symbol)
        {
            if (SupportedSymbols.Contains(symbol))
            {
                return HistoricalDataHelper.CountMinute1Candles(GetSymbolFileFullPath(symbol));
            }

            return 0;
        }

        public CandleDataModel GetLastSpotMinuteCandle(SymbolTypes symbol)
        {
            return GetSymbolsCandlesForSpesificSymbol(symbol).Last();
        }

        public void SyncSymbolDataModels()
        {
            SyncLock.EnterWriteLock();

            try
            {
                foreach (var symbol in SupportedSymbols)
                {
                    var lastSymbolsCandle = LastSymbolsCandles[symbol];

                    var expectedDateTime = DateTime.UtcNow;

                    if (expectedDateTime.Minute == 0 && expectedDateTime.Second == 0)
                    {
                        expectedDateTime = expectedDateTime.AddMinutes(-1);
                    }
                    else
                    {
                        expectedDateTime = expectedDateTime.AddMinutes(-1);

                        expectedDateTime = new DateTime(expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day, expectedDateTime.Hour, expectedDateTime.Minute, 0);
                    }

                    ReadMinuteUpdates(symbol, ref lastSymbolsCandle, expectedDateTime);

                    LastSymbolsCandles[symbol] = lastSymbolsCandle;
                }
            }
            finally
            {
                SyncLock.ExitWriteLock();
            }
        }

        public event BinanceConnectionStatusHandler BinanceConnectionStatus;
    }
}

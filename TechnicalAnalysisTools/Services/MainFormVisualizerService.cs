using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Shared.Conditions;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Services
{
    public class MainFormVisualizerService
    {
        private static bool WasCompiledAlarmLoaded { get; set; } = false;

        private Thread UpdaterThread { get; set; }

        private MarketDataProviderService Market { get; set; }

        private Dictionary<ICondition, DateTime> LastDateTimeAlarmsCalculated { get; } = new Dictionary<ICondition, DateTime>();

        private Dictionary<SymbolTypes, TechnicalAnalysisService> TechnicalAnalysis { get; } = new Dictionary<SymbolTypes, TechnicalAnalysisService>();

        public ObservableCollection<SymbolDataModel> MarketData { get; } = new ObservableCollection<SymbolDataModel>();

        public List<Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[], string>> Alarms { get; } = new List<Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[], string>>();

        public IList<SymbolTypes> SupportedSymbols
        {
            get
            {
                return Market.SupportedSymbols;
            }
        }

        private void UpdateMarket(bool forceUpdate = false)
        {
            //
            var currentDateTime = DateTime.UtcNow;

            bool allSymbolsSync;

            DateTime? symbolsSyncDateTime;

            int updatedCandlesCount = 0;

            //
            foreach (var symbol in Market.SupportedSymbols)
            {
                var technicalAnalysis = TechnicalAnalysis[symbol];

                var lastCandleDataModel = Market.GetNextMinuteCandle(symbol);

                while (lastCandleDataModel != null)
                {
                    updatedCandlesCount++;

                    technicalAnalysis.ApplyMinuteCandle(lastCandleDataModel, DatabaseSavingDataMode.Hourly);

                    lastCandleDataModel = Market.GetNextMinuteCandle(symbol);
                }
            }

            //
            allSymbolsSync = true;

            symbolsSyncDateTime = null;

            foreach (var symbol in Market.SupportedSymbols)
            {
                var dataModel = ((List<CandleDataModel>)TechnicalAnalysis[symbol].TimeFrameCandles[TimeFrames.Minute1]).Last();

                if (allSymbolsSync)
                {
                    if (!symbolsSyncDateTime.HasValue)
                    {
                        symbolsSyncDateTime = dataModel.OpenDateTime;
                    }

                    if (symbolsSyncDateTime.Value != dataModel.OpenDateTime)
                    {
                        allSymbolsSync = false;
                    }
                }
            }

            if (!allSymbolsSync)
            {
                Market.SyncSymbolDataModels();

                foreach (var symbol in Market.SupportedSymbols)
                {
                    var technicalAnalysis = TechnicalAnalysis[symbol];

                    var lastCandleDataModel = Market.GetNextMinuteCandle(symbol);

                    while (lastCandleDataModel != null)
                    {
                        updatedCandlesCount++;

                        technicalAnalysis.ApplyMinuteCandle(lastCandleDataModel, DatabaseSavingDataMode.Hourly);

                        lastCandleDataModel = Market.GetNextMinuteCandle(symbol);
                    }
                }
            }

            //
            allSymbolsSync = true;

            symbolsSyncDateTime = null;

            foreach (var symbol in Market.SupportedSymbols)
            {
                var dataModel = ((List<CandleDataModel>)TechnicalAnalysis[symbol].TimeFrameCandles[TimeFrames.Minute1]).Last();

                if (allSymbolsSync)
                {
                    if (!symbolsSyncDateTime.HasValue)
                    {
                        symbolsSyncDateTime = dataModel.OpenDateTime;
                    }

                    if (symbolsSyncDateTime.Value != dataModel.OpenDateTime)
                    {
                        allSymbolsSync = false;
                    }
                }
            }

            //
            if (forceUpdate || updatedCandlesCount > 0)
            {
                //
                lock (MarketData)
                {
                    foreach (var symbol in Market.SupportedSymbols)
                    {
                        var symbolDataModel = MarketData.First(p => p.Symbol == symbol);

                        var technicalAnalysis = TechnicalAnalysis[symbol];

                        foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
                        {
                            //
                            var dataModel = ((List<CandleDataModel>)technicalAnalysis.TimeFrameCandles[timeFrame]).Last();
                            var symbolTimeFrameDataModel = symbolDataModel.SymbolTimeFrames.First(p => p.TimeFrame == timeFrame);

                            symbolTimeFrameDataModel.Open = dataModel.Open;
                            symbolTimeFrameDataModel.Close = dataModel.Close;
                            symbolTimeFrameDataModel.High = dataModel.High;
                            symbolTimeFrameDataModel.Low = dataModel.Low;
                            symbolTimeFrameDataModel.Volume = dataModel.Volume;
                            symbolTimeFrameDataModel.QuoteVolume = dataModel.QuoteVolume;
                            symbolTimeFrameDataModel.CandleType = dataModel.CandleType;
                            symbolTimeFrameDataModel.BollingerBandsBasis = dataModel.BollingerBandsBasis;
                            symbolTimeFrameDataModel.BollingerUpper = dataModel.BollingerUpper;
                            symbolTimeFrameDataModel.BollingerLower = dataModel.BollingerLower;
                            symbolTimeFrameDataModel.Ema9Value = dataModel.Ema9Value;
                            symbolTimeFrameDataModel.Ema20Value = dataModel.Ema20Value;
                            symbolTimeFrameDataModel.Ema26Value = dataModel.Ema26Value;
                            symbolTimeFrameDataModel.Ema30Value = dataModel.Ema30Value;
                            symbolTimeFrameDataModel.Ema40Value = dataModel.Ema40Value;
                            symbolTimeFrameDataModel.Ema50Value = dataModel.Ema50Value;
                            symbolTimeFrameDataModel.Ema100Value = dataModel.Ema100Value;
                            symbolTimeFrameDataModel.Ema200Value = dataModel.Ema200Value;
                            symbolTimeFrameDataModel.IchimokuTenkanSen = dataModel.IchimokuTenkanSen;
                            symbolTimeFrameDataModel.IchimokuKijunSen = dataModel.IchimokuKijunSen;
                            symbolTimeFrameDataModel.IchimokuSenkouSpanA = dataModel.IchimokuSenkouSpanA;
                            symbolTimeFrameDataModel.IchimokuSenkouSpanB = dataModel.IchimokuSenkouSpanB;
                            symbolTimeFrameDataModel.IchimokuSenkouSpanA26 = dataModel.IchimokuSenkouSpanA26;
                            symbolTimeFrameDataModel.IchimokuSenkouSpanB26 = dataModel.IchimokuSenkouSpanB26;
                            symbolTimeFrameDataModel.MacdValue = dataModel.MacdValue;
                            symbolTimeFrameDataModel.MacdSignal = dataModel.MacdSignal;
                            symbolTimeFrameDataModel.MacdHistogram = dataModel.MacdHistogram;
                            symbolTimeFrameDataModel.MfiValue = dataModel.MfiValue;
                            symbolTimeFrameDataModel.RsiValue = dataModel.RsiValue;
                            symbolTimeFrameDataModel.Sma9Value = dataModel.Sma9Value;
                            symbolTimeFrameDataModel.Sma20Value = dataModel.Sma20Value;
                            symbolTimeFrameDataModel.Sma26Value = dataModel.Sma26Value;
                            symbolTimeFrameDataModel.Sma30Value = dataModel.Sma30Value;
                            symbolTimeFrameDataModel.Sma40Value = dataModel.Sma40Value;
                            symbolTimeFrameDataModel.Sma50Value = dataModel.Sma50Value;
                            symbolTimeFrameDataModel.Sma100Value = dataModel.Sma100Value;
                            symbolTimeFrameDataModel.Sma200Value = dataModel.Sma200Value;
                            symbolTimeFrameDataModel.StochKValue = dataModel.StochKValue;
                            symbolTimeFrameDataModel.StochDValue = dataModel.StochDValue;
                            symbolTimeFrameDataModel.StochRsiKValue = dataModel.StochRsiKValue;
                            symbolTimeFrameDataModel.StochRsiDValue = dataModel.StochRsiDValue;
                            symbolTimeFrameDataModel.WilliamsRValue = dataModel.WilliamsRValue;

                            Func<byte, byte, DivergenceDirectionTypes> divergenceCalculate = (ascending, descending) =>
                              {
                                  var result = DivergenceDirectionTypes.None;

                                  if (ascending > 0 && descending > 0)
                                  {
                                      result = DivergenceDirectionTypes.Unknown;
                                  }
                                  else if (ascending > 0)
                                  {
                                      result = DivergenceDirectionTypes.Ascending;
                                  }
                                  else if (descending > 0)
                                  {
                                      result = DivergenceDirectionTypes.Descending;
                                  }

                                  return result;
                              };

                            symbolTimeFrameDataModel.RegularRsiDivergence = divergenceCalculate(dataModel.RegularAscendingRsiDivergence, dataModel.RegularDescendingRsiDivergence);
                            symbolTimeFrameDataModel.RegularStochasticKValueDivergence = divergenceCalculate(dataModel.RegularAscendingStochasticKValueDivergence, dataModel.RegularDescendingStochasticKValueDivergence);
                            symbolTimeFrameDataModel.RegularStochasticDValueDivergence = divergenceCalculate(dataModel.RegularAscendingStochasticDValueDivergence, dataModel.RegularDescendingStochasticDValueDivergence);
                            symbolTimeFrameDataModel.RegularMacdValueDivergence = divergenceCalculate(dataModel.RegularAscendingMacdValueDivergence, dataModel.RegularDescendingMacdValueDivergence);
                            symbolTimeFrameDataModel.RegularMacdSignalDivergence = divergenceCalculate(dataModel.RegularAscendingMacdSignalDivergence, dataModel.RegularDescendingMacdSignalDivergence);
                            symbolTimeFrameDataModel.RegularMacdHistogramDivergence = divergenceCalculate(dataModel.RegularAscendingMacdHistogramDivergence, dataModel.RegularDescendingMacdHistogramDivergence);

                            symbolTimeFrameDataModel.HiddenRsiDivergence = divergenceCalculate(dataModel.HiddenAscendingRsiDivergence, dataModel.HiddenDescendingRsiDivergence);
                            symbolTimeFrameDataModel.HiddenStochasticKValueDivergence = divergenceCalculate(dataModel.HiddenAscendingStochasticKValueDivergence, dataModel.HiddenDescendingStochasticKValueDivergence);
                            symbolTimeFrameDataModel.HiddenStochasticDValueDivergence = divergenceCalculate(dataModel.HiddenAscendingStochasticDValueDivergence, dataModel.HiddenDescendingStochasticDValueDivergence);
                            symbolTimeFrameDataModel.HiddenMacdValueDivergence = divergenceCalculate(dataModel.HiddenAscendingMacdValueDivergence, dataModel.HiddenDescendingMacdValueDivergence);
                            symbolTimeFrameDataModel.HiddenMacdSignalDivergence = divergenceCalculate(dataModel.HiddenAscendingMacdSignalDivergence, dataModel.HiddenDescendingMacdSignalDivergence);
                            symbolTimeFrameDataModel.HiddenMacdHistogramDivergence = divergenceCalculate(dataModel.HiddenAscendingMacdHistogramDivergence, dataModel.HiddenDescendingMacdHistogramDivergence);

                            //
                            if (timeFrame == TimeFrames.Minute1)
                            {
                                symbolDataModel.LastMinuteCandle = dataModel.OpenDateTime;
                            }
                            else if (timeFrame == TimeFrames.Day1)
                            {
                                symbolDataModel.Open = dataModel.Open;
                                symbolDataModel.High = dataModel.High;
                                symbolDataModel.Low = dataModel.Low;
                                symbolDataModel.Close = dataModel.Close;
                                symbolDataModel.Volume = dataModel.Volume;
                                symbolDataModel.QuoteVolume = dataModel.QuoteVolume;
                                symbolDataModel.NumberOfTrades = dataModel.NumberOfTrades;
                                symbolDataModel.CandleType = dataModel.CandleType;
                            }
                        }
                    }
                }

                //
                var enabledAlarms = new List<SymbolAlarmDataModel>();
                var alarmsHistory = new List<SymbolAlarmDataModel>();

                foreach (var alarm in Alarms)
                {
                    if (alarm.Item1.Enabled)
                    {
                        try
                        {
                            //
                            var allSymbolsOfAlarmSyncd = true;

                            var allSymbolsOfAlarm = alarm.Item4;

                            DateTime? syncdDateTime = null;

                            foreach (var symbol in allSymbolsOfAlarm)
                            {
                                var symbolDataModel = MarketData.FirstOrDefault(p => p.Symbol == symbol);

                                if (symbolDataModel == null)
                                {
                                    allSymbolsOfAlarmSyncd = false;

                                    break;
                                }
                                else
                                {
                                    if (!syncdDateTime.HasValue)
                                    {
                                        syncdDateTime = symbolDataModel.LastMinuteCandle;
                                    }
                                    else
                                    {
                                        if (syncdDateTime.Value != symbolDataModel.LastMinuteCandle)
                                        {
                                            allSymbolsOfAlarmSyncd = false;

                                            break;
                                        }
                                    }
                                }
                            }

                            //
                            if (allSymbolsOfAlarmSyncd)
                            {
                                var alarmCalculatedInThisUpdate = false;

                                if (LastDateTimeAlarmsCalculated.ContainsKey(alarm.Item2))
                                {
                                    if (LastDateTimeAlarmsCalculated[alarm.Item2] == syncdDateTime.Value)
                                    {
                                        alarmCalculatedInThisUpdate = true;
                                    }
                                    else
                                    {
                                        LastDateTimeAlarmsCalculated[alarm.Item2] = syncdDateTime.Value;
                                    }
                                }
                                else
                                {
                                    LastDateTimeAlarmsCalculated[alarm.Item2] = syncdDateTime.Value;
                                }

                                if (!alarmCalculatedInThisUpdate)
                                {
                                    if (alarm.Item2.Calculate(null))
                                    {
                                        var candle = OnOperationCandleRequested(alarm.Item1.Symbol, TimeFrames.Minute1, 0);

                                        alarm.Item1.TriggerAlarm(candle.MomentaryDateTime, candle.Close);

                                        alarmsHistory.Add((SymbolAlarmDataModel)alarm.Item1.Clone());

                                        File.AppendAllLines(ServerAddressHelper.AlarmHistoryFile, new string[] { alarm.Item1.ToString() });
                                    }
                                }
                            }

                            if (!alarm.Item1.Seen)
                            {
                                enabledAlarms.Add(alarm.Item1);
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                if (enabledAlarms.Count > 0)
                {
                    NewAlarmsReceived?.Invoke(enabledAlarms);
                }

                if (alarmsHistory.Count > 0)
                {
                    NewAlarmsHistoryReceived?.Invoke(alarmsHistory);
                }

                //
                foreach (var symbol in Market.SupportedSymbols)
                {
                    var technicalAnalysis = TechnicalAnalysis[symbol];

                    technicalAnalysis.SaveDataToDataBase(null, DatabaseSavingDataMode.Immediately);
                }

                //
                MarketDataChanged?.Invoke();
            }

            //
            StatusBarInformationReceived?.Invoke(new Tuple<DateTime, bool>(currentDateTime, allSymbolsSync));
        }

        private CandleDataModel OnOperationCandleRequested(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber)
        {
            CandleDataModel result = null;

            if (TechnicalAnalysis.ContainsKey(symbol))
            {
                if (TechnicalAnalysis[symbol].TimeFrameCandles.ContainsKey(timeFrame))
                {
                    var candles = TechnicalAnalysis[symbol].TimeFrameCandles[timeFrame];

                    var index = candles.Count - 1 - candleNumber;

                    if (index >= 0 && index < candles.Count)
                    {
                        result = candles[candles.Count - 1 - candleNumber];
                    }
                }
            }

            return result;
        }

        private void Market_BinanceConnectionStatus(BinanceConnectionStatusModes status)
        {
            BinanceConnectionStatus?.Invoke(status);
        }

        public void Start()
        {
            //
            if (!WasCompiledAlarmLoaded)
            {
                WasCompiledAlarmLoaded = true;

                AssemblyHelper.CompiledAlarmDataFolder = ServerAddressHelper.CompiledAlarmDataFolder;

                foreach (var compiledScriptFile in Directory.GetFiles(ServerAddressHelper.CompiledAlarmDataFolder))
                {
                    var fileInfo = new FileInfo(compiledScriptFile);

                    if (fileInfo.Extension.ToLower() == ".dll")
                    {
                        AssemblyHelper.LoadAssemblyFile(compiledScriptFile);
                    }
                }
            }

            //
            var alarmFiles = Directory.GetFiles(ServerAddressHelper.AlarmDataFolder);

            foreach (var file in alarmFiles)
            {
                var script = File.ReadAllText(file);

                string name = "";
                SymbolTypes symbol = SymbolTypes.BtcUsdt;
                PositionTypes position = PositionTypes.Long;

                var alarm = AlarmHelper.ConvertStringToAlarmItem(script, ref name, ref symbol, ref position);

                if (alarm != null)
                {
                    var condition = AlarmHelper.CreateConditionFromAlarmItemDataModel(alarm, true, OnOperationCandleRequested);

                    if (condition != null)
                    {
                        //
                        var username = (new FileInfo(file)).Name.Split('_')[0];

                        //
                        var symbolAlarm = new SymbolAlarmDataModel(file, username)
                        {
                            Name = name,
                            Symbol = symbol,
                            Position = position,
                            Seen = true,
                            LastAlarm = DateTime.UtcNow,
                            Enabled = true
                        };

                        Alarms.Add(new Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[], string>(symbolAlarm, condition, alarm, AlarmHelper.GetNeededSymbols(condition), username));
                    }
                }
            }

            //
            Market = new MarketDataProviderService();

            Market.BinanceConnectionStatus += Market_BinanceConnectionStatus;

            Market.Start();

            for (var index = 0; index < Market.SupportedSymbols.Count; index++)
            {
                var symbol = Market.SupportedSymbols[index];

                var symbolDataModel = new SymbolDataModel()
                {
                    Symbol = symbol,
                    SymbolTimeFrames = new ObservableCollection<SymbolTimeFrameDataModel>(),
                    SymbolAlarms = new ObservableCollection<SymbolAlarmDataModel>(),
                    SupportsResistances = new ObservableCollection<SymbolSupportsResistancesDataModel>()
                };

                foreach (var alarm in Alarms)
                {
                    if (alarm.Item1.Symbol == symbol)
                    {
                        symbolDataModel.SymbolAlarms.Add(alarm.Item1);
                    }
                }

                foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
                {
                    symbolDataModel.SymbolTimeFrames.Add(new SymbolTimeFrameDataModel() { TimeFrame = timeFrame });
                }

                MarketData.Add(symbolDataModel);
            }
        }

        public void Stop()
        {
            try
            {
                UpdaterThread?.Abort();
            }
            catch
            {

            }

            try
            {
                Market.Stop();
            }
            catch
            {

            }
        }

        public void StartMarket(DateTime? saveCandlesToDatabaseDateTime, DateTime? saveMilestoneToDiskDateTime, CancellationToken cancellationToken)
        {
            //
            PreProcessServiceWorkingNotified?.Invoke(true);

            //
            int processedAllCandleDataModelsCount = 0;
            int allCandleDataModelsCount = 0;
            float currentMainProgressValue = 0;

            int processedDetailsCandleDataModelsCount = 0;
            int detailsCandleDataModelsCount = 0;
            float currentDetailsProgressValue = 0;

            var detailsCandleDataModelsCountList = new List<int>();

            for (var index = 0; index < Market.SupportedSymbols.Count; index++)
            {
                var symbol = Market.SupportedSymbols[index];

                detailsCandleDataModelsCountList.Add(Market.GetAvailableSpotMinuteCandleCount(symbol));

                allCandleDataModelsCount += detailsCandleDataModelsCountList.Last();
            }

            //
            for (var index = 0; index < Market.SupportedSymbols.Count; index++)
            {
                //
                var symbol = Market.SupportedSymbols[index];

                var symbolDataModel = MarketData[index];

                //
                processedDetailsCandleDataModelsCount = 0;
                detailsCandleDataModelsCount = detailsCandleDataModelsCountList[index];
                currentDetailsProgressValue = 0;

                DetailsProgressValueChanged?.Invoke(0, symbol);

                //
                var technicalAnalysis = TechnicalAnalysis[symbol] = new TechnicalAnalysisService(symbol, Market.GetLastSpotMinuteCandle(symbol).OpenDateTime, saveCandlesToDatabaseDateTime, saveMilestoneToDiskDateTime);

                technicalAnalysis.ServiceWorkingNotified += (value) => { AutoSavingServiceWorkingNotified?.Invoke(value); };

                technicalAnalysis.Start();

                CandleDataModel lastCandleDataModel = Market.GetNextMinuteCandle(symbol);

                while (lastCandleDataModel != null)
                {
                    //
                    technicalAnalysis.ApplyMinuteCandle(lastCandleDataModel, DatabaseSavingDataMode.Weekly);

                    //
                    processedDetailsCandleDataModelsCount += 1;

                    float detailsProgress = (float)Math.Round((float)processedDetailsCandleDataModelsCount / (float)detailsCandleDataModelsCount, 3) * 100f;

                    if (detailsProgress > 100f)
                    {
                        detailsProgress = 100f;
                    }

                    if (detailsProgress != currentDetailsProgressValue)
                    {
                        currentDetailsProgressValue = detailsProgress;

                        DetailsProgressValueChanged?.Invoke(detailsProgress, null);
                    }

                    //
                    processedAllCandleDataModelsCount += 1;

                    float mainProgress = (float)Math.Round((float)processedAllCandleDataModelsCount / (float)allCandleDataModelsCount, 3) * 100f;

                    if (mainProgress > 100f)
                    {
                        mainProgress = 100f;
                    }

                    if (mainProgress != currentMainProgressValue)
                    {
                        currentMainProgressValue = mainProgress;

                        MainProgressValueChanged?.Invoke(mainProgress, null);
                    }

                    //
                    lastCandleDataModel = Market.GetNextMinuteCandle(symbol);

                    //
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }

            //
            UpdaterThread = new Thread(() =>
            {
                var forceUpdate = true;

                for (; ; )
                {
                    UpdateMarket(forceUpdate);

                    forceUpdate = false;

                    Thread.Sleep(1000);
                }
            });

            UpdaterThread.Start();

            //
            PreProcessServiceWorkingNotified?.Invoke(false);
        }

        public bool RunAlarm(string script, string filename)
        {
            var result = false;

            string name = "";
            SymbolTypes symbol = SymbolTypes.BtcUsdt;
            PositionTypes position = PositionTypes.Long;

            var alarm = AlarmHelper.ConvertStringToAlarmItem(script, ref name, ref symbol, ref position);

            if (alarm != null)
            {
                var condition = AlarmHelper.CreateConditionFromAlarmItemDataModel(alarm, true, OnOperationCandleRequested);

                if (condition != null)
                {
                    //
                    var username = (new FileInfo(filename)).Name.Split('_')[0];

                    //
                    var symbolAlarm = new SymbolAlarmDataModel(filename, username)
                    {
                        Name = name,
                        Symbol = symbol,
                        Position = position,
                        Seen = true,
                        LastAlarm = DateTime.UtcNow,
                        Enabled = true
                    };

                    Alarms.Add(new Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[], string>(symbolAlarm, condition, alarm, AlarmHelper.GetNeededSymbols(condition), username));

                    //
                    if (Market.SupportedSymbols.Contains(symbol))
                    {
                        MarketData.First(p => p.Symbol == symbol).SymbolAlarms.Add(symbolAlarm);

                        result = true;
                    }
                }
            }

            return result;
        }

        public bool RemoveAlarm(string username, UiClientTypes clientType, Guid id)
        {
            var result = false;

            var alarm = Alarms.FirstOrDefault(p => p.Item1.Id == id).Item1;

            if (alarm != null)
            {
                if (alarm.Username == username || clientType == UiClientTypes.Admin)
                {
                    File.Delete(alarm.FileName);

                    for (var index = Alarms.Count - 1; index >= 0; index--)
                    {
                        if (Alarms[index].Item1 == alarm)
                        {
                            Alarms.RemoveAt(index);
                        }
                    }

                    result = true;
                }
            }

            return result;
        }

        public event ProgressValueChangedHandler MainProgressValueChanged;

        public event ProgressValueChangedHandler DetailsProgressValueChanged;

        public event ServiceWorkingNotifiedHandler PreProcessServiceWorkingNotified;

        public event ServiceWorkingNotifiedHandler AutoSavingServiceWorkingNotified;

        public event MarketDataChangedHandler MarketDataChanged;

        public event AlarmsReceivedHandler NewAlarmsReceived;

        public event AlarmsReceivedHandler NewAlarmsHistoryReceived;

        public event StatusBarInformationReceivedHandler StatusBarInformationReceived;

        public event BinanceConnectionStatusHandler BinanceConnectionStatus;
    }
}

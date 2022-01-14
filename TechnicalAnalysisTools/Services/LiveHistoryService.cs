using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Shared.Conditions;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Services
{
    public class LiveHistoryService
    {
        public LiveHistoryService(List<SymbolTypes> supportedSymbolTypes, List<Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[], string>> alarms)
        {
            //
            SymbolTimeFrameDataModelPropertyInfos = typeof(SymbolTimeFrameDataModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite).ToArray();

            //
            SupportedSymbolTypes = supportedSymbolTypes;

            //
            Alarms = alarms;

            //
            foreach (var symbol in SupportedSymbolTypes)
            {
                TimeFrameCandles[symbol] = new Dictionary<TimeFrames, IList<CandleDataModel>>();

                foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
                {
                    TimeFrameCandles[symbol][timeFrame] = new List<CandleDataModel>();
                }
            }

            //
            for (var index = 0; index < SupportedSymbolTypes.Count; index++)
            {
                var symbol = SupportedSymbolTypes[index];

                var symbolDataModel = new SymbolDataModel()
                {
                    Symbol = symbol,
                    SymbolTimeFrames = new ObservableCollection<SymbolTimeFrameDataModel>(),
                    SymbolAlarms = new ObservableCollection<SymbolAlarmDataModel>(),
                    SupportsResistances = new ObservableCollection<SymbolSupportsResistancesDataModel>()
                };

                foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
                {
                    symbolDataModel.SymbolTimeFrames.Add(new SymbolTimeFrameDataModel() { TimeFrame = timeFrame });
                }

                SymbolDataModelList.Add(symbolDataModel);
            }
        }

        private PropertyInfo[] SymbolTimeFrameDataModelPropertyInfos { get; }

        private List<SymbolTypes> SupportedSymbolTypes { get; }

        private List<Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[], string>> Alarms { get; }

        private Dictionary<SymbolTypes, Dictionary<TimeFrames, IList<CandleDataModel>>> TimeFrameCandles { get; } = new Dictionary<SymbolTypes, Dictionary<TimeFrames, IList<CandleDataModel>>>();

        public DateTime CurrentDateTime { get; private set; }

        public List<SymbolDataModel> SymbolDataModelList { get; } = new List<SymbolDataModel>();

        private CandleDataModel OnOperationCandleRequested(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber)
        {
            CandleDataModel result = null;

            if (SupportedSymbolTypes.Contains(symbol))
            {
                if (candleNumber == 0)
                {
                    var currentTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(CurrentDateTime);

                    result = DatabaseHelper.ReadCandleDataModelsFromDatabaseByMomentaryTimeStamp(DatabaseHelper.GetSymbolTableName(symbol, timeFrame), currentTimeStamp);
                }
                else
                {
                    //
                    var openDateTime = TimeFrameHelper.GetOpenDateTimeOfSpesificCandle(timeFrame, CurrentDateTime);

                    switch (timeFrame)
                    {
                        case TimeFrames.Minute1:
                            {
                                openDateTime = openDateTime.AddMinutes(-1 * candleNumber);
                            }
                            break;
                        case TimeFrames.Minute3:
                        case TimeFrames.Minute5:
                        case TimeFrames.Minute15:
                        case TimeFrames.Minute30:
                            {
                                openDateTime = openDateTime.AddMinutes(-1 * candleNumber * ((int)timeFrame));
                            }
                            break;
                        case TimeFrames.Hour1:
                        case TimeFrames.Hour2:
                        case TimeFrames.Hour4:
                        case TimeFrames.Hour6:
                        case TimeFrames.Hour8:
                        case TimeFrames.Hour12:
                            {
                                openDateTime = openDateTime.AddHours(-1 * candleNumber);
                            }
                            break;
                        case TimeFrames.Day1:
                            {
                                openDateTime = openDateTime.AddDays(-1 * candleNumber);
                            }
                            break;
                        case TimeFrames.Day3:
                            {
                                openDateTime = openDateTime.AddDays(-1 * candleNumber * 3);
                            }
                            break;
                        case TimeFrames.Week1:
                            {
                                openDateTime = openDateTime.AddDays(-1 * candleNumber * 7);
                            }
                            break;
                        case TimeFrames.Month1:
                            {
                                openDateTime = openDateTime.AddMonths(-1 * candleNumber);
                            }
                            break;
                        default:
                            {
                                throw new Exception("TimeFrame is not valid.");
                            }
                    }

                    //
                    var closeDateTime = TimeFrameHelper.GetCloseDateTimeOfSpesificCandle(timeFrame, openDateTime);

                    var closeTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(closeDateTime);

                    result = DatabaseHelper.ReadCandleDataModelsFromDatabaseByMomentaryTimeStamp(DatabaseHelper.GetSymbolTableName(symbol, timeFrame), closeTimeStamp);

                    if (result == null)
                    {
                        var openTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(openDateTime);

                        result = DatabaseHelper.ReadCandleDataModelsFromDatabaseByMomentaryTimeStamp(DatabaseHelper.GetSymbolTableName(symbol, timeFrame), openTimeStamp);
                    }
                }
            }

            return result;
        }

        private void SetDefaultSymbolDataModel(SymbolDataModel symbolDataModel)
        {
            symbolDataModel.LastMinuteCandle = new DateTime(1970, 1, 1);
            symbolDataModel.CandleType = CandleType.None;
            symbolDataModel.Open = 0;
            symbolDataModel.High = 0;
            symbolDataModel.Low = 0;
            symbolDataModel.Close = 0;
            symbolDataModel.Volume = 0;
            symbolDataModel.QuoteVolume = 0;
            symbolDataModel.NumberOfTrades = 0;

            foreach (var dataModel in symbolDataModel.SymbolTimeFrames)
            {
                dataModel.CandleType = CandleType.None;

                foreach (var proprty in SymbolTimeFrameDataModelPropertyInfos)
                {
                    if (proprty.PropertyType == typeof(float))
                    {
                        proprty.SetValue(dataModel, 0);
                    }
                    else if (proprty.PropertyType == typeof(DivergenceDirectionTypes))
                    {
                        proprty.SetValue(dataModel, DivergenceDirectionTypes.None);
                    }
                }
            }

            foreach (var alarm in symbolDataModel.SymbolAlarms)
            {
                alarm.Seen = true;
                alarm.LastAlarm = new DateTime(1970, 1, 1);
                alarm.NotSeenEnabledCount = 0;
                alarm.TotalEnabledCount = 0;
                alarm.Price = 0;
                alarm.Enabled = true;
            }
        }

        private bool LoadCurrentData(DateTime dateTime, SymbolTypes[] symbols)
        {
            var result = true;

            //
            foreach (var symbol in SupportedSymbolTypes)
            {
                foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
                {
                    TimeFrameCandles[symbol][timeFrame].Clear();
                }
            }

            //
            foreach (var symbol in SupportedSymbolTypes)
            {
                if (symbols.Contains(symbol))
                {
                    foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
                    {
                        var dataModel = DatabaseHelper.ReadCandleDataModelsFromDatabaseByMomentaryDateTime(DatabaseHelper.GetSymbolTableName(symbol, timeFrame), dateTime);

                        if (dataModel != null)
                        {
                            TimeFrameCandles[symbol][timeFrame].Add(dataModel);
                        }
                    }
                }
            }

            //
            foreach (var symbol in SupportedSymbolTypes)
            {
                if (symbols.Contains(symbol))
                {
                    var symbolDataModel = SymbolDataModelList.FirstOrDefault(p => p.Symbol == symbol);

                    SetDefaultSymbolDataModel(symbolDataModel);

                    foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
                    {
                        //
                        var dataModel = TimeFrameCandles[symbol][timeFrame].LastOrDefault();

                        if (dataModel != null)
                        {
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
                                var divergence = DivergenceDirectionTypes.None;

                                if (ascending > 0 && descending > 0)
                                {
                                    divergence = DivergenceDirectionTypes.Unknown;
                                }
                                else if (ascending > 0)
                                {
                                    divergence = DivergenceDirectionTypes.Ascending;
                                }
                                else if (descending > 0)
                                {
                                    divergence = DivergenceDirectionTypes.Descending;
                                }

                                return divergence;
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
                        else
                        {
                            SetDefaultSymbolDataModel(symbolDataModel);

                            break;
                        }
                    }
                }
            }

            //
            if (Alarms != null)
            {
                //
                var alarms = new List<Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[]>>();

                foreach (var alarm in Alarms)
                {
                    //
                    var symbolAlarmDataModel = (SymbolAlarmDataModel)alarm.Item1.Clone();

                    symbolAlarmDataModel.Seen = true;
                    symbolAlarmDataModel.LastAlarm = CurrentDateTime;
                    symbolAlarmDataModel.TotalEnabledCount = 0;
                    symbolAlarmDataModel.NotSeenEnabledCount = 0;
                    symbolAlarmDataModel.Price = 0;
                    symbolAlarmDataModel.Enabled = true;

                    //
                    var condition = AlarmHelper.CreateConditionFromAlarmItemDataModel(alarm.Item3, true, alarm.Item2.OperationCandleRequested, null);

                    alarms.Add(new Tuple<SymbolAlarmDataModel, ICondition, AlarmItemDataModel, SymbolTypes[]>(symbolAlarmDataModel, condition, alarm.Item3, alarm.Item4));
                }

                //
                for (var index = 0; index < SupportedSymbolTypes.Count; index++)
                {
                    var symbol = SupportedSymbolTypes[index];

                    if (symbols.Contains(symbol))
                    {
                        var symbolDataModel = SymbolDataModelList.FirstOrDefault(p => p.Symbol == symbol);

                        symbolDataModel.SymbolAlarms.Clear();

                        foreach (var alarm in alarms)
                        {
                            if (alarm.Item1.Symbol == symbol)
                            {
                                var addToSymbolAlarms = true;

                                var allSymbolsOfAlarm = alarm.Item4;

                                foreach (var symbolOfAlarm in allSymbolsOfAlarm)
                                {
                                    if (!symbols.Contains(symbolOfAlarm))
                                    {
                                        addToSymbolAlarms = false;

                                        break;
                                    }
                                }

                                if (addToSymbolAlarms)
                                {
                                    symbolDataModel.SymbolAlarms.Add(alarm.Item1);
                                }
                            }
                        }
                    }
                }

                foreach (var alarm in alarms)
                {
                    if (symbols.Contains(alarm.Item1.Symbol))
                    {
                        try
                        {
                            //
                            var alarmCanCalculate = true;

                            var allSymbolsOfAlarm = alarm.Item4;

                            //
                            foreach (var symbolOfAlarm in allSymbolsOfAlarm)
                            {
                                if (!symbols.Contains(symbolOfAlarm))
                                {
                                    alarmCanCalculate = false;

                                    break;
                                }
                            }

                            //
                            if (alarmCanCalculate)
                            {
                                DateTime? syncdDateTime = null;

                                foreach (var symbol in allSymbolsOfAlarm)
                                {
                                    var symbolDataModel = SymbolDataModelList.FirstOrDefault(p => p.Symbol == symbol);

                                    if (symbolDataModel == null)
                                    {
                                        alarmCanCalculate = false;

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
                                                alarmCanCalculate = false;

                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            //
                            if (alarmCanCalculate)
                            {
                                if (alarm.Item2.Calculate(OnOperationCandleRequested))
                                {
                                    alarm.Item1.TriggerAlarm(dateTime, 0);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return result;
        }

        public DateTime GetFirstUpdateDateTime()
        {
            return DatabaseHelper.GetFirstDateTime(DatabaseHelper.GetSymbolTableName(SupportedSymbolTypes[0], TimeFrames.Minute1)).Value;
        }

        public DateTime GetLastUpdateDateTime()
        {
            return DatabaseHelper.GetLastDateTime(DatabaseHelper.GetSymbolTableName(SupportedSymbolTypes[0], TimeFrames.Minute1)).Value;
        }

        public bool Update(DateTime currentDateTime, SymbolTypes[] symbols)
        {
            var result = false;

            if (currentDateTime >= GetFirstUpdateDateTime() && currentDateTime <= GetLastUpdateDateTime())
            {
                var oldCurrentDateTime = CurrentDateTime;

                CurrentDateTime = currentDateTime;

                if (LoadCurrentData(currentDateTime, symbols))
                {
                    result = true;
                }
                else
                {
                    CurrentDateTime = oldCurrentDateTime;
                }
            }

            return result;
        }

        public bool UpdateToFirstAvailableDateTime(SymbolTypes[] symbols)
        {
            return Update(GetFirstUpdateDateTime(), symbols);
        }

        public bool UpdateToLastAvailableDateTime(SymbolTypes[] symbols)
        {
            return Update(GetLastUpdateDateTime(), symbols);
        }

        public AlarmItemDataModel EvaluateAlarm(AlarmItemDataModel alarmItem)
        {
            var result = AlarmHelper.ExpandPeriodicAlarmItem(alarmItem, null);

            var dictionary = new Dictionary<Guid, AlarmItemDataModel>();

            var stack = new Stack<AlarmItemDataModel>();

            stack.Push(result);

            while (stack.Count != 0)
            {
                var item = stack.Pop();

                dictionary.Add(item.Id, item);

                foreach (var child in item.Items)
                {
                    stack.Push(child);
                }
            }

            var condition = AlarmHelper.CreateConditionFromAlarmItemDataModel(result, false, OnOperationCandleRequested, (id, conditionResult) =>
            {
                if (dictionary.ContainsKey(id))
                {
                    dictionary[id].Tag = conditionResult;
                }
            });

            condition.Calculate(null);

            return result;
        }
    }
}

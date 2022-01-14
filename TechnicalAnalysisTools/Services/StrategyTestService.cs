using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TechnicalAnalysisTools.Auxiliaries;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Shared.Conditions;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Services
{
    public class StrategyTestService
    {
        static StrategyTestService()
        {
            FieldInfos = typeof(CandleDataModel).GetFields(BindingFlags.Public | BindingFlags.Instance).Where(p => !p.IsNotSerialized).ToArray();
        }

        public StrategyTestService(StrategyTestDataModel strategyTestData, Guid sessionId, string testDirectoryAddress)
        {
            StrategyTestData = strategyTestData;

            SessionId = sessionId;

            TestDirectoryAddress = testDirectoryAddress;

            StrategyTestReportFile = Path.Combine(TestDirectoryAddress, "StrategyTestReport.csv");
            StrategyTestStatusFile = Path.Combine(TestDirectoryAddress, "StrategyTestStatus.csv");

            File.WriteAllLines(StrategyTestReportFile, new string[] { StrategyTestReportDataModel.FieldNamesToString() });
            File.WriteAllLines(StrategyTestStatusFile, new string[] { StrategyTestStatusDataModel.FieldNamesToString() });
        }

        private static FieldInfo[] FieldInfos;

        private Random RandomNumberGenerator { get; } = new Random();

        private StrategyTestDataModel StrategyTestData { get; set; }

        private Guid SessionId { get; set; }

        private int OrderId { get; set; } = 0;

        private int SubOrderId { get; set; } = 0;

        private int ActionId { get; set; } = 0;

        private string TestDirectoryAddress { get; set; }

        private string StrategyTestReportFile { get; set; }

        private string StrategyTestStatusFile { get; set; }

        private bool WasStopped { get; set; } = false;

        private Dictionary<SymbolTypes, Dictionary<TimeFrames, DatabaseSequenceCandleReaderService>> DatabaseSequenceCandleReaders { get; set; }

        private Dictionary<SymbolTypes, Dictionary<TimeFrames, IList<CandleDataModel>>> SymbolTimeFrameCandles { get; set; }

        private ICondition EnterAlarmCondition { get; set; }

        private ICondition TakeProfitAlarmCondition { get; set; }

        private ICondition StopLossAlarmCondition { get; set; }

        private List<TradeSubOrderAuxiliary> TradeSubOrders { get; set; } = new List<TradeSubOrderAuxiliary>();

        private TradeWalletAuxiliary TradeWallet { get; set; }

        private IList<SymbolTypes> GetNeededSymbols(IList<ICondition> conditions)
        {
            var result = new List<SymbolTypes>();

            result.Add(StrategyTestData.Symbol);

            foreach (var condition in conditions)
            {
                if (condition != null)
                {
                    var stack = new Stack<ICondition>();

                    stack.Push(condition);

                    while (stack.Count != 0)
                    {
                        //
                        var currentCondition = stack.Pop();

                        //
                        if (currentCondition is LogicalOperationConditionBase)
                        {
                            foreach (var child in ((LogicalOperationConditionBase)currentCondition).Conditions)
                            {
                                stack.Push(child);
                            }

                        }
                        else if (currentCondition is CandleOperationConditionBase)
                        {
                            var currentCandleCondition = currentCondition as CandleOperationConditionBase;

                            if (!result.Contains(currentCandleCondition.Symbol))
                            {
                                result.Add(currentCandleCondition.Symbol);
                            }
                        }
                        else
                        {
                            throw new Exception("Condition is not valid.");
                        }
                    }
                }
            }

            return result;
        }

        private Dictionary<SymbolTypes, Dictionary<TimeFrames, DatabaseSequenceCandleReaderService>> CreateDatabaseSequenceCandleReaders(IList<SymbolTypes> symbols)
        {
            Dictionary<SymbolTypes, Dictionary<TimeFrames, DatabaseSequenceCandleReaderService>> result = null;

            if (symbols.Count != 0)
            {
                var errorOccured = false;

                var databaseSequences = new Dictionary<SymbolTypes, Dictionary<TimeFrames, DatabaseSequenceCandleReaderService>>();

                var startTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(StrategyTestData.FromDateTime) - 60;
                var endTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(StrategyTestData.ToDateTime) + 60;

                foreach (var symbol in symbols)
                {
                    var symbolDatabaseSequences = new Dictionary<TimeFrames, DatabaseSequenceCandleReaderService>();

                    foreach (var timeFrame in (TimeFrames[])Enum.GetValues(typeof(TimeFrames)))
                    {
                        var script = string.Format("SELECT * FROM [{0}] WHERE {1} < [MomentaryTimeStamp] AND [MomentaryTimeStamp] < {2}", DatabaseHelper.GetSymbolTableName(symbol, timeFrame), startTimeStamp, endTimeStamp);

                        var databaseSequence = new DatabaseSequenceCandleReaderService(script);

                        if (databaseSequence.Start())
                        {
                            symbolDatabaseSequences.Add(timeFrame, databaseSequence);
                        }
                        else
                        {
                            errorOccured = true;

                            break;
                        }
                    }

                    databaseSequences.Add(symbol, symbolDatabaseSequences);

                    if (errorOccured)
                    {
                        break;
                    }
                }

                if (!errorOccured)
                {
                    result = databaseSequences;
                }
            }

            return result;
        }

        private void DisposeDatabaseSequenceCandleReaders(Dictionary<SymbolTypes, Dictionary<TimeFrames, DatabaseSequenceCandleReaderService>> databaseSequenceCandleReaders)
        {
            if (databaseSequenceCandleReaders != null)
            {
                foreach (var symbol in databaseSequenceCandleReaders.Keys)
                {
                    var timeFrameSequenceCandleReaders = databaseSequenceCandleReaders[symbol];

                    foreach (var timeFrame in (TimeFrames[])Enum.GetValues(typeof(TimeFrames)))
                    {
                        if (timeFrameSequenceCandleReaders.ContainsKey(timeFrame))
                        {
                            var sequenceCandleReader = timeFrameSequenceCandleReaders[timeFrame];

                            if (sequenceCandleReader != null)
                            {
                                sequenceCandleReader.Stop();
                            }
                        }
                    }
                }
            }
        }

        private CandleDataModel OnOperationCandleRequested(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber)
        {
            CandleDataModel result = null;

            if (SymbolTimeFrameCandles != null)
            {
                if (SymbolTimeFrameCandles.ContainsKey(symbol))
                {
                    if (SymbolTimeFrameCandles[symbol].ContainsKey(timeFrame))
                    {
                        var candles = SymbolTimeFrameCandles[symbol][timeFrame];

                        var index = candles.Count - 1 - candleNumber;

                        if (index >= 0 && index < candles.Count)
                        {
                            result = candles[candles.Count - 1 - candleNumber];
                        }
                    }
                }
            }

            return result;
        }

        private Dictionary<SymbolTypes, Dictionary<TimeFrames, CandleDataModel>> ReadInitializedSyncedSymbolTimeFrameCandle(out DateTime? firstDateTime)
        {
            var result = new Dictionary<SymbolTypes, Dictionary<TimeFrames, CandleDataModel>>();

            firstDateTime = null;

            //
            foreach (var symbol in DatabaseSequenceCandleReaders.Keys)
            {
                if (DatabaseSequenceCandleReaders.ContainsKey(symbol))
                {
                    var timeFrameSequenceCandleReaders = DatabaseSequenceCandleReaders[symbol];

                    result.Add(symbol, new Dictionary<TimeFrames, CandleDataModel>());

                    foreach (var timeFrame in (TimeFrames[])Enum.GetValues(typeof(TimeFrames)))
                    {
                        if (timeFrameSequenceCandleReaders.ContainsKey(timeFrame))
                        {
                            var sequenceCandleReader = timeFrameSequenceCandleReaders[timeFrame];

                            var dataModel = sequenceCandleReader.Next();

                            if (dataModel != null)
                            {
                                result[symbol].Add(timeFrame, dataModel);

                                if (firstDateTime.HasValue)
                                {
                                    if (dataModel.MomentaryDateTime > firstDateTime)
                                    {
                                        firstDateTime = dataModel.MomentaryDateTime;
                                    }
                                }
                                else
                                {
                                    firstDateTime = dataModel.MomentaryDateTime;
                                }
                            }
                            else
                            {
                                result = null;

                                break;
                            }
                        }
                        else
                        {
                            result = null;

                            break;
                        }
                    }

                    if (result == null)
                    {
                        break;
                    }
                }
                else
                {
                    result = null;

                    break;
                }
            }

            //
            if (!firstDateTime.HasValue)
            {
                result = null;
            }

            if (result == null)
            {
                firstDateTime = null;
            }
            else
            {
                var initializedDateTime = firstDateTime.Value;

                foreach (var symbol in DatabaseSequenceCandleReaders.Keys)
                {
                    foreach (var timeFrame in (TimeFrames[])Enum.GetValues(typeof(TimeFrames)))
                    {
                        var dataModel = result[symbol][timeFrame];

                        if (dataModel.MomentaryDateTime != initializedDateTime)
                        {
                            var sequenceCandleReader = DatabaseSequenceCandleReaders[symbol][timeFrame];

                            var newDataModel = sequenceCandleReader.Next();

                            while (true)
                            {
                                if (newDataModel == null)
                                {
                                    result = null;

                                    break;
                                }
                                else
                                {
                                    if (newDataModel.MomentaryDateTime == initializedDateTime)
                                    {
                                        result[symbol][timeFrame] = newDataModel;

                                        break;
                                    }
                                    else
                                    {
                                        newDataModel = sequenceCandleReader.Next();
                                    }
                                }
                            }

                            if (result[symbol][timeFrame].MomentaryDateTime != initializedDateTime)
                            {
                                result = null;
                            }

                            if (result == null)
                            {
                                break;
                            }
                        }
                    }

                    if (result == null)
                    {
                        firstDateTime = null;

                        break;
                    }
                }
            }

            return result;
        }

        private Dictionary<SymbolTypes, Dictionary<TimeFrames, CandleDataModel>> NextTimeFrameCandle(out int readCandlesCount)
        {
            var result = new Dictionary<SymbolTypes, Dictionary<TimeFrames, CandleDataModel>>();

            readCandlesCount = 0;

            DateTime? candlesDateTime = null;

            foreach (var symbol in DatabaseSequenceCandleReaders.Keys)
            {
                var timeFrameSequenceCandleReaders = DatabaseSequenceCandleReaders[symbol];

                result.Add(symbol, new Dictionary<TimeFrames, CandleDataModel>());

                foreach (var timeFrame in (TimeFrames[])Enum.GetValues(typeof(TimeFrames)))
                {
                    if (timeFrameSequenceCandleReaders.ContainsKey(timeFrame))
                    {
                        var sequenceCandleReader = timeFrameSequenceCandleReaders[timeFrame];

                        var dataModel = sequenceCandleReader.Next();

                        if (dataModel != null)
                        {
                            result[symbol].Add(timeFrame, dataModel);

                            readCandlesCount++;

                            if (candlesDateTime.HasValue)
                            {
                                if (dataModel.MomentaryDateTime != candlesDateTime)
                                {
                                    result = null;

                                    break;
                                }
                            }
                            else
                            {
                                candlesDateTime = dataModel.MomentaryDateTime;
                            }
                        }
                        else
                        {
                            result = null;

                            break;
                        }
                    }
                    else
                    {
                        result = null;

                        break;
                    }
                }

                if (result == null)
                {
                    break;
                }
            }

            if (result == null)
            {
                readCandlesCount = 0;
            }

            return result;
        }

        private void SaveDataToTestDirectory(StrategyTestStatusDataModel strategyTestStatus)
        {
            if (strategyTestStatus != null)
            {
                //
                File.AppendAllLines(StrategyTestStatusFile, new string[] { strategyTestStatus.ToString() });

                //
                if (strategyTestStatus.StrategyTestReport != null)
                {
                    File.AppendAllLines(StrategyTestReportFile, new string[] { strategyTestStatus.StrategyTestReport.ToString() });
                }
            }
        }

        private bool CalculateCondition(ICondition condition)
        {
            var result = condition.Calculate(null);

            if (!condition.AreNeededCandlesAvailable)
            {
                result = false;
            }

            return result;
        }

        private void ApplyCandle(Dictionary<SymbolTypes, Dictionary<TimeFrames, CandleDataModel>> candles)
        {
            var timeFrames = (TimeFrames[])Enum.GetValues(typeof(TimeFrames));

            foreach (var symbol in candles.Keys)
            {
                for (var index = 0; index < timeFrames.Length; index++)
                {
                    //
                    var timeFrame = timeFrames[index];

                    var candle = candles[symbol][timeFrame];
                    var symbolTimeFrameCandles = SymbolTimeFrameCandles[symbol][timeFrame];

                    //
                    var lastTimeFrameCandles = symbolTimeFrameCandles.LastOrDefault();

                    if (lastTimeFrameCandles != null && candle.OpenDateTime == lastTimeFrameCandles.OpenDateTime)
                    {
                        foreach (var fieldInfo in FieldInfos)
                        {
                            fieldInfo.SetValue(lastTimeFrameCandles, fieldInfo.GetValue(candle));
                        }
                    }
                    else
                    {
                        symbolTimeFrameCandles.Add(candle);
                    }

                    //
                    if (symbolTimeFrameCandles.Count > ServerConstantHelper.MaximumCandlesInMemory)
                    {
                        var selectedIndexToDelete = symbolTimeFrameCandles.Count - ServerConstantHelper.MaximumCandlesInMemory - 1;

                        symbolTimeFrameCandles[selectedIndexToDelete] = null;

                        symbolTimeFrameCandles.RemoveAt(selectedIndexToDelete);
                    }
                }
            }
        }

        private void AddLogToStrategyTestStatus(TradeSubOrderAuxiliary tradeSubOrder, TradeSubOrderActions tradeSubOrderAction, StrategyTestStatusDataModel strategyTestStatus, DateTime currentTime, float price)
        {
            ActionId++;

            var strategyTestLog = new StrategyTestLogDataModel
            {
                ActionId = ActionId,
                OrderId = tradeSubOrder.OrderId,
                SubOrderId = tradeSubOrder.Id,
                Time = currentTime,
                SubOrderAction = tradeSubOrderAction,
                Price = price
            };

            strategyTestStatus.StrategyTestLogs.Add(strategyTestLog);
        }

        private void AddSubOrderToStrategyTestStatus(TradeSubOrderAuxiliary tradeSubOrder, TradeSubOrderActions tradeSubOrderAction, StrategyTestStatusDataModel strategyTestStatus)
        {
            var strategyTestOrder = new StrategyTestOrderDataModel
            {
                OrderId = tradeSubOrder.OrderId,
                SubOrderId = tradeSubOrder.Id,
                LastTradeSubOrderAction = tradeSubOrderAction,
                StartTime = tradeSubOrder.OpenTime,
                EndTime = tradeSubOrder.CloseTime,
                IsOpened = tradeSubOrder.IsOpened,
                IsClosed = tradeSubOrder.IsClosed,
                Won = false,
                EnterPrice = tradeSubOrder.OpenPrice,
                ExitPrice = tradeSubOrder.ClosePrice
            };

            if (tradeSubOrderAction == TradeSubOrderActions.Cancel || tradeSubOrderAction == TradeSubOrderActions.CancelBySplittedTakeProfit)
            {
                strategyTestOrder.Won = false;
                strategyTestOrder.Size = 0;
                strategyTestOrder.Fee = 0;
                strategyTestOrder.Profit = 0;
                strategyTestOrder.ProfitPercent = 0;
                strategyTestOrder.SavedProfit = 0;
            }
            else if (tradeSubOrder.TradeAllocatedBalance != null)
            {
                strategyTestOrder.Won = tradeSubOrder.TradeAllocatedBalance.Profit > 0;
                strategyTestOrder.Size = tradeSubOrder.TradeAllocatedBalance.EnterLeveragedBalance;
                strategyTestOrder.Fee = tradeSubOrder.TradeAllocatedBalance.TotalMarketFee;
                strategyTestOrder.Profit = tradeSubOrder.TradeAllocatedBalance.Profit;
                strategyTestOrder.ProfitPercent = tradeSubOrder.TradeAllocatedBalance.Profit / tradeSubOrder.TradeAllocatedBalance.AllocatedQuoteBalance * 100f;
                strategyTestOrder.SavedProfit = StrategyTestData.SaveProfitPercentOfWinPosition / 100f * tradeSubOrder.TradeAllocatedBalance.Profit;
            }

            strategyTestStatus.StrategyTestOrders.Add(strategyTestOrder);
        }

        private bool AdjustEnterdTradeSubOrderTradeMode(TradeSubOrderAuxiliary tradeSubOrder, float price)
        {
            var result = true;

            // TakeProfitTradeSubOrderMode
            switch (StrategyTestData.ExitTakeProfit.TradeSubOrderTriggerMode)
            {
                case TradeSubOrderTriggerModes.Fixed:
                    {
                        if (StrategyTestData.Position == PositionTypes.Short)
                        {
                            if (price - StrategyTestData.ExitTakeProfit.FixedAmount >= 0)
                            {
                                tradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.TakeProfit, price - StrategyTestData.ExitTakeProfit.FixedAmount, FixedModeActiveRangeTypes.Up);
                            }
                            else
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            tradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.TakeProfit, price + StrategyTestData.ExitTakeProfit.FixedAmount, FixedModeActiveRangeTypes.Down);
                        }
                    }
                    break;
                case TradeSubOrderTriggerModes.Percent:
                    {
                        if (StrategyTestData.Position == PositionTypes.Short)
                        {
                            tradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.TakeProfit, price * (1f - (StrategyTestData.ExitTakeProfit.PercentAmount / 100f)), FixedModeActiveRangeTypes.Up);
                        }
                        else
                        {
                            tradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.TakeProfit, price * (1f + (StrategyTestData.ExitTakeProfit.PercentAmount / 100f)), FixedModeActiveRangeTypes.Down);
                        }
                    }
                    break;
            }

            // StopLossTradeSubOrderMode
            if (result)
            {
                switch (StrategyTestData.ExitStopLoss.TradeSubOrderTriggerMode)
                {
                    case TradeSubOrderTriggerModes.Fixed:
                        {
                            if (StrategyTestData.Position == PositionTypes.Long)
                            {
                                if (price - StrategyTestData.ExitStopLoss.FixedAmount >= 0)
                                {
                                    tradeSubOrder.StopLossTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.StopLoss, price - StrategyTestData.ExitStopLoss.FixedAmount, FixedModeActiveRangeTypes.Up);
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            else
                            {
                                tradeSubOrder.StopLossTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.StopLoss, price + StrategyTestData.ExitStopLoss.FixedAmount, FixedModeActiveRangeTypes.Down);
                            }
                        }
                        break;
                    case TradeSubOrderTriggerModes.Percent:
                        {
                            if (StrategyTestData.Position == PositionTypes.Long)
                            {
                                tradeSubOrder.StopLossTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.StopLoss, price * (1f - (StrategyTestData.ExitStopLoss.PercentAmount / 100f)), FixedModeActiveRangeTypes.Up);
                            }
                            else
                            {
                                tradeSubOrder.StopLossTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.StopLoss, price * (1f + (StrategyTestData.ExitStopLoss.PercentAmount / 100f)), FixedModeActiveRangeTypes.Down);
                            }
                        }
                        break;
                }
            }

            // LiquidTradeSubOrderMode
            if (result)
            {
                if (StrategyTestData.Position == PositionTypes.Long)
                {
                    tradeSubOrder.LiquidTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.Liquid, tradeSubOrder.TradeAllocatedBalance.GetLiquidPrice(), FixedModeActiveRangeTypes.Up);
                }
                else
                {
                    tradeSubOrder.LiquidTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.Liquid, tradeSubOrder.TradeAllocatedBalance.GetLiquidPrice(), FixedModeActiveRangeTypes.Down);
                }
            }

            return result;
        }

        private TradeSubOrderAuxiliary CreateEnterdTradeSubOrderAuxiliary(CandleDataModel candle, TradeAllocatedBalanceAuxiliary tradeAllocatedBalance, int orderId, int subOrderId)
        {
            TradeSubOrderAuxiliary result = null;

            if (tradeAllocatedBalance != null)
            {
                result = new TradeSubOrderAuxiliary()
                {
                    Id = subOrderId,
                    OrderId = orderId,
                    TradeSubStatusType = TradeSubStatusTypes.Entered,
                    OpenTime = candle.MomentaryDateTime,
                    CloseTime = DateTime.MinValue,
                    OpenPrice = candle.Close,
                    ClosePrice = 0,
                    TradeAllocatedBalance = tradeAllocatedBalance
                };

                if (!AdjustEnterdTradeSubOrderTradeMode(result, candle.Close))
                {
                    result = null;
                }
            }

            return result;
        }

        private void ApplyTakeProfitTradeTriggerModeOnEnteredTradeSubOrder(TradeSubOrderAuxiliary tradeSubOrder, StrategyTestStatusDataModel strategyTestStatus, DateTime currentTime, float closePrice)
        {
            switch (StrategyTestData.ExitTakeProfit.TradeSubOrderMode)
            {
                case TradeSubOrderModes.None:
                    {
                        tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Exited;
                        tradeSubOrder.CloseTime = currentTime;
                        tradeSubOrder.ClosePrice = closePrice;
                        tradeSubOrder.SetTradeSubOrderModeObjectsToNull();

                        tradeSubOrder.TradeAllocatedBalance.SetExitPrice(closePrice);
                        TradeWallet.DeallocateSuccessfulTrade(tradeSubOrder.TradeAllocatedBalance);

                        AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.TakeProfit, strategyTestStatus, currentTime, closePrice);
                        AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.TakeProfit, strategyTestStatus);
                    }
                    break;
                case TradeSubOrderModes.TrailingOrder:
                    {
                        tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.PendingForExit;

                        if (StrategyTestData.Position == PositionTypes.Long)
                        {
                            tradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderTrailingOrderModeAuxiliary(closePrice, TrailingDirectionTypes.Up, StrategyTestData.ExitTakeProfit.TrailingOrder.TolerantPercentForLoss);
                        }
                        else
                        {
                            tradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderTrailingOrderModeAuxiliary(closePrice, TrailingDirectionTypes.Down, StrategyTestData.ExitTakeProfit.TrailingOrder.TolerantPercentForLoss);
                        }

                        AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.PendingExitByTakeProfit, strategyTestStatus, currentTime, closePrice);
                        AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.PendingExitByTakeProfit, strategyTestStatus);
                    }
                    break;
                case TradeSubOrderModes.GridOrder:
                    {
                        //
                        tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.CancelledBySplittedTakeProfit;

                        AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.CancelBySplittedTakeProfit, strategyTestStatus, currentTime, closePrice);
                        AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.CancelBySplittedTakeProfit, strategyTestStatus);

                        //
                        var splitedTradeAllocatedBalance = TradeWallet.SplitTradeAllocatedBalance(tradeSubOrder.TradeAllocatedBalance, StrategyTestData.ExitTakeProfit.GridOrder.StepCount);

                        for (var index = 0; index < StrategyTestData.ExitTakeProfit.GridOrder.StepCount; index++)
                        {
                            //
                            SubOrderId++;

                            var newTradeSubOrder = new TradeSubOrderAuxiliary()
                            {
                                Id = SubOrderId,
                                OrderId = tradeSubOrder.OrderId,
                                TradeSubStatusType = TradeSubStatusTypes.PendingForExit,
                                OpenTime = tradeSubOrder.OpenTime,
                                CloseTime = tradeSubOrder.CloseTime,
                                OpenPrice = tradeSubOrder.OpenPrice,
                                ClosePrice = tradeSubOrder.ClosePrice,
                                TradeAllocatedBalance = splitedTradeAllocatedBalance[index]
                            };

                            if (StrategyTestData.Position == PositionTypes.Long)
                            {
                                newTradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.TakeProfit, closePrice * (1f + StrategyTestData.ExitTakeProfit.GridOrder.Percent / 100f / StrategyTestData.ExitTakeProfit.GridOrder.StepCount * index), FixedModeActiveRangeTypes.Down);
                            }
                            else
                            {
                                newTradeSubOrder.TakeProfitTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.TakeProfit, closePrice * (1f - StrategyTestData.ExitTakeProfit.GridOrder.Percent / 100f / StrategyTestData.ExitTakeProfit.GridOrder.StepCount * index), FixedModeActiveRangeTypes.Up);
                            }

                            newTradeSubOrder.StopLossTradeSubOrderMode = tradeSubOrder.StopLossTradeSubOrderMode;
                            newTradeSubOrder.LiquidTradeSubOrderMode = tradeSubOrder.LiquidTradeSubOrderMode;

                            TradeSubOrders.Add(newTradeSubOrder);

                            AddLogToStrategyTestStatus(newTradeSubOrder, TradeSubOrderActions.PendingExitBySplittedTakeProfit, strategyTestStatus, currentTime, closePrice);
                            AddSubOrderToStrategyTestStatus(newTradeSubOrder, TradeSubOrderActions.PendingExitBySplittedTakeProfit, strategyTestStatus);

                            //
                            if (index == 0)
                            {
                                newTradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Exited;
                                newTradeSubOrder.CloseTime = currentTime;
                                newTradeSubOrder.ClosePrice = closePrice;
                                newTradeSubOrder.TradeAllocatedBalance.SetExitPrice(closePrice);
                                newTradeSubOrder.SetTradeSubOrderModeObjectsToNull();

                                TradeWallet.DeallocateSuccessfulTrade(newTradeSubOrder.TradeAllocatedBalance);

                                AddLogToStrategyTestStatus(newTradeSubOrder, TradeSubOrderActions.TakeProfit, strategyTestStatus, currentTime, closePrice);
                                AddSubOrderToStrategyTestStatus(newTradeSubOrder, TradeSubOrderActions.TakeProfit, strategyTestStatus);
                            }
                        }
                    }
                    break;
            }
        }

        private void TraverseCandleForSubOrder(TradeSubOrderAuxiliary tradeSubOrder, CandleDataModel candle, float fromPrice, float toPrice, ref bool mustPendingEnterSubOrdersBeCanceled, ref float priceToPendingEnterSubOrdersMustBeCanceled, StrategyTestStatusDataModel strategyTestStatus)
        {
            switch (tradeSubOrder.TradeSubStatusType)
            {
                case TradeSubStatusTypes.PendingForEnter:
                    {
                        float matchedPrice;

                        if (tradeSubOrder.EnterTradeSubOrderMode.TraversePrice(fromPrice, toPrice, out matchedPrice))
                        {
                            tradeSubOrder.TradeAllocatedBalance.SetEnterPrice(matchedPrice);
                            tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Entered;
                            tradeSubOrder.OpenTime = candle.MomentaryDateTime;
                            tradeSubOrder.OpenPrice = matchedPrice;
                            tradeSubOrder.EnterTradeSubOrderMode = null;

                            if (AdjustEnterdTradeSubOrderTradeMode(tradeSubOrder, matchedPrice))
                            {
                                AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Enter, strategyTestStatus, candle.MomentaryDateTime, matchedPrice);
                                AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Enter, strategyTestStatus);

                                TraverseCandleForSubOrder(tradeSubOrder, candle, matchedPrice, toPrice, ref mustPendingEnterSubOrdersBeCanceled, ref priceToPendingEnterSubOrdersMustBeCanceled, strategyTestStatus);
                            }
                            else
                            {
                                tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Cancelled;

                                TradeWallet.DeallocateFailedTrade(tradeSubOrder.TradeAllocatedBalance);

                                AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus, candle.MomentaryDateTime, matchedPrice);
                                AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus);
                            }
                        }
                    }
                    break;
                case TradeSubStatusTypes.Entered:
                case TradeSubStatusTypes.PendingForExit:
                    {
                        //
                        float takeProfitMatchedPrice = 0;
                        float stopLossMatchedPrice = 0;
                        float liquidMatchedPrice;

                        var takeProfitResult = false;
                        var stopLossResult = false;
                        bool liquidResult;

                        if (StrategyTestData.ExitTakeProfit.TradeSubOrderTriggerMode != TradeSubOrderTriggerModes.Alarm)
                        {
                            takeProfitResult = tradeSubOrder.TakeProfitTradeSubOrderMode.TraversePrice(fromPrice, toPrice, out takeProfitMatchedPrice);
                        }

                        if (StrategyTestData.ExitStopLoss.TradeSubOrderTriggerMode != TradeSubOrderTriggerModes.Alarm)
                        {
                            stopLossResult = tradeSubOrder.StopLossTradeSubOrderMode.TraversePrice(fromPrice, toPrice, out stopLossMatchedPrice);
                        }

                        liquidResult = tradeSubOrder.LiquidTradeSubOrderMode.TraversePrice(fromPrice, toPrice, out liquidMatchedPrice);

                        //
                        if (takeProfitResult || stopLossResult || liquidResult)
                        {
                            //
                            var takeProfitMatchedPriceDistance = takeProfitResult ? Math.Abs(takeProfitMatchedPrice - fromPrice) : float.MaxValue;
                            var stopLossMatchedPriceDistance = stopLossResult ? Math.Abs(stopLossMatchedPrice - fromPrice) : float.MaxValue;
                            var liquidMatchedPriceDistance = liquidResult ? Math.Abs(liquidMatchedPrice - fromPrice) : float.MaxValue;

                            var minimumDistanceToFromPrice = Math.Min(takeProfitMatchedPriceDistance, Math.Min(stopLossMatchedPriceDistance, liquidMatchedPriceDistance));

                            //
                            mustPendingEnterSubOrdersBeCanceled = true;

                            if (minimumDistanceToFromPrice == takeProfitMatchedPriceDistance)
                            {
                                priceToPendingEnterSubOrdersMustBeCanceled = takeProfitMatchedPrice;
                            }
                            else if (minimumDistanceToFromPrice == stopLossMatchedPriceDistance)
                            {
                                priceToPendingEnterSubOrdersMustBeCanceled = stopLossMatchedPrice;
                            }
                            else if (minimumDistanceToFromPrice == liquidMatchedPriceDistance)
                            {
                                priceToPendingEnterSubOrdersMustBeCanceled = liquidMatchedPrice;
                            }

                            //
                            if (tradeSubOrder.TradeSubStatusType == TradeSubStatusTypes.Entered)
                            {
                                #region tradeSubOrder.TradeSubStatusType == TradeSubStatusTypes.Entered

                                if (minimumDistanceToFromPrice == takeProfitMatchedPriceDistance)
                                {
                                    ApplyTakeProfitTradeTriggerModeOnEnteredTradeSubOrder(tradeSubOrder, strategyTestStatus, candle.MomentaryDateTime, takeProfitMatchedPrice);
                                }
                                else
                                {
                                    float closePrice;
                                    TradeSubOrderActions tradeSubOrderAction;

                                    if (minimumDistanceToFromPrice == stopLossMatchedPriceDistance)
                                    {
                                        closePrice = stopLossMatchedPrice;
                                        tradeSubOrderAction = TradeSubOrderActions.StopLoss;
                                    }
                                    else
                                    {
                                        closePrice = liquidMatchedPrice;
                                        tradeSubOrderAction = TradeSubOrderActions.Liquid;
                                    }

                                    tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Exited;
                                    tradeSubOrder.CloseTime = candle.MomentaryDateTime;
                                    tradeSubOrder.ClosePrice = closePrice;
                                    tradeSubOrder.SetTradeSubOrderModeObjectsToNull();

                                    tradeSubOrder.TradeAllocatedBalance.SetExitPrice(closePrice);
                                    TradeWallet.DeallocateSuccessfulTrade(tradeSubOrder.TradeAllocatedBalance);

                                    AddLogToStrategyTestStatus(tradeSubOrder, tradeSubOrderAction, strategyTestStatus, candle.MomentaryDateTime, closePrice);
                                    AddSubOrderToStrategyTestStatus(tradeSubOrder, tradeSubOrderAction, strategyTestStatus);
                                }

                                #endregion
                            }
                            else
                            {
                                #region tradeSubOrder.TradeSubStatusType == TradeSubStatusTypes.PendingForExit

                                float closePrice;
                                TradeSubOrderActions tradeSubOrderAction;

                                if (minimumDistanceToFromPrice == takeProfitMatchedPriceDistance)
                                {
                                    closePrice = takeProfitMatchedPrice;
                                    tradeSubOrderAction = TradeSubOrderActions.TakeProfit;
                                }
                                else if (minimumDistanceToFromPrice == stopLossMatchedPriceDistance)
                                {
                                    closePrice = stopLossMatchedPrice;
                                    tradeSubOrderAction = TradeSubOrderActions.StopLoss;
                                }
                                else
                                {
                                    closePrice = liquidMatchedPrice;
                                    tradeSubOrderAction = TradeSubOrderActions.Liquid;
                                }

                                tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Exited;
                                tradeSubOrder.CloseTime = candle.MomentaryDateTime;
                                tradeSubOrder.ClosePrice = closePrice;
                                tradeSubOrder.SetTradeSubOrderModeObjectsToNull();

                                tradeSubOrder.TradeAllocatedBalance.SetExitPrice(closePrice);
                                TradeWallet.DeallocateSuccessfulTrade(tradeSubOrder.TradeAllocatedBalance);

                                AddLogToStrategyTestStatus(tradeSubOrder, tradeSubOrderAction, strategyTestStatus, candle.MomentaryDateTime, closePrice);
                                AddSubOrderToStrategyTestStatus(tradeSubOrder, tradeSubOrderAction, strategyTestStatus);

                                #endregion
                            }
                        }
                    }
                    break;
            }
        }

        private void TraverseCandle(CandleDataModel candle, float fromPrice, float toPrice, StrategyTestStatusDataModel strategyTestStatus)
        {
            var tradeSubOrdersList = TradeSubOrders.ToList();

            foreach (var tradeSubOrder in tradeSubOrdersList)
            {
                var mustPendingEnterSubOrdersBeCanceled = false;
                var priceToPendingEnterSubOrdersMustBeCanceled = 0f;

                TraverseCandleForSubOrder(tradeSubOrder, candle, fromPrice, toPrice, ref mustPendingEnterSubOrdersBeCanceled, ref priceToPendingEnterSubOrdersMustBeCanceled, strategyTestStatus);

                if (mustPendingEnterSubOrdersBeCanceled)
                {
                    foreach (var pendingEnterTradeSubOrder in TradeSubOrders.Where(p => p.TradeSubStatusType == TradeSubStatusTypes.PendingForEnter))
                    {
                        if (pendingEnterTradeSubOrder.OrderId == tradeSubOrder.OrderId)
                        {
                            pendingEnterTradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Cancelled;
                            pendingEnterTradeSubOrder.SetTradeSubOrderModeObjectsToNull();

                            TradeWallet.DeallocateFailedTrade(tradeSubOrder.TradeAllocatedBalance);

                            AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus, candle.MomentaryDateTime, priceToPendingEnterSubOrdersMustBeCanceled);
                            AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus);
                        }
                    }
                }
            }
        }

        private void TraverseCandle(CandleDataModel candle, StrategyTestStatusDataModel strategyTestStatus)
        {
            var isMovingUpFirst = true;

            switch (StrategyTestData.StrategyTestPriceMovementFlowMode)
            {
                case StrategyTestPriceMovementFlowModes.Random:
                    {
                        isMovingUpFirst = RandomNumberGenerator.Next() % 2 == 0;
                    }
                    break;
                case StrategyTestPriceMovementFlowModes.Realistic:
                    {
                        isMovingUpFirst = candle.Open > candle.Close;
                    }
                    break;
                case StrategyTestPriceMovementFlowModes.Best:
                    {
                        isMovingUpFirst = StrategyTestData.Position == PositionTypes.Long;
                    }
                    break;
                case StrategyTestPriceMovementFlowModes.Worst:
                    {
                        isMovingUpFirst = StrategyTestData.Position != PositionTypes.Long;
                    }
                    break;
            }

            if (isMovingUpFirst)
            {
                TraverseCandle(candle, candle.Open, candle.High, strategyTestStatus);
                TraverseCandle(candle, candle.High, candle.Low, strategyTestStatus);
                TraverseCandle(candle, candle.Low, candle.Close, strategyTestStatus);
            }
            else
            {
                TraverseCandle(candle, candle.Open, candle.Low, strategyTestStatus);
                TraverseCandle(candle, candle.Low, candle.High, strategyTestStatus);
                TraverseCandle(candle, candle.High, candle.Close, strategyTestStatus);
            }
        }

        private void CheckExitAlarmsToExistSubOrders(CandleDataModel candle, StrategyTestStatusDataModel strategyTestStatus)
        {
            if (StrategyTestData.ExitTakeProfit.TradeSubOrderTriggerMode == TradeSubOrderTriggerModes.Alarm)
            {
                var tradeSubOrdersList = TradeSubOrders.ToList();

                if (tradeSubOrdersList.Count(p => p.TradeSubStatusType == TradeSubStatusTypes.PendingForEnter || p.TradeSubStatusType == TradeSubStatusTypes.Entered) != 0)
                {
                    if (CalculateCondition(TakeProfitAlarmCondition))
                    {
                        foreach (var tradeSubOrder in tradeSubOrdersList)
                        {
                            if (tradeSubOrder.TradeSubStatusType == TradeSubStatusTypes.PendingForEnter)
                            {
                                tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Cancelled;
                                tradeSubOrder.SetTradeSubOrderModeObjectsToNull();

                                TradeWallet.DeallocateFailedTrade(tradeSubOrder.TradeAllocatedBalance);

                                AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                                AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus);
                            }
                            else if (tradeSubOrder.TradeSubStatusType == TradeSubStatusTypes.Entered)
                            {
                                ApplyTakeProfitTradeTriggerModeOnEnteredTradeSubOrder(tradeSubOrder, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                            }
                        }
                    }
                }
            }

            if (StrategyTestData.ExitStopLoss.TradeSubOrderTriggerMode == TradeSubOrderTriggerModes.Alarm)
            {
                var tradeSubOrdersList = TradeSubOrders.ToList();

                if (tradeSubOrdersList.Count(p => p.TradeSubStatusType == TradeSubStatusTypes.PendingForEnter || p.TradeSubStatusType == TradeSubStatusTypes.Entered || p.TradeSubStatusType == TradeSubStatusTypes.PendingForExit) != 0)
                {
                    if (CalculateCondition(StopLossAlarmCondition))
                    {
                        foreach (var tradeSubOrder in tradeSubOrdersList)
                        {
                            switch (tradeSubOrder.TradeSubStatusType)
                            {
                                case TradeSubStatusTypes.PendingForEnter:
                                    {
                                        tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Cancelled;
                                        tradeSubOrder.SetTradeSubOrderModeObjectsToNull();

                                        TradeWallet.DeallocateFailedTrade(tradeSubOrder.TradeAllocatedBalance);

                                        AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                                        AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Cancel, strategyTestStatus);
                                    }
                                    break;
                                case TradeSubStatusTypes.Entered:
                                case TradeSubStatusTypes.PendingForExit:
                                    {
                                        tradeSubOrder.TradeSubStatusType = TradeSubStatusTypes.Exited;
                                        tradeSubOrder.CloseTime = candle.MomentaryDateTime;
                                        tradeSubOrder.ClosePrice = candle.Close;
                                        tradeSubOrder.SetTradeSubOrderModeObjectsToNull();
                                        tradeSubOrder.TradeAllocatedBalance.SetExitPrice(candle.Close);

                                        TradeWallet.DeallocateSuccessfulTrade(tradeSubOrder.TradeAllocatedBalance);

                                        AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.StopLoss, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                                        AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.StopLoss, strategyTestStatus);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void CheckEnterAlarm(CandleDataModel candle, StrategyTestStatusDataModel strategyTestStatus)
        {
            if (TradeWallet.TotalPayableBalance > 0)
            {
                if (CalculateCondition(EnterAlarmCondition))
                {
                    switch (StrategyTestData.Enter.TradeSubOrderMode)
                    {
                        case TradeSubOrderModes.None:
                            {
                                var tradeAllocatedBalance = TradeWallet.AllocateForFixedAmount(StrategyTestData.Position);

                                if (tradeAllocatedBalance != null)
                                {
                                    OrderId++;
                                    SubOrderId++;

                                    tradeAllocatedBalance.SetEnterPrice(candle.Close);

                                    var tradeSubOrder = CreateEnterdTradeSubOrderAuxiliary(candle, tradeAllocatedBalance, OrderId, SubOrderId);

                                    if (tradeSubOrder != null)
                                    {
                                        TradeSubOrders.Add(tradeSubOrder);

                                        AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Enter, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                                        AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Enter, strategyTestStatus);
                                    }
                                    else
                                    {
                                        TradeWallet.DeallocateFailedTrade(tradeAllocatedBalance);
                                    }
                                }
                            }
                            break;
                        case TradeSubOrderModes.TrailingOrder:
                            {
                                var tradeAllocatedBalance = TradeWallet.AllocateForFixedAmount(StrategyTestData.Position);

                                if (tradeAllocatedBalance != null)
                                {
                                    OrderId++;
                                    SubOrderId++;

                                    var tradeSubOrder = new TradeSubOrderAuxiliary()
                                    {
                                        Id = SubOrderId,
                                        OrderId = OrderId,
                                        TradeSubStatusType = TradeSubStatusTypes.PendingForEnter,
                                        OpenTime = DateTime.MinValue,
                                        CloseTime = DateTime.MinValue,
                                        OpenPrice = 0,
                                        ClosePrice = 0,
                                        TradeAllocatedBalance = tradeAllocatedBalance
                                    };

                                    // EnterTradeSubOrderMode
                                    if (StrategyTestData.Position == PositionTypes.Long)
                                    {
                                        tradeSubOrder.EnterTradeSubOrderMode = new TradeSubOrderTrailingOrderModeAuxiliary(candle.Close, TrailingDirectionTypes.Down, StrategyTestData.Enter.TrailingOrder.TolerantPercentForLoss);
                                    }
                                    else
                                    {
                                        tradeSubOrder.EnterTradeSubOrderMode = new TradeSubOrderTrailingOrderModeAuxiliary(candle.Close, TrailingDirectionTypes.Up, StrategyTestData.Enter.TrailingOrder.TolerantPercentForLoss);
                                    }

                                    //
                                    TradeSubOrders.Add(tradeSubOrder);

                                    AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.PendingEnter, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                                    AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.PendingEnter, strategyTestStatus);
                                }
                            }
                            break;
                        case TradeSubOrderModes.GridOrder:
                            {
                                var firstStepAmount = 0f;

                                for (var index = 0; index < StrategyTestData.Enter.GridOrder.StepCount; index++)
                                {
                                    if (index == 0)
                                    {
                                        var tradeAllocatedBalance = TradeWallet.AllocateForGridFirstStepAmount(StrategyTestData.Position, StrategyTestData.Enter.GridOrder.StepCount);

                                        if (tradeAllocatedBalance != null)
                                        {
                                            firstStepAmount = tradeAllocatedBalance.AllocatedQuoteBalance;

                                            OrderId++;
                                            SubOrderId++;

                                            tradeAllocatedBalance.SetEnterPrice(candle.Close);

                                            var tradeSubOrder = CreateEnterdTradeSubOrderAuxiliary(candle, tradeAllocatedBalance, OrderId, SubOrderId);

                                            if (tradeSubOrder != null)
                                            {
                                                TradeSubOrders.Add(tradeSubOrder);

                                                AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Enter, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                                                AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.Enter, strategyTestStatus);
                                            }
                                            else
                                            {
                                                TradeWallet.DeallocateFailedTrade(tradeAllocatedBalance);
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        var tradeAllocatedBalance = TradeWallet.AllocateForGridNextStepAmount(StrategyTestData.Position, firstStepAmount);

                                        if (tradeAllocatedBalance != null)
                                        {
                                            //
                                            SubOrderId++;

                                            var tradeSubOrder = new TradeSubOrderAuxiliary()
                                            {
                                                Id = SubOrderId,
                                                OrderId = OrderId,
                                                TradeSubStatusType = TradeSubStatusTypes.PendingForEnter,
                                                OpenTime = DateTime.MinValue,
                                                CloseTime = DateTime.MinValue,
                                                OpenPrice = 0,
                                                ClosePrice = 0,
                                                TradeAllocatedBalance = tradeAllocatedBalance
                                            };

                                            //
                                            if (StrategyTestData.Position == PositionTypes.Long)
                                            {
                                                tradeSubOrder.EnterTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.Enter, candle.Close * (1f - StrategyTestData.Enter.GridOrder.Percent / 100f / StrategyTestData.Enter.GridOrder.StepCount * index), FixedModeActiveRangeTypes.Up);
                                            }
                                            else
                                            {
                                                tradeSubOrder.EnterTradeSubOrderMode = new TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes.Enter, candle.Close * (1f + StrategyTestData.Enter.GridOrder.Percent / 100f / StrategyTestData.Enter.GridOrder.StepCount * index), FixedModeActiveRangeTypes.Down);
                                            }

                                            //
                                            TradeSubOrders.Add(tradeSubOrder);

                                            AddLogToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.PendingEnter, strategyTestStatus, candle.MomentaryDateTime, candle.Close);
                                            AddSubOrderToStrategyTestStatus(tradeSubOrder, TradeSubOrderActions.PendingEnter, strategyTestStatus);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void ProcessCandle(CandleDataModel candle, StrategyTestStatusDataModel strategyTestStatus)
        {
            TraverseCandle(candle, strategyTestStatus);

            CheckExitAlarmsToExistSubOrders(candle, strategyTestStatus);

            CheckEnterAlarm(candle, strategyTestStatus);
        }

        private void FillStrategyTestReport(StrategyTestReportDataModel strategyTestReport, float currentPrice, StrategyTestStatusTypes strategyTestStatusType)
        {
            var pendingForEnter = TradeSubOrders.Where(p => p.TradeSubStatusType == TradeSubStatusTypes.PendingForEnter).Select(p => p.TradeAllocatedBalance);
            var others = TradeSubOrders.Where(p => p.TradeSubStatusType == TradeSubStatusTypes.Entered || p.TradeSubStatusType == TradeSubStatusTypes.PendingForExit).Select(p => p.TradeAllocatedBalance);

            float totalSavedProfit;
            float totalPaiedMarketFee;
            float totalWalletBalance;
            int wonTradeCount;
            float wonTradePercent;

            TradeWallet.EstimateFinalWalletByTradeAllocatedBalances(pendingForEnter, others, currentPrice, out totalSavedProfit, out totalPaiedMarketFee, out totalWalletBalance, out wonTradeCount, out wonTradePercent);

            strategyTestReport.OrderCount = OrderId;
            strategyTestReport.SubOrderCount = SubOrderId;
            strategyTestReport.ActionCount = ActionId;
            strategyTestReport.LastStrategyTestStatus = strategyTestStatusType;
            strategyTestReport.WonSubOrderCount = wonTradeCount;
            strategyTestReport.WonSubOrderPercent = wonTradePercent;
            strategyTestReport.InitialDeposit = StrategyTestData.InitialBaseCoinDeposit;
            strategyTestReport.TotalFee = totalPaiedMarketFee;
            strategyTestReport.TotalProfit = totalWalletBalance - strategyTestReport.InitialDeposit;
            strategyTestReport.TotalProfitPercent = strategyTestReport.TotalProfit / strategyTestReport.InitialDeposit * 100f;
            strategyTestReport.TotalSavedProfit = TradeWallet.TotalSavedProfit;
            strategyTestReport.TotalBalance = totalWalletBalance;
        }

        public bool Init()
        {
            var result = true;

            var message = StrategyTestData.Validation();

            if (string.IsNullOrWhiteSpace(message))
            {
                //
                var conditions = new List<ICondition>();

                if (!string.IsNullOrWhiteSpace(StrategyTestData.Enter.Alarm))
                {
                    var error = false;

                    try
                    {
                        var temp1 = "";
                        var temp2 = SymbolTypes.BtcUsdt;
                        var temp3 = PositionTypes.Long;

                        var alarmItem = AlarmHelper.ConvertStringToAlarmItem(File.ReadAllText(StrategyTestData.Enter.Alarm), ref temp1, ref temp2, ref temp3);

                        if (alarmItem == null)
                        {
                            error = true;
                        }
                        else
                        {
                            var condition = AlarmHelper.CreateConditionFromAlarmItemDataModel(alarmItem, true, OnOperationCandleRequested);

                            if (condition == null)
                            {
                                error = true;
                            }
                            else
                            {
                                EnterAlarmCondition = condition;

                                conditions.Add(condition);
                            }
                        }
                    }
                    catch
                    {
                        error = true;
                    }

                    if (error)
                    {
                        result = false;

                        message = "Parsing the enter alarm was failed.";
                    }
                }

                if (result && !string.IsNullOrWhiteSpace(StrategyTestData.ExitTakeProfit.Alarm))
                {
                    var error = false;

                    try
                    {
                        var temp1 = "";
                        var temp2 = SymbolTypes.BtcUsdt;
                        var temp3 = PositionTypes.Long;

                        var alarmItem = AlarmHelper.ConvertStringToAlarmItem(File.ReadAllText(StrategyTestData.ExitTakeProfit.Alarm), ref temp1, ref temp2, ref temp3);

                        if (alarmItem == null)
                        {
                            error = true;
                        }
                        else
                        {
                            var condition = AlarmHelper.CreateConditionFromAlarmItemDataModel(alarmItem, true, OnOperationCandleRequested);

                            if (condition == null)
                            {
                                error = true;
                            }
                            else
                            {
                                TakeProfitAlarmCondition = condition;

                                conditions.Add(condition);
                            }
                        }
                    }
                    catch
                    {
                        error = true;
                    }

                    if (error)
                    {
                        result = false;

                        message = "Parsing the take profit alarm was failed.";
                    }
                }

                if (result && !string.IsNullOrWhiteSpace(StrategyTestData.ExitStopLoss.Alarm))
                {
                    var error = false;

                    try
                    {
                        var temp1 = "";
                        var temp2 = SymbolTypes.BtcUsdt;
                        var temp3 = PositionTypes.Long;

                        var alarmItem = AlarmHelper.ConvertStringToAlarmItem(File.ReadAllText(StrategyTestData.ExitStopLoss.Alarm), ref temp1, ref temp2, ref temp3);

                        if (alarmItem == null)
                        {
                            error = true;
                        }
                        else
                        {
                            var condition = AlarmHelper.CreateConditionFromAlarmItemDataModel(alarmItem, true, OnOperationCandleRequested);

                            if (condition == null)
                            {
                                error = true;
                            }
                            else
                            {
                                StopLossAlarmCondition = condition;

                                conditions.Add(condition);
                            }
                        }
                    }
                    catch
                    {
                        error = true;
                    }

                    if (error)
                    {
                        result = false;

                        message = "Parsing the stop loss alarm was failed.";
                    }
                }

                //
                if (result)
                {
                    result = false;

                    var neededSymbols = GetNeededSymbols(conditions);

                    if (neededSymbols.Count != 0)
                    {
                        DatabaseSequenceCandleReaders = CreateDatabaseSequenceCandleReaders(neededSymbols);

                        if (DatabaseSequenceCandleReaders != null)
                        {
                            SymbolTimeFrameCandles = new Dictionary<SymbolTypes, Dictionary<TimeFrames, IList<CandleDataModel>>>();

                            foreach (var symbol in neededSymbols)
                            {
                                var timeFrameCandles = new Dictionary<TimeFrames, IList<CandleDataModel>>();

                                foreach (var timeFrame in (TimeFrames[])Enum.GetValues(typeof(TimeFrames)))
                                {
                                    timeFrameCandles.Add(timeFrame, new List<CandleDataModel>());
                                }

                                SymbolTimeFrameCandles.Add(symbol, timeFrameCandles);
                            }

                            result = true;
                        }
                        else
                        {
                            message = "Database sequence readers collection were empty.";
                        }
                    }
                    else
                    {
                        message = "Needed symbols collection were empty.";
                    }
                }
            }
            else
            {
                result = false;
            }

            if (result)
            {
                TradeWallet = new TradeWalletAuxiliary();

                TradeWallet.Init(StrategyTestData);
            }
            else
            {
                DisposeDatabaseSequenceCandleReaders(DatabaseSequenceCandleReaders);

                WasStopped = true;

                var errorStrategyTestStatus = new StrategyTestStatusDataModel
                {
                    Id = StrategyTestData.Id,
                    StrategyTestStatusType = StrategyTestStatusTypes.Error,
                    Progress = 0f,
                    Message = message
                };

                StrategyTestStatusChanged?.Invoke(SessionId, errorStrategyTestStatus);

                SaveDataToTestDirectory(errorStrategyTestStatus);
            }

            return result;
        }

        public void Start()
        {
            if (!WasStopped)
            {
                DateTime? candlesDateTime;

                var initializedCandles = ReadInitializedSyncedSymbolTimeFrameCandle(out candlesDateTime);

                if (initializedCandles != null && candlesDateTime.HasValue)
                {
                    //
                    var minuteCandleIndex = 0;
                    var totalMinuteCandles = (StrategyTestData.ToDateTime - candlesDateTime.Value).TotalMinutes;

                    //
                    var result = true;

                    var readCandles = initializedCandles;

                    //
                    var errorMessage = "";

                    var updateStrategyTestStatus = new StrategyTestStatusDataModel
                    {
                        Id = StrategyTestData.Id,
                        StrategyTestStatusType = StrategyTestStatusTypes.Update
                    };

                    var updateProgressStrategyTestStatus = new StrategyTestStatusDataModel
                    {
                        Id = StrategyTestData.Id,
                        StrategyTestStatusType = StrategyTestStatusTypes.Update,
                        StrategyTestLogs = null,
                        StrategyTestOrders = null,
                        StrategyTestReport = null
                    };

                    //
                    CandleDataModel lastMainCandle = null;

                    var delayCounter = 0;

                    while (result)
                    {
                        minuteCandleIndex++;

                        lastMainCandle = readCandles[StrategyTestData.Symbol][TimeFrames.Minute1];

                        ApplyCandle(readCandles);

                        ProcessCandle(lastMainCandle, updateStrategyTestStatus);

                        if (WasStopped)
                        {
                            result = false;
                        }
                        else
                        {
                            //
                            if (StrategyTestData.VisualMode)
                            {
                                if (TimeFrameHelper.IsThisMinuteCandleLastTimeFrameCandle(lastMainCandle.MomentaryDateTime, StrategyTestData.VisualTickFrame))
                                {
                                    //
                                    if (lastMainCandle.MomentaryDateTime >= StrategyTestData.VisualSkipToDateTime)
                                    {
                                        updateStrategyTestStatus.Candles.Add(OnOperationCandleRequested(StrategyTestData.Symbol, StrategyTestData.VisualTimeFrame, 0));
                                    }

                                    FillStrategyTestReport(updateStrategyTestStatus.StrategyTestReport, lastMainCandle.Close, StrategyTestStatusTypes.Update);

                                    updateStrategyTestStatus.TotalBalance = updateStrategyTestStatus.StrategyTestReport.TotalBalance;
                                    updateStrategyTestStatus.Progress = ((float)minuteCandleIndex) / ((float)totalMinuteCandles) * 100f;

                                    //
                                    StrategyTestStatusChanged?.Invoke(SessionId, updateStrategyTestStatus);

                                    //
                                    updateStrategyTestStatus.Candles.Clear();
                                    updateStrategyTestStatus.StrategyTestLogs.Clear();
                                    updateStrategyTestStatus.StrategyTestOrders.Clear();
                                    updateStrategyTestStatus.StrategyTestReport.SetDefault();

                                    //
                                    delayCounter++;

                                    if (delayCounter > 10)
                                    {
                                        var maximumDelay = 5000f;

                                        int miliseconds;

                                        if (StrategyTestData.VisualTickPerSecond == 0)
                                        {
                                            miliseconds = (int)maximumDelay;
                                        }
                                        else if (StrategyTestData.VisualTickPerSecond == 100)
                                        {
                                            miliseconds = 0;
                                        }
                                        else
                                        {
                                            var weight = (100f - (float)(StrategyTestData.VisualTickPerSecond)) / 100f;

                                            miliseconds = (int)(weight * maximumDelay);
                                        }

                                        Thread.Sleep((1000 + miliseconds) / 25);

                                        delayCounter = 0;
                                    }
                                }
                            }
                            else
                            {
                                var progress = ((float)minuteCandleIndex) / ((float)totalMinuteCandles) * 100f;

                                if (updateProgressStrategyTestStatus.Progress + 0.1f < progress)
                                {
                                    updateProgressStrategyTestStatus.Progress = progress;

                                    StrategyTestStatusChanged?.Invoke(SessionId, updateProgressStrategyTestStatus);
                                }
                            }

                            //
                            if (lastMainCandle.MomentaryDateTime >= StrategyTestData.ToDateTime)
                            {
                                break;
                            }

                            //
                            int readCandlesCount;

                            readCandles = NextTimeFrameCandle(out readCandlesCount);

                            if (readCandles == null)
                            {
                                result = false;

                                errorMessage = "Error in reading candles from database.";
                            }
                            else
                            {
                                if (readCandlesCount == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    //
                    if (lastMainCandle != null)
                    {
                        FillStrategyTestReport(updateStrategyTestStatus.StrategyTestReport, lastMainCandle.Close, StrategyTestStatusTypes.Update);

                        updateStrategyTestStatus.TotalBalance = updateStrategyTestStatus.StrategyTestReport.TotalBalance;
                    }

                    updateStrategyTestStatus.Progress = 0;

                    if (result)
                    {
                        updateStrategyTestStatus.StrategyTestStatusType = StrategyTestStatusTypes.Finish;
                    }
                    else
                    {
                        updateStrategyTestStatus.StrategyTestStatusType = StrategyTestStatusTypes.Error;
                        updateStrategyTestStatus.Message = errorMessage;
                    }

                    if (updateStrategyTestStatus.StrategyTestReport != null)
                    {
                        updateStrategyTestStatus.StrategyTestReport.LastStrategyTestStatus = updateStrategyTestStatus.StrategyTestStatusType;
                    }

                    StrategyTestStatusChanged?.Invoke(SessionId, updateStrategyTestStatus);

                    SaveDataToTestDirectory(updateStrategyTestStatus);
                }
                else
                {
                    var errorStrategyTestStatus = new StrategyTestStatusDataModel
                    {
                        Id = StrategyTestData.Id,
                        StrategyTestStatusType = StrategyTestStatusTypes.Error,
                        Progress = 0f,
                        Message = "Retrieve data for strategy test was failed."
                    };

                    StrategyTestStatusChanged?.Invoke(SessionId, errorStrategyTestStatus);

                    SaveDataToTestDirectory(errorStrategyTestStatus);
                }
            }

            DisposeDatabaseSequenceCandleReaders(DatabaseSequenceCandleReaders);
        }

        public void Stop()
        {
            WasStopped = true;
        }

        public event StrategyTestStatusChangedHandler StrategyTestStatusChanged;
    }
}

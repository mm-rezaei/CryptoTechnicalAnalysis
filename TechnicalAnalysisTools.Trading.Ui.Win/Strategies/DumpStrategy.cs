using Binance.Net.Enums;
using Binance.Net.Objects.Spot.MarketStream;
using System;
using System.Collections.Generic;
using System.Linq;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Trading.Ui.Win.Auxiliaries;
using TechnicalAnalysisTools.Trading.Ui.Win.Enumerations;
using TechnicalAnalysisTools.Trading.Ui.Win.Services;
using ReflectionHelper = TechnicalAnalysisTools.Trading.Ui.Win.Helpers.ReflectionHelper;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Strategies
{
    internal class DumpStrategy : StrategyBase
    {
        public DumpStrategy(BinanceSpotClientService binanceSpotClient, SymbolTypes symbol, decimal usdtAmount, int minutesCount, decimal dumpPercent, decimal enterTrailingPercent, decimal takeProfitPercent, decimal takeProfitTrailingPercent, decimal stopLossPercent, int roundDigitCount) : base(binanceSpotClient, symbol, usdtAmount)
        {
            MinutesCount = minutesCount;
            DumpPercent = dumpPercent;
            EnterTrailingPercent = enterTrailingPercent;
            TakeProfitPercent = takeProfitPercent;
            TakeProfitTrailingPercent = takeProfitTrailingPercent;
            StopLossPercent = stopLossPercent;
            RoundDigitCount = roundDigitCount;
            WasBought = false;
        }

        private int MinutesCount { get; }

        private decimal DumpPercent { get; }

        private decimal EnterTrailingPercent { get; }

        private decimal TakeProfitPercent { get; }

        private decimal TakeProfitTrailingPercent { get; }

        private decimal StopLossPercent { get; }

        private int RoundDigitCount { get; }

        private bool WasBought { get; set; }

        private List<BinanceStreamTick> Candles { get; set; } = new List<BinanceStreamTick>();

        private TradeSubOrderTrailingOrderModeAuxiliary EnterTradeSubOrderTrailingOrderMode { get; set; }

        private TradeSubOrderTrailingOrderModeAuxiliary ExitTradeSubOrderTrailingOrderMode { get; set; }

        private decimal EnterPrice { get; set; }

        private decimal EnterAmount { get; set; }

        private DateTime ValidTimeForEnter { get; set; } = DateTime.MinValue;

        private void LogData(DateTime datetime, string action, decimal price, decimal amount, string message)
        {
            var log = string.Format("{0}, {1}, {2}, Price={3}", datetime.ToString("yyyy/MM/dd HH:mm:ss"), Symbol, action, price);

            if (amount != 0)
            {
                log += $", Amount={amount}";
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                log += "," + message;
            }

            OnLogReceived(log);
        }

        private decimal RoundDecimal(decimal number)
        {
            var multiNumber = 1m;

            for (var index = 0; index < RoundDigitCount; index++)
            {
                multiNumber *= 10m;
            }

            var result = (int)(number * multiNumber);

            return result / multiNumber;
        }

        private void Reset()
        {
            WasBought = false;

            EnterPrice = 0;
            EnterAmount = 0;

            EnterTradeSubOrderTrailingOrderMode = null;
            ExitTradeSubOrderTrailingOrderMode = null;
        }

        private void AddCandle(BinanceStreamTick candle)
        {
            var lastCandle = Candles.LastOrDefault();

            if (lastCandle == null)
            {
                Candles.Add(candle);
            }
            else
            {
                if (candle.OpenTime == lastCandle.OpenTime)
                {
                    ReflectionHelper.CopyValuableProperties(candle, lastCandle);
                }
                else
                {
                    if (candle.OpenTime == lastCandle.OpenTime.AddMinutes(1))
                    {
                        Candles.Add(candle);
                    }
                    else
                    {
                        for (var index = lastCandle.OpenTime.AddMinutes(1); index < candle.OpenTime; index = index.AddMinutes(1))
                        {
                            Candles.Add(null);
                        }

                        Candles.Add(candle);
                    }
                }
            }

            if (Candles.Count > MinutesCount + 1)
            {
                Candles = Candles.Skip(Candles.Count - MinutesCount - 1).ToList();
            }
        }

        public override void CheckPrice(BinanceStreamTick candle)
        {
            candle.OpenTime = new DateTime(candle.CloseTime.Year, candle.CloseTime.Month, candle.CloseTime.Day, candle.CloseTime.Hour, candle.CloseTime.Minute, 0);

            AddCandle(candle);

            if (candle.CloseTime.Second % 3 == 0)
            {
                if (WasBought == false)
                {
                    if (EnterTradeSubOrderTrailingOrderMode == null)
                    {
                        if (Candles.Count >= MinutesCount + 1 && candle.CloseTime > ValidTimeForEnter)
                        {
                            var canEnter = false;

                            for (var index = 0; index < MinutesCount; index++)
                            {
                                var oldCandle = Candles[Candles.Count - index - 2];

                                if (oldCandle != null)
                                {
                                    var expectedValue = oldCandle.LastPrice * (1m - (DumpPercent / 100m));

                                    if (candle.LastPrice <= expectedValue)
                                    {
                                        canEnter = true;

                                        break;
                                    }
                                }
                            }

                            if (canEnter)
                            {
                                EnterTradeSubOrderTrailingOrderMode = new TradeSubOrderTrailingOrderModeAuxiliary(candle.LastPrice, TrailingDirectionTypes.Down, EnterTrailingPercent);

                                LogData(candle.CloseTime, "Enable Enter Trailing", candle.LastPrice, 0, "");
                            }
                        }
                    }
                    else
                    {
                        if (EnterTradeSubOrderTrailingOrderMode.CheckPrice(candle.LastPrice))
                        {
                            EnterTradeSubOrderTrailingOrderMode = null;

                            string errorMessage;

                            LogData(candle.CloseTime, "Enter Buy", candle.LastPrice, 0, "");

                            var binancePlacedOrder = BinanceSpotClient.OpenMarketOrder(Symbol, RoundDecimal(UsdtAmount / candle.LastPrice), out errorMessage);

                            if (binancePlacedOrder != null && binancePlacedOrder.Status == OrderStatus.Filled)
                            {
                                WasBought = true;

                                EnterPrice = binancePlacedOrder.AverageFillPrice.Value;
                                EnterAmount = RoundDecimal(binancePlacedOrder.QuantityFilled);

                                LogData(binancePlacedOrder.CreateTime, "Successful Buy", EnterPrice, EnterAmount, "");
                            }
                            else
                            {
                                LogData(candle.CloseTime, "Failed Buy", candle.LastPrice, RoundDecimal(UsdtAmount / candle.LastPrice), errorMessage);

                                Reset();
                            }
                        }
                    }
                }
                else
                {
                    if (ExitTradeSubOrderTrailingOrderMode == null)
                    {
                        var expectedTakeProfitPrice = EnterPrice * (1m + (TakeProfitPercent / 100m));
                        var expectedStopLossPrice = EnterPrice * (1m - (StopLossPercent / 100m));

                        if (candle.LastPrice <= expectedStopLossPrice)
                        {
                            string errorMessage;

                            LogData(candle.CloseTime, "Enter Sell", candle.LastPrice, 0, "");

                            var binancePlacedOrder = BinanceSpotClient.CloseMarketOrder(Symbol, EnterAmount, out errorMessage);

                            for (var index = 0; index < 10; index++)
                            {
                                if (binancePlacedOrder != null)
                                {
                                    break;
                                }

                                binancePlacedOrder = BinanceSpotClient.CloseMarketOrder(Symbol, EnterAmount, out errorMessage);
                            }

                            if (binancePlacedOrder != null && binancePlacedOrder.Status == OrderStatus.Filled)
                            {
                                LogData(binancePlacedOrder.CreateTime, "StopLoss", binancePlacedOrder.AverageFillPrice.Value, RoundDecimal(binancePlacedOrder.QuantityFilled), null);

                                var profitPercent = Math.Round(((binancePlacedOrder.AverageFillPrice.Value / EnterPrice) - 1m) * 100m, 2);

                                LogData(binancePlacedOrder.CreateTime, "Profit Percent%", profitPercent, 0, null);

                                ValidTimeForEnter = binancePlacedOrder.CreateTime.AddSeconds(20);

                                Reset();
                            }
                            else
                            {
                                LogData(candle.CloseTime, "Failed StopLoss", candle.LastPrice, EnterAmount, errorMessage);
                            }
                        }
                        else if (candle.LastPrice >= expectedTakeProfitPrice)
                        {
                            ExitTradeSubOrderTrailingOrderMode = new TradeSubOrderTrailingOrderModeAuxiliary(candle.LastPrice, TrailingDirectionTypes.Up, TakeProfitTrailingPercent);

                            LogData(candle.CloseTime, "Enable TakeProfit Trailing", candle.LastPrice, 0, "");
                        }
                    }
                    else
                    {
                        if (ExitTradeSubOrderTrailingOrderMode.CheckPrice(candle.LastPrice))
                        {
                            string errorMessage;

                            LogData(candle.CloseTime, "Enter Sell", candle.LastPrice, 0, "");

                            var binancePlacedOrder = BinanceSpotClient.CloseMarketOrder(Symbol, EnterAmount, out errorMessage);

                            for (var index = 0; index < 10; index++)
                            {
                                if (binancePlacedOrder != null)
                                {
                                    break;
                                }

                                binancePlacedOrder = BinanceSpotClient.CloseMarketOrder(Symbol, EnterAmount, out errorMessage);
                            }

                            if (binancePlacedOrder != null && binancePlacedOrder.Status == OrderStatus.Filled)
                            {
                                LogData(binancePlacedOrder.CreateTime, "Successful TakeProfit", binancePlacedOrder.AverageFillPrice.Value, RoundDecimal(binancePlacedOrder.QuantityFilled), "");

                                var profitPercent = Math.Round(((binancePlacedOrder.AverageFillPrice.Value / EnterPrice) - 1m) * 100m, 2);

                                LogData(binancePlacedOrder.CreateTime, "Profit Percent%", profitPercent, 0, null);

                                ValidTimeForEnter = binancePlacedOrder.CreateTime.AddSeconds(20);

                                Candles.RemoveAt(0);

                                Reset();
                            }
                            else
                            {
                                LogData(candle.CloseTime, "Failed TakeProfit", candle.LastPrice, EnterAmount, errorMessage);
                            }
                        }
                    }
                }
            }
        }
    }
}

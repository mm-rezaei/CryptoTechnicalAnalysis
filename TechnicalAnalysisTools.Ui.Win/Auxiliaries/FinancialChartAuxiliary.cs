using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Xaml.Charting;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Ui.Win.Auxiliaries
{
    internal class FinancialChartAuxiliary
    {
        public FinancialChartAuxiliary(SymbolTypes symbol, Chart chartControl, Shared.Enumerations.IndicatorType[] indicatorTypes)
        {
            Symbol = symbol;

            ChartControl = chartControl;

            IndicatorTypes = indicatorTypes;
        }

        private SymbolTypes Symbol { get; set; }

        private Chart ChartControl { get; set; }

        private Shared.Enumerations.IndicatorType[] IndicatorTypes { get; set; }

        private ChartArea CandlesArea { get; set; }

        private ChartCandleElement CandleElement { get; set; }

        private ChartArea TradeCountArea { get; set; }

        private ChartIndicatorElement BuyCountElement { get; set; }

        private ChartIndicatorElement SellCountElement { get; set; }

        private FakeIndicatorAuxiliary BuyCountFakeIndicator { get; set; } = new FakeIndicatorAuxiliary();

        private FakeIndicatorAuxiliary SellCountFakeIndicator { get; set; } = new FakeIndicatorAuxiliary();

        private Dictionary<FieldInfo, List<IndicatorValueElement>> IndicatorValueElements { get; set; } = new Dictionary<FieldInfo, List<IndicatorValueElement>>();

        private class IndicatorValueElement
        {
            public Shared.Enumerations.IndicatorType IndicatorType { get; set; }

            public ChartArea IndicatorArea { get; set; }

            public ChartIndicatorElement IndicatorElement { get; set; }

            public FakeIndicatorAuxiliary Indicator { get; set; }
        }

        public void InitChart()
        {
            //
            ChartControl.ClearAreas();

            //
            CandlesArea = new ChartArea();

            ChartControl.Areas.Add(CandlesArea);

            CandleElement = new ChartCandleElement() { FullTitle = Symbol.ToString(), StrokeThickness = 1 };

            CandlesArea.Elements.Add(CandleElement);

            //
            TradeCountArea = new ChartArea() { Title = "Positions" };

            ChartControl.Areas.Add(TradeCountArea);

            BuyCountElement = new ChartIndicatorElement() { Color = Colors.Green, FullTitle = "Buy", DrawStyle = ChartIndicatorDrawStyles.Histogram };
            SellCountElement = new ChartIndicatorElement() { Color = Colors.Red, FullTitle = "Sell", DrawStyle = ChartIndicatorDrawStyles.Histogram };

            TradeCountArea.Elements.Add(BuyCountElement);
            TradeCountArea.Elements.Add(SellCountElement);

            //
            ChartArea smaChartArea = null;
            ChartArea emaChartArea = null;

            foreach (var indicator in IndicatorTypes)
            {
                switch (indicator)
                {
                    case Shared.Enumerations.IndicatorType.BollingerBands:
                        {
                            var bollingerBandsBasisField = typeof(CandleDataModel).GetField("BollingerBandsBasis");
                            var bollingerUpperField = typeof(CandleDataModel).GetField("BollingerUpper");
                            var bollingerLowerField = typeof(CandleDataModel).GetField("BollingerLower");

                            var bollingerBandsBasisChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Black, FullTitle = "BB BandsBasis" };
                            var bollingerUpperChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Black, FullTitle = "BB Upper" };
                            var bollingerLowerChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Black, FullTitle = "BB Lower" };

                            IndicatorValueElements.Add(bollingerBandsBasisField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = bollingerBandsBasisChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(bollingerUpperField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = bollingerUpperChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(bollingerLowerField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = bollingerLowerChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });

                            CandlesArea.Elements.Add(bollingerBandsBasisChartIndicatorElement);
                            CandlesArea.Elements.Add(bollingerUpperChartIndicatorElement);
                            CandlesArea.Elements.Add(bollingerLowerChartIndicatorElement);
                        }
                        break;
                    case Shared.Enumerations.IndicatorType.Ichimoku:
                        {
                            var ichimokuTenkanSenField = typeof(CandleDataModel).GetField("IchimokuTenkanSen");
                            var ichimokuKijunSenField = typeof(CandleDataModel).GetField("IchimokuKijunSen");
                            var ichimokuSenkouSpanAField = typeof(CandleDataModel).GetField("IchimokuSenkouSpanA");
                            var ichimokuSenkouSpanBField = typeof(CandleDataModel).GetField("IchimokuSenkouSpanB");

                            var ichimokuTenkanSenChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Blue, FullTitle = "Tenkan" };
                            var ichimokuKijunSenChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.DarkRed, FullTitle = "Kijun" };
                            var ichimokuSenkouSpanAChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Green, FullTitle = "SpanA" };
                            var ichimokuSenkouSpanBChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Red, FullTitle = "SpanB" };

                            IndicatorValueElements.Add(ichimokuTenkanSenField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = ichimokuTenkanSenChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(ichimokuKijunSenField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = ichimokuKijunSenChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(ichimokuSenkouSpanAField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = ichimokuSenkouSpanAChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(ichimokuSenkouSpanBField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = ichimokuSenkouSpanBChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });

                            CandlesArea.Elements.Add(ichimokuTenkanSenChartIndicatorElement);
                            CandlesArea.Elements.Add(ichimokuKijunSenChartIndicatorElement);
                            CandlesArea.Elements.Add(ichimokuSenkouSpanAChartIndicatorElement);
                            CandlesArea.Elements.Add(ichimokuSenkouSpanBChartIndicatorElement);
                        }
                        break;
                    case Shared.Enumerations.IndicatorType.Macd:
                        {
                            var macdHistogramField = typeof(CandleDataModel).GetField("MacdHistogram");
                            var macdValueField = typeof(CandleDataModel).GetField("MacdValue");
                            var macdSignalField = typeof(CandleDataModel).GetField("MacdSignal");

                            var macdHistogramChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Green, FullTitle = "Histogram", DrawStyle = ChartIndicatorDrawStyles.Histogram };
                            var macdValueChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Blue, FullTitle = "Value" };
                            var macdSignalChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Red, FullTitle = "Signal" };

                            var macdArea = new ChartArea() { Title = "Macd" };

                            IndicatorValueElements.Add(macdHistogramField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = macdArea, IndicatorElement = macdHistogramChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(macdValueField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = macdArea, IndicatorElement = macdValueChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(macdSignalField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = macdArea, IndicatorElement = macdSignalChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });

                            ChartControl.Areas.Add(macdArea);

                            macdArea.Elements.Add(macdHistogramChartIndicatorElement);
                            macdArea.Elements.Add(macdValueChartIndicatorElement);
                            macdArea.Elements.Add(macdSignalChartIndicatorElement);
                        }
                        break;
                    case Shared.Enumerations.IndicatorType.Rsi:
                        {
                            var rsiField = typeof(CandleDataModel).GetField("RsiValue");

                            var rsiChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.DarkRed, FullTitle = "Rsi" };

                            var rsiArea = new ChartArea() { Title = "Rsi" };

                            IndicatorValueElements.Add(rsiField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = rsiArea, IndicatorElement = rsiChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });

                            ChartControl.Areas.Add(rsiArea);

                            rsiArea.Elements.Add(rsiChartIndicatorElement);
                        }
                        break;
                    case Shared.Enumerations.IndicatorType.Stoch:
                        {
                            var stochKField = typeof(CandleDataModel).GetField("StochKValue");
                            var stochDField = typeof(CandleDataModel).GetField("StochDValue");

                            var stochKChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Blue, FullTitle = "K" };
                            var stochDChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Red, FullTitle = "D" };

                            var stochArea = new ChartArea() { Title = "Stoch" };

                            IndicatorValueElements.Add(stochKField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = stochArea, IndicatorElement = stochKChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(stochDField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = stochArea, IndicatorElement = stochDChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });

                            ChartControl.Areas.Add(stochArea);

                            stochArea.Elements.Add(stochKChartIndicatorElement);
                            stochArea.Elements.Add(stochDChartIndicatorElement);
                        }
                        break;
                    case Shared.Enumerations.IndicatorType.StochRsi:
                        {
                            var stochRsiKField = typeof(CandleDataModel).GetField("StochRsiKValue");
                            var stochRsiDField = typeof(CandleDataModel).GetField("StochRsiDValue");

                            var stochRsiKChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Blue, FullTitle = "K" };
                            var stochRsiDChartIndicatorElement = new ChartIndicatorElement() { Color = Colors.Red, FullTitle = "D" };

                            var stochRsiArea = new ChartArea() { Title = "StochRsi" };

                            IndicatorValueElements.Add(stochRsiKField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = stochRsiArea, IndicatorElement = stochRsiKChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });
                            IndicatorValueElements.Add(stochRsiDField, new List<IndicatorValueElement>() { new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = stochRsiArea, IndicatorElement = stochRsiDChartIndicatorElement, Indicator = new FakeIndicatorAuxiliary() } });

                            ChartControl.Areas.Add(stochRsiArea);

                            stochRsiArea.Elements.Add(stochRsiKChartIndicatorElement);
                            stochRsiArea.Elements.Add(stochRsiDChartIndicatorElement);
                        }
                        break;
                    case Shared.Enumerations.IndicatorType.Ema9:
                    case Shared.Enumerations.IndicatorType.Ema20:
                    case Shared.Enumerations.IndicatorType.Ema26:
                    case Shared.Enumerations.IndicatorType.Ema30:
                    case Shared.Enumerations.IndicatorType.Ema40:
                    case Shared.Enumerations.IndicatorType.Ema50:
                    case Shared.Enumerations.IndicatorType.Ema100:
                    case Shared.Enumerations.IndicatorType.Ema200:
                        {
                            var emaField = typeof(CandleDataModel).GetField(indicator + "Value");

                            var color = Colors.Green;

                            switch (indicator)
                            {
                                case Shared.Enumerations.IndicatorType.Ema9:
                                    color = Colors.Blue;
                                    break;
                                case Shared.Enumerations.IndicatorType.Ema20:
                                    color = Colors.Brown;
                                    break;
                                case Shared.Enumerations.IndicatorType.Ema26:
                                    color = Colors.Black;
                                    break;
                                case Shared.Enumerations.IndicatorType.Ema30:
                                    color = Colors.Gold;
                                    break;
                                case Shared.Enumerations.IndicatorType.Ema40:
                                    color = Colors.Pink;
                                    break;
                                case Shared.Enumerations.IndicatorType.Ema50:
                                    color = Colors.Maroon;
                                    break;
                                case Shared.Enumerations.IndicatorType.Ema100:
                                    color = Colors.SteelBlue;
                                    break;
                                case Shared.Enumerations.IndicatorType.Ema200:
                                    color = Colors.Orange;
                                    break;
                            }

                            var chartIndicatorElement1 = new ChartIndicatorElement() { Color = color, FullTitle = indicator.ToString() };
                            var chartIndicatorElement2 = new ChartIndicatorElement() { Color = color, FullTitle = indicator.ToString() };

                            if (emaChartArea == null)
                            {
                                emaChartArea = new ChartArea() { Title = "Ema" };

                                ChartControl.Areas.Add(emaChartArea);
                            }

                            IndicatorValueElements.Add(emaField, new List<IndicatorValueElement>()
                            {
                                new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = emaChartArea, IndicatorElement = chartIndicatorElement1, Indicator = new FakeIndicatorAuxiliary() },
                                new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = chartIndicatorElement2, Indicator = new FakeIndicatorAuxiliary() }
                            });

                            emaChartArea.Elements.Add(chartIndicatorElement1);
                            CandlesArea.Elements.Add(chartIndicatorElement2);
                        }
                        break;
                    case Shared.Enumerations.IndicatorType.Sma9:
                    case Shared.Enumerations.IndicatorType.Sma20:
                    case Shared.Enumerations.IndicatorType.Sma26:
                    case Shared.Enumerations.IndicatorType.Sma30:
                    case Shared.Enumerations.IndicatorType.Sma40:
                    case Shared.Enumerations.IndicatorType.Sma50:
                    case Shared.Enumerations.IndicatorType.Sma100:
                    case Shared.Enumerations.IndicatorType.Sma200:
                        {
                            var smaField = typeof(CandleDataModel).GetField(indicator + "Value");

                            var color = Colors.Green;

                            switch (indicator)
                            {
                                case Shared.Enumerations.IndicatorType.Sma9:
                                    color = Colors.Blue;
                                    break;
                                case Shared.Enumerations.IndicatorType.Sma20:
                                    color = Colors.Brown;
                                    break;
                                case Shared.Enumerations.IndicatorType.Sma26:
                                    color = Colors.Black;
                                    break;
                                case Shared.Enumerations.IndicatorType.Sma30:
                                    color = Colors.Gold;
                                    break;
                                case Shared.Enumerations.IndicatorType.Sma40:
                                    color = Colors.Pink;
                                    break;
                                case Shared.Enumerations.IndicatorType.Sma50:
                                    color = Colors.Maroon;
                                    break;
                                case Shared.Enumerations.IndicatorType.Sma100:
                                    color = Colors.SteelBlue;
                                    break;
                                case Shared.Enumerations.IndicatorType.Sma200:
                                    color = Colors.Orange;
                                    break;
                            }

                            var chartIndicatorElement1 = new ChartIndicatorElement() { Color = color, FullTitle = indicator.ToString() };
                            var chartIndicatorElement2 = new ChartIndicatorElement() { Color = color, FullTitle = indicator.ToString() };

                            if (smaChartArea == null)
                            {
                                smaChartArea = new ChartArea() { Title = "Sma" };

                                ChartControl.Areas.Add(smaChartArea);
                            }

                            IndicatorValueElements.Add(smaField, new List<IndicatorValueElement>()
                            {
                                new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = smaChartArea, IndicatorElement = chartIndicatorElement1, Indicator = new FakeIndicatorAuxiliary() },
                                new IndicatorValueElement() { IndicatorType = indicator, IndicatorArea = CandlesArea, IndicatorElement = chartIndicatorElement2, Indicator = new FakeIndicatorAuxiliary() }
                            });

                            smaChartArea.Elements.Add(chartIndicatorElement1);
                            CandlesArea.Elements.Add(chartIndicatorElement2);
                        }
                        break;
                }
            }

            //
            var areas = new List<ChartArea>();

            foreach (var list in IndicatorValueElements.Values)
            {
                areas.AddRange(list.Select(p => p.IndicatorArea));
            }

            areas.Add(CandlesArea);
            areas.Add(TradeCountArea);

            foreach (var area in areas)
            {
                if (area != CandlesArea)
                {
                    area.XAxises.First().IsVisible = false;
                }

                area.YAxises.First().AutoRange = true;

                area.XAxises[0].DrawMinorGridLines = false;
                area.YAxises[0].DrawMinorGridLines = false;
                area.XAxises[0].DrawMajorGridLines = false;
                area.YAxises[0].DrawMajorGridLines = false;
            }
        }

        public void ProcessCandle(CandleDataModel candleDataModel, int buyCount, int sellCount, bool isFinalTickOfShownTimeFrame)
        {
            //
            var buyCountIndicatorValue = new DecimalIndicatorValue(BuyCountFakeIndicator, Convert.ToDecimal(buyCount));
            var sellCountIndicatorValue = new DecimalIndicatorValue(SellCountFakeIndicator, Convert.ToDecimal(sellCount));

            buyCountIndicatorValue.IsFinal = true;
            buyCountIndicatorValue.IsEmpty = false;

            sellCountIndicatorValue.IsFinal = true;
            sellCountIndicatorValue.IsEmpty = false;

            //
            var timeFrameCandle = new TimeFrameCandle();

            timeFrameCandle.OpenTime = candleDataModel.OpenDateTime;
            timeFrameCandle.OpenPrice = Convert.ToDecimal(candleDataModel.Open);
            timeFrameCandle.HighPrice = Convert.ToDecimal(candleDataModel.High);
            timeFrameCandle.LowPrice = Convert.ToDecimal(candleDataModel.Low);
            timeFrameCandle.ClosePrice = Convert.ToDecimal(candleDataModel.Close);
            timeFrameCandle.TotalVolume = Convert.ToDecimal(candleDataModel.Volume);
            timeFrameCandle.Security = new Security();

            if (isFinalTickOfShownTimeFrame)
            {
                timeFrameCandle.State = CandleStates.Finished;
            }
            else
            {
                timeFrameCandle.State = CandleStates.Active;
            }

            //
            var data = new ChartDrawData();

            var dataItem = data.Group(candleDataModel.OpenDateTime)
                               .Add(CandleElement, timeFrameCandle)
                               .Add(BuyCountElement, buyCountIndicatorValue)
                               .Add(SellCountElement, sellCountIndicatorValue);

            foreach (var indicatorValueElementFieldInfo in IndicatorValueElements.Keys)
            {
                foreach (var indicatorValueElement in IndicatorValueElements[indicatorValueElementFieldInfo])
                {
                    var decimalIndicatorValue = new DecimalIndicatorValue(indicatorValueElement.Indicator, Convert.ToDecimal(indicatorValueElementFieldInfo.GetValue(candleDataModel)));

                    decimalIndicatorValue.IsFinal = true;
                    decimalIndicatorValue.IsEmpty = false;

                    dataItem = dataItem.Add(indicatorValueElement.IndicatorElement, decimalIndicatorValue);
                }
            }

            ChartControl.Draw(data);
        }
    }
}

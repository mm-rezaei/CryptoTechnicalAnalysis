using System;
using System.Collections.Generic;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.Indicators
{
    public abstract class DivergenceIndicatorBase : IndicatorBase
    {
        public DivergenceIndicatorBase(IList<CandleDataModel> candles, Func<CandleDataModel, float> priceFunction, Func<CandleDataModel, float> indicatorValueFunction, float? indicatorValueBaseLine = null, float tolerateLineBreaking = 0, int divergenceCandlesDistance = 10, int maximumDivergenceDetection = 1)
        {
            Candles = candles;

            GetPriceFunction = priceFunction;

            GetIndicatorValueFunction = indicatorValueFunction;

            TolerateLineBreaking = tolerateLineBreaking;

            DivergenceCandlesDistance = divergenceCandlesDistance;

            MaximumDivergenceDetection = maximumDivergenceDetection;

            IndicatorValueBaseLine = indicatorValueBaseLine;
        }

        private int MaximumCandleCountForCheck { get; } = 120;

        private IList<CandleDataModel> Candles { get; }

        private Func<CandleDataModel, float> GetPriceFunction { get; }

        private Func<CandleDataModel, float> GetIndicatorValueFunction { get; }

        private float TolerateLineBreaking { get; }

        private int DivergenceCandlesDistance { get; }

        private int MaximumDivergenceDetection { get; }

        private float? IndicatorValueBaseLine { get; }

        protected abstract DivergenceTypes DivergenceType { get; }

        private bool AreAllValuesTopOfLine(int startIndex, int endIndex, Func<CandleDataModel, float> GetValueFunction)
        {
            bool result = true;

            // f(x) = ax + b
            float a = (GetValueFunction(Candles[endIndex]) - GetValueFunction(Candles[startIndex])) / ((float)(endIndex - startIndex));
            float b = GetValueFunction(Candles[endIndex]) - a * ((float)endIndex);

            for (var index = startIndex + 2; index < endIndex - 1; index++)
            {
                var lineValue = a * ((float)index) + b;
                var value = GetValueFunction(Candles[index]);

                if (lineValue >= 0)
                {
                    lineValue = lineValue * (1f - TolerateLineBreaking);
                }
                else
                {
                    lineValue = lineValue * (1f + TolerateLineBreaking);
                }

                if (lineValue > value)
                {
                    result = false;

                    break;
                }
            }

            return result;
        }

        private bool AreAllValuesBottomOfLine(int startIndex, int endIndex, Func<CandleDataModel, float> GetValueFunction)
        {
            bool result = true;

            // f(x) = ax + b
            float a = (GetValueFunction(Candles[endIndex]) - GetValueFunction(Candles[startIndex])) / ((float)(endIndex - startIndex));
            float b = GetValueFunction(Candles[endIndex]) - a * ((float)endIndex);

            for (var index = startIndex + 2; index < endIndex - 1; index++)
            {
                var lineValue = a * ((float)index) + b;
                var value = GetValueFunction(Candles[index]);

                if (lineValue >= 0)
                {
                    lineValue = lineValue * (1f + TolerateLineBreaking);
                }
                else
                {
                    lineValue = lineValue * (1f - TolerateLineBreaking);
                }

                if (lineValue < value)
                {
                    result = false;

                    break;
                }
            }

            return result;
        }

        private bool IsSuitableForDivergence(int index, Func<CandleDataModel, float> GetValueFunction)
        {
            var result = false;

            if (index > 1)
            {
                var value1 = GetValueFunction(Candles[index - 2]);
                var value2 = GetValueFunction(Candles[index - 1]);
                var value3 = GetValueFunction(Candles[index]);
                var value4 = GetValueFunction(Candles[index + 1]);
                var value5 = GetValueFunction(Candles[index + 2]);

                switch (DivergenceType)
                {
                    case DivergenceTypes.RegularAscending:
                    case DivergenceTypes.HiddenAscending:
                        {
                            if (Math.Min(value1, value2) > value3 && value3 < Math.Min(value4, value5))
                            {
                                result = true;
                            }
                        }
                        break;
                    case DivergenceTypes.RegularDescending:
                    case DivergenceTypes.HiddenDescending:
                        {
                            if (Math.Max(value1, value2) < value3 && value3 > Math.Max(value4, value5))
                            {
                                result = true;
                            }
                        }
                        break;
                }
            }

            return result;
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var result = 0;

            var startIndex = Candles.Count - 1 - (DivergenceCandlesDistance - 2) - 1 - 2;
            var endIndex = Candles.Count - 1 - 2;

            if (startIndex >= 0)
            {
                if (IsSuitableForDivergence(endIndex, GetPriceFunction) && IsSuitableForDivergence(endIndex, GetIndicatorValueFunction))
                {
                    var price2 = GetPriceFunction(Candles[endIndex]);
                    var indicatorValue2 = GetIndicatorValueFunction(Candles[endIndex]);

                    for (var index = startIndex; index >= 0; index--)
                    {
                        //
                        if (IsSuitableForDivergence(index, GetPriceFunction) && IsSuitableForDivergence(index, GetIndicatorValueFunction))
                        {
                            //
                            var price1 = GetPriceFunction(Candles[index]);
                            var indicatorValue1 = GetIndicatorValueFunction(Candles[index]);

                            //
                            var indicatorValuesValidation = true;

                            if (IndicatorValueBaseLine.HasValue)
                            {
                                switch (DivergenceType)
                                {
                                    case DivergenceTypes.RegularAscending:
                                    case DivergenceTypes.HiddenAscending:
                                        {
                                            if (indicatorValue1 >= IndicatorValueBaseLine || indicatorValue2 >= IndicatorValueBaseLine)
                                            {
                                                indicatorValuesValidation = false;
                                            }
                                        }
                                        break;
                                    case DivergenceTypes.RegularDescending:
                                    case DivergenceTypes.HiddenDescending:
                                        {
                                            if (indicatorValue1 <= IndicatorValueBaseLine || indicatorValue2 <= IndicatorValueBaseLine)
                                            {
                                                indicatorValuesValidation = false;
                                            }
                                        }
                                        break;
                                }
                            }

                            //
                            if (indicatorValuesValidation)
                            {
                                switch (DivergenceType)
                                {
                                    case DivergenceTypes.RegularAscending:
                                        {
                                            if (price1 > price2 && indicatorValue1 < indicatorValue2)
                                            {
                                                if (AreAllValuesTopOfLine(index, endIndex, GetPriceFunction) && AreAllValuesTopOfLine(index, endIndex, GetIndicatorValueFunction))
                                                {
                                                    result++;
                                                }
                                            }
                                        }
                                        break;
                                    case DivergenceTypes.RegularDescending:
                                        {
                                            if (price1 < price2 && indicatorValue1 > indicatorValue2)
                                            {
                                                if (AreAllValuesBottomOfLine(index, endIndex, GetPriceFunction) && AreAllValuesBottomOfLine(index, endIndex, GetIndicatorValueFunction))
                                                {
                                                    result++;
                                                }
                                            }
                                        }
                                        break;
                                    case DivergenceTypes.HiddenAscending:
                                        {
                                            if (price1 < price2 && indicatorValue1 > indicatorValue2)
                                            {
                                                if (AreAllValuesTopOfLine(index, endIndex, GetPriceFunction) && AreAllValuesTopOfLine(index, endIndex, GetIndicatorValueFunction))
                                                {
                                                    result++;
                                                }
                                            }
                                        }
                                        break;
                                    case DivergenceTypes.HiddenDescending:
                                        {
                                            if (price1 > price2 && indicatorValue1 < indicatorValue2)
                                            {
                                                if (AreAllValuesBottomOfLine(index, endIndex, GetPriceFunction) && AreAllValuesBottomOfLine(index, endIndex, GetIndicatorValueFunction))
                                                {
                                                    result++;
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }

                        //
                        if (endIndex - index + 1 >= MaximumCandleCountForCheck || result >= 10)
                        {
                            break;
                        }
                        else if (result == MaximumDivergenceDetection)
                        {
                            break;
                        }
                    }
                }
            }

            return new SingleIndicatorValue<int>(this, result);
        }
    }
}

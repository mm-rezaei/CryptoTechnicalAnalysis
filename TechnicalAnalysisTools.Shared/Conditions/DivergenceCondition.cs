using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "DivergenceType", typeof(DivergenceIndicatorTypes))]
    public class DivergenceCondition : CandleOperationConditionBase
    {
        public DivergenceCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, DivergenceIndicatorTypes divergence) : base(symbol, timeFrame, candleNumber)
        {
            Divergence = divergence;
        }

        private DivergenceIndicatorTypes Divergence { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            byte resultNumber = 0;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle != null)
            {
                switch (Divergence)
                {
                    case DivergenceIndicatorTypes.RegularAscendingRsi:
                        resultNumber = candle.RegularAscendingRsiDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularAscendingStochasticKValue:
                        resultNumber = candle.RegularAscendingStochasticKValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularAscendingStochasticDValue:
                        resultNumber = candle.RegularAscendingStochasticDValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularAscendingMacdValue:
                        resultNumber = candle.RegularAscendingMacdValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularAscendingMacdSignal:
                        resultNumber = candle.RegularAscendingMacdSignalDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularAscendingMacdHistogram:
                        resultNumber = candle.RegularAscendingMacdHistogramDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularDescendingRsi:
                        resultNumber = candle.RegularDescendingRsiDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularDescendingStochasticKValue:
                        resultNumber = candle.RegularDescendingStochasticKValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularDescendingStochasticDValue:
                        resultNumber = candle.RegularDescendingStochasticDValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularDescendingMacdValue:
                        resultNumber = candle.RegularDescendingMacdValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularDescendingMacdSignal:
                        resultNumber = candle.RegularDescendingMacdSignalDivergence;
                        break;
                    case DivergenceIndicatorTypes.RegularDescendingMacdHistogram:
                        resultNumber = candle.RegularDescendingMacdHistogramDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenAscendingRsi:
                        resultNumber = candle.HiddenAscendingRsiDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenAscendingStochasticKValue:
                        resultNumber = candle.HiddenAscendingStochasticKValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenAscendingStochasticDValue:
                        resultNumber = candle.HiddenAscendingStochasticDValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenAscendingMacdValue:
                        resultNumber = candle.HiddenAscendingMacdValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenAscendingMacdSignal:
                        resultNumber = candle.HiddenAscendingMacdSignalDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenAscendingMacdHistogram:
                        resultNumber = candle.HiddenAscendingMacdHistogramDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenDescendingRsi:
                        resultNumber = candle.HiddenDescendingRsiDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenDescendingStochasticKValue:
                        resultNumber = candle.HiddenDescendingStochasticKValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenDescendingStochasticDValue:
                        resultNumber = candle.HiddenDescendingStochasticDValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenDescendingMacdValue:
                        resultNumber = candle.HiddenDescendingMacdValueDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenDescendingMacdSignal:
                        resultNumber = candle.HiddenDescendingMacdSignalDivergence;
                        break;
                    case DivergenceIndicatorTypes.HiddenDescendingMacdHistogram:
                        resultNumber = candle.HiddenDescendingMacdHistogramDivergence;
                        break;
                }
            }
            else
            {
                AreNeededCandlesAvailable = false;
            }

            result = resultNumber > 0;

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}

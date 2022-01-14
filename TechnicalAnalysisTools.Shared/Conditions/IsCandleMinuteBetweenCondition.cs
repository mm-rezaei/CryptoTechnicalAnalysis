using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Start Minute", typeof(int))]
    [OperationParameter(1, "End Minute", typeof(int))]
    public class IsCandleMinuteBetweenCondition : CandleOperationConditionBase
    {
        public IsCandleMinuteBetweenCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, int start, int end) : base(symbol, timeFrame, candleNumber)
        {
            Start = start;

            End = end;
        }

        private int Start { get; }

        private int End { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle != null && Start <= End)
            {
                result = candle.MomentaryDateTime.Minute >= Start && candle.MomentaryDateTime.Minute <= End;
            }
            else
            {
                result = false;

                AreNeededCandlesAvailable = false;
            }

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}

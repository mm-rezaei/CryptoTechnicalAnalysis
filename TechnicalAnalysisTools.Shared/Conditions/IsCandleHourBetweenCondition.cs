using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Start Hour", typeof(int))]
    [OperationParameter(1, "End Hour", typeof(int))]
    public class IsCandleHourBetweenCondition : CandleOperationConditionBase
    {
        public IsCandleHourBetweenCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, int start, int end) : base(symbol, timeFrame, candleNumber)
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
                result = candle.MomentaryDateTime.Hour >= Start && candle.MomentaryDateTime.Hour <= End;
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

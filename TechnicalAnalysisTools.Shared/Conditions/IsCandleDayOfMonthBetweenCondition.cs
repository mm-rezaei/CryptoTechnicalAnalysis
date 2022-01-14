using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Start Day", typeof(int))]
    [OperationParameter(1, "End Day", typeof(int))]
    public class IsCandleDayOfMonthBetweenCondition : CandleOperationConditionBase
    {
        public IsCandleDayOfMonthBetweenCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, int start, int end) : base(symbol, timeFrame, candleNumber)
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
                result = candle.MomentaryDateTime.Day >= Start && candle.MomentaryDateTime.Day <= End;
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

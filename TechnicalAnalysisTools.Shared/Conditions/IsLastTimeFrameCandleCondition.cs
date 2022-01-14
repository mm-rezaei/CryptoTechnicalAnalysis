using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Next Minutes Count", typeof(int))]
    public class IsLastTimeFrameCandleCondition : CandleOperationConditionBase
    {
        public IsLastTimeFrameCandleCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, int nextMinutesCount) : base(symbol, timeFrame, candleNumber)
        {
            NextMinutesCount = nextMinutesCount;
        }

        private int NextMinutesCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle != null)
            {
                var dateTime = candle.MomentaryDateTime;

                if (NextMinutesCount != 0)
                {
                    dateTime = dateTime.AddMinutes(NextMinutesCount);
                }

                result = IsThisMinuteCandleLastTimeFrameCandle(dateTime, TimeFrame);
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

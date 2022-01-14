using System;
using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Day of Week", typeof(DayOfWeek))]
    public class IsCandleDayOfWeekCondition : CandleOperationConditionBase
    {
        public IsCandleDayOfWeekCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, DayOfWeek day) : base(symbol, timeFrame, candleNumber)
        {
            Day = day;
        }

        private DayOfWeek Day { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle != null)
            {
                result = candle.MomentaryDateTime.DayOfWeek == Day;
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

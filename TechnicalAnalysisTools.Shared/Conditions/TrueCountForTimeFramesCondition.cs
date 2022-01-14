using System.Linq;
using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "True Count", typeof(int))]
    public class TrueCountForTimeFramesCondition : LogicalOperationForTimeFramesConditionBase
    {
        public TrueCountForTimeFramesCondition(ICondition[] conditions, TimeFrames lowerTimeFrame, TimeFrames upperTimeFrame, int trueCount) : base(conditions, lowerTimeFrame, upperTimeFrame)
        {
            TrueCount = trueCount;
        }

        private int TrueCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            if (LowerTimeFrame <= UpperTimeFrame)
            {
                var trueResult = 0;

                var condition = Conditions[0];

                foreach (var selectedTimeFrame in OperationConditionHelper.TimeFrames.Where(p => p >= LowerTimeFrame && p <= UpperTimeFrame))
                {
                    if (condition.Calculate(operationCandleRequested, selectedTimeFrame, candleNumber))
                    {
                        AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;

                        trueResult++;

                        if (ShortCircuit)
                        {
                            if (trueResult >= TrueCount)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;
                    }
                }

                result = trueResult >= TrueCount;
            }
            else
            {
                result = false;
            }

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}

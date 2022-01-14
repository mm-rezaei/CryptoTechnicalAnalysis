using System.Linq;
using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "False Count", typeof(int))]
    public class FalseCountForTimeFramesCondition : LogicalOperationForTimeFramesConditionBase
    {
        public FalseCountForTimeFramesCondition(ICondition[] conditions, TimeFrames lowerTimeFrame, TimeFrames upperTimeFrame, int falseCount) : base(conditions, lowerTimeFrame, upperTimeFrame)
        {
            FalseCount = falseCount;
        }

        private int FalseCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            if (LowerTimeFrame <= UpperTimeFrame)
            {
                var falseResult = 0;

                var condition = Conditions[0];

                foreach (var selectedTimeFrame in OperationConditionHelper.TimeFrames.Where(p => p >= LowerTimeFrame && p <= UpperTimeFrame))
                {
                    if (!condition.Calculate(operationCandleRequested, selectedTimeFrame, candleNumber))
                    {
                        AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;

                        falseResult++;

                        if (ShortCircuit)
                        {
                            if (falseResult >= FalseCount)
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

                result = falseResult >= FalseCount;
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

using System.Linq;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public class OrForTimeFramesCondition : LogicalOperationForTimeFramesConditionBase
    {
        public OrForTimeFramesCondition(ICondition[] conditions, TimeFrames lowerTimeFrame, TimeFrames upperTimeFrame) : base(conditions, lowerTimeFrame, upperTimeFrame)
        {

        }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            if (LowerTimeFrame <= UpperTimeFrame)
            {
                result = false;

                var condition = Conditions[0];

                foreach (var selectedTimeFrame in OperationConditionHelper.TimeFrames.Where(p => p >= LowerTimeFrame && p <= UpperTimeFrame))
                {
                    if (ShortCircuit)
                    {
                        result = result || condition.Calculate(operationCandleRequested, selectedTimeFrame, candleNumber);

                        AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;

                        if (result)
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = result | condition.Calculate(operationCandleRequested, selectedTimeFrame, candleNumber);

                        AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;
                    }
                }
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

using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public class OrCondition : LogicalOperationConditionBase
    {
        public OrCondition(ICondition[] conditions) : base(conditions)
        {

        }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            var result = false;

            AreNeededCandlesAvailable = true;

            foreach (var condition in Conditions)
            {
                if (ShortCircuit)
                {
                    result = result || condition.Calculate(operationCandleRequested, timeFrame, candleNumber);

                    AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;

                    if (result)
                    {
                        break;
                    }
                }
                else
                {
                    result = result | condition.Calculate(operationCandleRequested, timeFrame, candleNumber);

                    AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;
                }
            }

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}

using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public class AndForNumbersCondition : LogicalOperationForNumbersConditionBase
    {
        public AndForNumbersCondition(ICondition[] conditions, int lowerNumber, int upperNumber) : base(conditions, lowerNumber, upperNumber)
        {

        }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            if (LowerNumber <= UpperNumber)
            {
                result = true;

                var condition = Conditions[0];

                for (var index = LowerNumber; index <= UpperNumber; index++)
                {
                    if (ShortCircuit)
                    {
                        result = result && condition.Calculate(operationCandleRequested, timeFrame, index);

                        AreNeededCandlesAvailable = AreNeededCandlesAvailable && condition.AreNeededCandlesAvailable;

                        if (!result)
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = result & condition.Calculate(operationCandleRequested, timeFrame, index);

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

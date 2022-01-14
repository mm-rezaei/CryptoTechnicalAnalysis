using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "False Count", typeof(int))]
    public class FalseCountForNumbersCondition : LogicalOperationForNumbersConditionBase
    {
        public FalseCountForNumbersCondition(ICondition[] conditions, int lowerNumber, int upperNumber, int falseCount) : base(conditions, lowerNumber, upperNumber)
        {
            FalseCount = falseCount;
        }

        private int FalseCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            if (LowerNumber <= UpperNumber)
            {
                var falseResult = 0;

                var condition = Conditions[0];

                for (var index = LowerNumber; index <= UpperNumber; index++)
                {
                    if (!condition.Calculate(operationCandleRequested, timeFrame, index))
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

using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "True Count", typeof(int))]
    public class TrueCountForNumbersCondition : LogicalOperationForNumbersConditionBase
    {
        public TrueCountForNumbersCondition(ICondition[] conditions, int lowerNumber, int upperNumber, int trueCount) : base(conditions, lowerNumber, upperNumber)
        {
            TrueCount = trueCount;
        }

        private int TrueCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            if (LowerNumber <= UpperNumber)
            {
                var trueResult = 0;

                var condition = Conditions[0];

                for (var index = LowerNumber; index <= UpperNumber; index++)
                {
                    if (condition.Calculate(operationCandleRequested, timeFrame, index))
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

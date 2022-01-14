using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "True Count", typeof(int))]
    public class TrueCountCondition : LogicalOperationConditionBase
    {
        public TrueCountCondition(ICondition[] conditions, int trueCount) : base(conditions)
        {
            TrueCount = trueCount;
        }

        private int TrueCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var trueResult = 0;

            foreach (var condition in Conditions)
            {
                if (condition.Calculate(operationCandleRequested, timeFrame, candleNumber))
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

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}


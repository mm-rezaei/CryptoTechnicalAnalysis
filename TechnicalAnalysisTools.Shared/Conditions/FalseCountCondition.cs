using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "False Count", typeof(int))]
    public class FalseCountCondition : LogicalOperationConditionBase
    {
        public FalseCountCondition(ICondition[] conditions, int falseCount) : base(conditions)
        {
            FalseCount = falseCount;
        }

        private int FalseCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var falseResult = 0;

            foreach (var condition in Conditions)
            {
                if (!condition.Calculate(operationCandleRequested, timeFrame, candleNumber))
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

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}


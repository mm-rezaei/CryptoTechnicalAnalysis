using System.Linq;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public class NotCondition : LogicalOperationConditionBase
    {
        public NotCondition(ICondition[] conditions) : base(conditions)
        {

        }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            var result = !Conditions.First().Calculate(operationCandleRequested, timeFrame, candleNumber);

            AreNeededCandlesAvailable = Conditions.First().AreNeededCandlesAvailable;

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}
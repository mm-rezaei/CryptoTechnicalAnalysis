using System;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public abstract class ConditionBase : ICondition
    {
        public Guid Id { get; set; }

        public bool AreNeededCandlesAvailable { get; set; } = true;

        public abstract bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null);

        public OperationCandleRequestedHandler OperationCandleRequested { get; set; }

        protected void OnConditionResultEvaluated(bool result)
        {
            ConditionResultEvaluated?.Invoke(Id, result);
        }

        public event ConditionResultEvaluatedHandler ConditionResultEvaluated;
    }
}

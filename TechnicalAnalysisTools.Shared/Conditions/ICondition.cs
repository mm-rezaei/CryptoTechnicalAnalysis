using System;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public interface ICondition
    {
        Guid Id { get; set; }

        bool AreNeededCandlesAvailable { get; set; }

        bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null);

        OperationCandleRequestedHandler OperationCandleRequested { get; set; }

        event ConditionResultEvaluatedHandler ConditionResultEvaluated;
    }
}

using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(-2, "Lower TimeFrame", typeof(TimeFrames))]
    [OperationParameter(-1, "Upper TimeFrame", typeof(TimeFrames))]
    public abstract class LogicalOperationForTimeFramesConditionBase : LogicalOperationConditionBase
    {
        public LogicalOperationForTimeFramesConditionBase(ICondition[] conditions, TimeFrames lowerTimeFrame, TimeFrames upperTimeFrame) : base(conditions)
        {
            LowerTimeFrame = lowerTimeFrame;

            UpperTimeFrame = upperTimeFrame;
        }

        protected TimeFrames LowerTimeFrame { get; set; }

        protected TimeFrames UpperTimeFrame { get; set; }
    }
}

using TechnicalAnalysisTools.Shared.Attributes;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(-2, "Lower Number", typeof(int))]
    [OperationParameter(-1, "Upper Number", typeof(int))]
    public abstract class LogicalOperationForNumbersConditionBase : LogicalOperationConditionBase
    {
        public LogicalOperationForNumbersConditionBase(ICondition[] conditions, int lowerNumber, int upperNumber) : base(conditions)
        {
            LowerNumber = lowerNumber;

            UpperNumber = upperNumber;
        }

        protected int LowerNumber { get; }

        protected int UpperNumber { get; }
    }
}

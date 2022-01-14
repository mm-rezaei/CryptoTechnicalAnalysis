
namespace TechnicalAnalysisTools.Shared.Conditions
{
    public abstract class LogicalOperationConditionBase : ConditionBase
    {
        public LogicalOperationConditionBase(ICondition[] conditions)
        {
            ShortCircuit = true;

            Conditions = conditions;
        }

        protected bool ShortCircuit { get; set; }

        public ICondition[] Conditions { get; }

        public void SetShortCircuit(bool value)
        {
            ShortCircuit = value;
        }
    }
}

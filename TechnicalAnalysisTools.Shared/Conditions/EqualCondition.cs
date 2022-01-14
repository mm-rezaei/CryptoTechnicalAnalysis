using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Field Name", typeof(CandleDataModelFields))]
    [OperationParameter(1, "Value", typeof(float))]
    public class EqualCondition : CandleOperationConditionBase
    {
        public EqualCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleDataModelFields field, float value) : base(symbol, timeFrame, candleNumber)
        {
            Field = field;

            Value = value;
        }

        private CandleDataModelFields Field { get; }

        private float Value { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle != null)
            {
                var value = (float)ReadFieldValue(candle, Field.ToString());

                result = value == Value;
            }
            else
            {
                result = false;

                AreNeededCandlesAvailable = false;
            }

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}

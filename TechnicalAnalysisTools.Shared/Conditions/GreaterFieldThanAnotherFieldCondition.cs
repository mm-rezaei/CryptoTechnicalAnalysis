using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Field Name 1", typeof(CandleDataModelFields))]
    [OperationParameter(1, "Field Name 2", typeof(CandleDataModelFields))]
    public class GreaterFieldThanAnotherFieldCondition : CandleOperationConditionBase
    {
        public GreaterFieldThanAnotherFieldCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleDataModelFields field1, CandleDataModelFields field2) : base(symbol, timeFrame, candleNumber)
        {
            Field1 = field1;
            Field2 = field2;
        }

        private CandleDataModelFields Field1 { get; }

        private CandleDataModelFields Field2 { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle != null)
            {
                var value1 = (float)ReadFieldValue(candle, Field1.ToString());
                var value2 = (float)ReadFieldValue(candle, Field2.ToString());

                result = value1 > value2;
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

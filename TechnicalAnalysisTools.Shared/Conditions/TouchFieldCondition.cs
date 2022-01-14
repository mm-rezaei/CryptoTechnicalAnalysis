using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Field Name 1", typeof(CandleDataModelFields))]
    [OperationParameter(1, "Field Name 2", typeof(CandleDataModelFields))]
    public class TouchFieldCondition : CandleOperationConditionBase
    {
        public TouchFieldCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleDataModelFields field1, CandleDataModelFields field2) : base(symbol, timeFrame, candleNumber)
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

            var candle1 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber, 1);
            var candle2 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle1 != null && candle2 != null)
            {
                var field1Value1 = (float)ReadFieldValue(candle1, Field1.ToString());
                var field2Value1 = (float)ReadFieldValue(candle1, Field2.ToString());

                var field1Value2 = (float)ReadFieldValue(candle2, Field1.ToString());
                var field2Value2 = (float)ReadFieldValue(candle2, Field2.ToString());

                if (field1Value1 >= field2Value1)
                {
                    result = field1Value2 <= field2Value2;
                }
                else
                {
                    result = field1Value2 >= field2Value2;
                }
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

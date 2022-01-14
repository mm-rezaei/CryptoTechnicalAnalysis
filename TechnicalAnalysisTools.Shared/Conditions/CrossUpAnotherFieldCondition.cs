using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Field Name 1", typeof(CandleDataModelFields))]
    [OperationParameter(1, "Field Name 2", typeof(CandleDataModelFields))]
    [OperationParameter(2, "Previous Candle No", typeof(int))]
    public class CrossUpAnotherFieldCondition : CandleOperationConditionBase
    {
        public CrossUpAnotherFieldCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleDataModelFields field1, CandleDataModelFields field2, int candlesCount) : base(symbol, timeFrame, candleNumber)
        {
            Field1 = field1;

            Field2 = field2;

            CandlesCount = candlesCount;
        }

        private CandleDataModelFields Field1 { get; }

        private CandleDataModelFields Field2 { get; }

        private int CandlesCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle1 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber, CandlesCount);
            var candle2 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle1 != null && candle2 != null)
            {
                var candle1Value1 = (float)ReadFieldValue(candle1, Field1.ToString());
                var candle1Value2 = (float)ReadFieldValue(candle1, Field2.ToString());

                var candle2Value1 = (float)ReadFieldValue(candle2, Field1.ToString());
                var candle2Value2 = (float)ReadFieldValue(candle2, Field2.ToString());

                result = candle1Value1 < candle1Value2 && candle2Value1 >= candle2Value2;
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

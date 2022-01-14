using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Field Name", typeof(CandleDataModelFields))]
    [OperationParameter(1, "Value", typeof(float))]
    [OperationParameter(2, "Previous Candle No", typeof(int))]
    public class CrossDownCondition : CandleOperationConditionBase
    {
        public CrossDownCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleDataModelFields field, float value, int candlesCount) : base(symbol, timeFrame, candleNumber)
        {
            Field = field;
			
            Value = value;

            CandlesCount = candlesCount;
        }

        private CandleDataModelFields Field { get; }

        private float Value { get; }
		
        private int CandlesCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle1 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber, CandlesCount);
            var candle2 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle1 != null && candle2 != null)
            {
                var value1 = (float)ReadFieldValue(candle1, Field.ToString());
                var value2 = (float)ReadFieldValue(candle2, Field.ToString());

                result = value1 > Value && value2 <= Value;
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

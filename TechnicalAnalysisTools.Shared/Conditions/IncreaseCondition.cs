using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Field Name", typeof(CandleDataModelFields))]
    [OperationParameter(1, "Previous Candle No", typeof(int))]
    public class IncreaseCondition : CandleOperationConditionBase
    {
        public IncreaseCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleDataModelFields field, int candlesCount) : base(symbol, timeFrame, candleNumber)
        {
            Field = field;

            CandlesCount = candlesCount;
        }

        private CandleDataModelFields Field { get; }

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

                result = value1 < value2;
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

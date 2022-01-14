using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Field Name", typeof(CandleDataModelFields))]
    [OperationParameter(1, "Changed %", typeof(float))]
    [OperationParameter(2, "Previous Candle No", typeof(int))]
    public class DumpCondition : CandleOperationConditionBase
    {
        public DumpCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleDataModelFields field, float changed, int candlesCount) : base(symbol, timeFrame, candleNumber)
        {
            Field = field;

            Changed = changed;

            CandlesCount = candlesCount;
        }

        private CandleDataModelFields Field { get; }

        private float Changed { get; }

        private int CandlesCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            var result = false;

            AreNeededCandlesAvailable = true;

            for (var index = 1; index <= CandlesCount; index++)
            {
                var candle1 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber, index);
                var candle2 = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

                if (candle1 != null && candle2 != null)
                {
                    var value1 = (float)ReadFieldValue(candle1, Field.ToString());
                    var value2 = (float)ReadFieldValue(candle2, Field.ToString());

                    var value = value1 * (1f - (Changed / 100f));

                    result = value2 <= value;
                }
                else
                {
                    result = false;

                    AreNeededCandlesAvailable = false;
                }

                if (result || !AreNeededCandlesAvailable)
                {
                    break;
                }
            }

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}

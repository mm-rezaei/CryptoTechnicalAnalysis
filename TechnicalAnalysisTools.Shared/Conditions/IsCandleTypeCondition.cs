using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Candle Type", typeof(CandleType))]
    public class IsCandleTypeCondition : CandleOperationConditionBase
    {
        public IsCandleTypeCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, CandleType candleType) : base(symbol, timeFrame, candleNumber)
        {
            CandleType = candleType;
        }

        private CandleType CandleType { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber);

            if (candle != null)
            {
                result = candle.CandleType.HasFlag(CandleType);
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

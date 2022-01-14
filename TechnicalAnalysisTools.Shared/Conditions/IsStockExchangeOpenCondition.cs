using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Stock Exchange", typeof(StockExchanges))]
    [OperationParameter(1, "Previous Candle No", typeof(int))]
    public class IsStockExchangeOpenCondition : CandleOperationConditionBase
    {
        public IsStockExchangeOpenCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, StockExchanges stockExchanges, int candlesCount) : base(symbol, timeFrame, candleNumber)
        {
            Exchange = stockExchanges;

            CandlesCount = candlesCount;
        }

        private StockExchanges Exchange { get; }

        private int CandlesCount { get; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result;

            AreNeededCandlesAvailable = true;

            var candle = RequestOperationCandle(operationCandleRequested, timeFrame, candleNumber, CandlesCount);

            if (candle != null)
            {
                result = StockExchangesTimeHelper.IsStockExchangeOpen(Exchange, candle.MomentaryDateTime);
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

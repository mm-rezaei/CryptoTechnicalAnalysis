using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class StochasticRsiIndicatorCaller : IndicatorCallerBase<StochasticRsiIndicator>
    {
        public StochasticRsiIndicatorCaller(StochasticRsiIndicator indicator) : base(indicator)
        {

        }

        protected override bool OnProcess(Candle candle, CandleDataModel candleDataModel)
        {
            var result = false;

            var indicatorValue = Indicator.Process(candle.ClosePrice, candle.State == StockSharp.Messages.CandleStates.Finished);

            if (Indicator.IsFormed)
            {
                if (!indicatorValue.IsEmpty)
                {
                    candleDataModel.StochRsiKValue = Indicator.K.GetCurrentValue<float>();
                    candleDataModel.StochRsiDValue = Indicator.D.GetCurrentValue<float>();

                    result = true;
                }
            }
            else
            {
                candleDataModel.StochRsiKValue = 0;
                candleDataModel.StochRsiDValue = 0;

                result = true;
            }

            return result;
        }
    }
}

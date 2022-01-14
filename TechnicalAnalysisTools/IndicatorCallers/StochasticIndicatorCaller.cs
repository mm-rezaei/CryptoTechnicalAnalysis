using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class StochasticIndicatorCaller : IndicatorCallerBase<StochasticIndicator>
    {
        public StochasticIndicatorCaller(StochasticIndicator indicator) : base(indicator)
        {

        }

        protected override bool OnProcess(Candle candle, CandleDataModel candleDataModel)
        {
            var result = false;

            var indicatorValue = Indicator.Process(candle);

            if (Indicator.IsFormed)
            {
                if (!indicatorValue.IsEmpty)
                {
                    candleDataModel.StochKValue = Indicator.K.GetCurrentValue<float>();
                    candleDataModel.StochDValue = Indicator.D.GetCurrentValue<float>();

                    result = true;
                }
            }
            else
            {
                candleDataModel.StochKValue = 0;
                candleDataModel.StochDValue = 0;

                result = true;
            }

            return result;
        }
    }
}


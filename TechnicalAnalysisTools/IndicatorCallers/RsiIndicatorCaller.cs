using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class RsiIndicatorCaller : IndicatorCallerBase<RsiIndicator>
    {
        public RsiIndicatorCaller(RsiIndicator indicator) : base(indicator)
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
                    candleDataModel.RsiValue = indicatorValue.GetValue<float>();

                    result = true;
                }
            }
            else
            {
                candleDataModel.RsiValue = 0;

                result = true;
            }

            return result;
        }
    }
}


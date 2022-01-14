using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class WilliamsRIndicatorCaller : IndicatorCallerBase<WilliamsRIndicator>
    {
        public WilliamsRIndicatorCaller(WilliamsRIndicator indicator) : base(indicator)
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
                    candleDataModel.WilliamsRValue = indicatorValue.GetValue<float>();

                    result = true;
                }
            }
            else
            {
                candleDataModel.WilliamsRValue = 0;

                result = true;
            }

            return result;
        }
    }
}

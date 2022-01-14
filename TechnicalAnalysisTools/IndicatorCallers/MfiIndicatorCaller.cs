using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class MfiIndicatorCaller : IndicatorCallerBase<MfiIndicator>
    {
        public MfiIndicatorCaller(MfiIndicator indicator) : base(indicator)
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
                    candleDataModel.MfiValue = indicatorValue.GetValue<float>();

                    result = true;
                }
            }
            else
            {
                candleDataModel.MfiValue = 0;

                result = true;
            }

            return result;
        }
    }
}


using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class BollingerBandsIndicatorCaller : IndicatorCallerBase<BollingerBandsIndicator>
    {
        public BollingerBandsIndicatorCaller(BollingerBandsIndicator indicator) : base(indicator)
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
                    candleDataModel.BollingerBandsBasis = Indicator.MovingAverage.GetCurrentValue<float>();
                    candleDataModel.BollingerLower = Indicator.LowBand.GetCurrentValue<float>();
                    candleDataModel.BollingerUpper = Indicator.UpBand.GetCurrentValue<float>();

                    result = true;
                }
            }
            else
            {
                candleDataModel.BollingerBandsBasis = 0;
                candleDataModel.BollingerLower = 0;
                candleDataModel.BollingerUpper = 0;

                result = true;
            }

            return result;
        }
    }
}


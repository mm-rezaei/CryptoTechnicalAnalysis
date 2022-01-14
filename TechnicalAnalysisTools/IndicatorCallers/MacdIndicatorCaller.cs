using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class MacdIndicatorCaller : IndicatorCallerBase<MacdIndicator>
    {
        public MacdIndicatorCaller(MacdIndicator indicator) : base(indicator)
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
                    candleDataModel.MacdValue = Indicator.Macd.GetCurrentValue<float>();
                    candleDataModel.MacdSignal = Indicator.SignalMa.GetCurrentValue<float>();
                    candleDataModel.MacdHistogram = candleDataModel.MacdValue - candleDataModel.MacdSignal;

                    result = true;
                }
            }
            else
            {
                candleDataModel.MacdValue = 0;
                candleDataModel.MacdSignal = 0;
                candleDataModel.MacdHistogram = 0;

                result = true;
            }

            return result;
        }
    }
}


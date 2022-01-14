﻿using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class HiddenAscendingStochasticKValueDivergenceIndicatorCaller : IndicatorCallerBase<HiddenAscendingDivergenceIndicator>
    {
        public HiddenAscendingStochasticKValueDivergenceIndicatorCaller(HiddenAscendingDivergenceIndicator indicator) : base(indicator)
        {

        }

        protected override bool OnProcess(Candle candle, CandleDataModel candleDataModel)
        {
            var result = false;

            var indicatorValue = Indicator.Process(candle);

            if (!indicatorValue.IsEmpty)
            {
                var divergence = indicatorValue.GetValue<int>();

                if (divergence < 0)
                {
                    divergence = 0;
                }
                else if (divergence > 255)
                {
                    divergence = 255;
                }

                candleDataModel.HiddenAscendingStochasticKValueDivergence = (byte)divergence;

                result = true;
            }

            return result;
        }
    }
}

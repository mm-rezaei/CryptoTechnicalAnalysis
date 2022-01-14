using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class IchimokuIndicatorCaller : IndicatorCallerBase<IchimokuIndicator>
    {
        public IchimokuIndicatorCaller(IchimokuIndicator indicator) : base(indicator)
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
                    candleDataModel.IchimokuChikouSpan = Indicator.Chinkou.GetCurrentValue<float>();

                    candleDataModel.IchimokuKijunSen = Indicator.Kijun.GetCurrentValue<float>();
                    candleDataModel.IchimokuTenkanSen = Indicator.Tenkan.GetCurrentValue<float>();
                    candleDataModel.IchimokuSenkouSpanA = Indicator.SenkouA.GetCurrentValue<float>();
                    candleDataModel.IchimokuSenkouSpanA26 = Indicator.SenkouA26.GetCurrentValue<float>();
                    candleDataModel.IchimokuSenkouSpanB = Indicator.SenkouB.GetCurrentValue<float>();
                    candleDataModel.IchimokuSenkouSpanB26 = Indicator.SenkouB26.GetCurrentValue<float>();

                    result = true;
                }
            }
            else
            {
                candleDataModel.IchimokuChikouSpan = 0;

                candleDataModel.IchimokuKijunSen = 0;
                candleDataModel.IchimokuTenkanSen = 0;
                candleDataModel.IchimokuSenkouSpanA = 0;
                candleDataModel.IchimokuSenkouSpanA26 = 0;
                candleDataModel.IchimokuSenkouSpanB = 0;
                candleDataModel.IchimokuSenkouSpanB26 = 0;

                result = true;
            }

            return result;
        }
    }
}


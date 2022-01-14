using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class MfiIndicator : IndicatorBase
    {
        public MfiIndicator()
        {
        }

        public override bool IsFormed { get => true; }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var candle = input.GetValue<Candle>();

            if (candle.TotalVolume == 0)
                return new DecimalIndicatorValue(this);

            if (input.IsFinal)
                IsFormed = true;

            return new DecimalIndicatorValue(this, (candle.HighPrice - candle.LowPrice) / candle.TotalVolume);
        }
    }
}

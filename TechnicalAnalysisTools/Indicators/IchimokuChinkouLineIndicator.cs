using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class IchimokuChinkouLineIndicator : LengthIndicatorBase<decimal>
    {
        public IchimokuChinkouLineIndicator()
        {
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var price = input.GetValue<Candle>().ClosePrice;

            if (Buffer.Count > Length)
                Buffer.RemoveAt(0);

            if (input.IsFinal)
                Buffer.Add(price);

            return new DecimalIndicatorValue(this, price);
        }
    }
}

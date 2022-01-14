using System.Linq;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class SmaIndicator : LengthIndicatorBase<decimal>
    {
        public SmaIndicator()
        {
            Length = 32;
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            var newValue = input.GetValue<decimal>();

            if (input.IsFinal)
            {
                Buffer.Add(newValue);

                if (Buffer.Count > Length)
                    Buffer.RemoveAt(0);
            }

            if (input.IsFinal)
                return new DecimalIndicatorValue(this, Buffer.Sum() / Length);

            return new DecimalIndicatorValue(this, (Buffer.Skip(1).Sum() + newValue) / Length);
        }
    }
}

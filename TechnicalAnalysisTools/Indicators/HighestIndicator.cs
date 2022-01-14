using System;
using System.Linq;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class HighestIndicator : LengthIndicatorBase<decimal>
    {
        public HighestIndicator()
        {
            Length = 5;
        }

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            decimal result;

            var newValue = input.GetValue<decimal>();

            if (input.IsFinal)
            {
                Buffer.Add(newValue);

                if (Buffer.Count > Length)
                {
                    Buffer.RemoveAt(0);
                }

                result = Buffer.Aggregate(newValue, (current, t) => Math.Max(t, current));
            }
            else if (Buffer.Count == Length)
            {
                result = Buffer.Skip(1).Aggregate(newValue, (current, t) => Math.Max(t, current));
            }
            else
            {
                result = Buffer.Aggregate(newValue, (current, t) => Math.Max(t, current));
            }

            return new DecimalIndicatorValue(this, result);
        }
    }
}

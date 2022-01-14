using System;
using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class BollingerBandsIndicator : ComplexIndicatorBase
    {
        private readonly StandardDeviationIndicator _dev = new StandardDeviationIndicator();

        public BollingerBandsIndicator() : this(new SmaIndicator())
        {
        }

        public BollingerBandsIndicator(LengthIndicatorBase<decimal> ma)
        {
            InnerIndicators.Add(MovingAverage = ma);
            InnerIndicators.Add(UpBand = new BollingerBandIndicator(MovingAverage, _dev) { Name = "UpBand" });
            InnerIndicators.Add(LowBand = new BollingerBandIndicator(MovingAverage, _dev) { Name = "LowBand" });
            Width = 2;
        }

        public LengthIndicatorBase<decimal> MovingAverage { get; }

        public BollingerBandIndicator UpBand { get; }

        public BollingerBandIndicator LowBand { get; }

        public virtual int Length
        {
            get => MovingAverage.Length;
            set
            {
                _dev.Length = MovingAverage.Length = value;
                Reset();
            }
        }

        public decimal Width
        {
            get => UpBand.Width;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Incorrect value of bandwidth.");

                UpBand.Width = value;
                LowBand.Width = -value;

                Reset();
            }
        }

        public override void Reset()
        {
            base.Reset();
            _dev.Reset();
        }

        public override bool IsFormed => MovingAverage.IsFormed;

        protected override IIndicatorValue OnProcess(IIndicatorValue input)
        {
            _dev.Process(input);
            var maValue = MovingAverage.Process(input);
            var value = new ComplexIndicatorValue(this);
            value.InnerValues.Add(MovingAverage, maValue);
            value.InnerValues.Add(UpBand, UpBand.Process(input));
            value.InnerValues.Add(LowBand, LowBand.Process(input));
            return value;
        }
    }
}

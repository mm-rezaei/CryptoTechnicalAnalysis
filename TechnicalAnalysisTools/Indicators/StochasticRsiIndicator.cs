using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class StochasticRsiIndicator : ComplexIndicatorBase
    {
        public StochasticRsiIndicator()
        {
            InnerIndicators.Add(new RsiIndicator() { Length = 14 });
            InnerIndicators.Add(new StochasticRsiKIndicator());
            InnerIndicators.Add(K = new SmaIndicator { Length = 3 });
            InnerIndicators.Add(D = new SmaIndicator { Length = 3 });

            Mode = ComplexIndicatorModes.Sequence;
        }

        public SmaIndicator K { get; }

        public SmaIndicator D { get; }
    }
}

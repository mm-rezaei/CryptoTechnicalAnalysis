using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class StochasticIndicator : ComplexIndicatorBase
    {
        public StochasticIndicator()
        {
            InnerIndicators.Add(new StochasticKIndicator());
            InnerIndicators.Add(K = new SmaIndicator { Length = 3 });
            InnerIndicators.Add(D = new SmaIndicator { Length = 3 });

            Mode = ComplexIndicatorModes.Sequence;
        }

        public SmaIndicator K { get; }

        public SmaIndicator D { get; }
    }
}

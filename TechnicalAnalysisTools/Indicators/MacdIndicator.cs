using StockSharp.Algo.Indicators;

namespace TechnicalAnalysisTools.Indicators
{
    public class MacdIndicator : ComplexIndicatorBase
    {
        public MacdIndicator() : this(new MovingAverageConvergenceDivergenceIndicator(), new EmaIndicator { Length = 9 })
        {
        }

        public MacdIndicator(MovingAverageConvergenceDivergenceIndicator macd, EmaIndicator signalMa) : base(macd, signalMa)
        {
            Macd = macd;
            SignalMa = signalMa;
            Mode = ComplexIndicatorModes.Sequence;
        }

        public MovingAverageConvergenceDivergenceIndicator Macd { get; }

        public EmaIndicator SignalMa { get; }
    }
}

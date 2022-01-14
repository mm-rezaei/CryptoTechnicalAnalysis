using System;

namespace TechnicalAnalysisTools.Shared.Enumerations
{
    [Flags]
    public enum CandleType : byte
    {
        None = 0,
        Bullish = 1,
        Bearish = 2,
        Dragonfly = 4,
        Gravestone = 8,
        Hammer = 16,
        Marubozu = 32,
        SpinningTop = 64
    }
}

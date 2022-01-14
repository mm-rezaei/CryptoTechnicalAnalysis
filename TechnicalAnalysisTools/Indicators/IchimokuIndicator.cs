using System;

namespace TechnicalAnalysisTools.Indicators
{
    public class IchimokuIndicator : ComplexIndicatorBase
    {
        public IchimokuIndicator() : this(new IchimokuLineIndicator { Length = 9 }, new IchimokuLineIndicator { Length = 26 })
        {
        }

        public IchimokuIndicator(IchimokuLineIndicator tenkan, IchimokuLineIndicator kijun)
        {
            if (tenkan == null)
                throw new ArgumentNullException(nameof(tenkan));

            if (kijun == null)
                throw new ArgumentNullException(nameof(kijun));

            InnerIndicators.Add(Tenkan = tenkan);
            InnerIndicators.Add(Kijun = kijun);
            InnerIndicators.Add(SenkouA = new IchimokuSenkouALineIndicator(Tenkan, Kijun));
            InnerIndicators.Add(SenkouA26 = new IchimokuSenkouA26LineIndicator(Tenkan, Kijun));
            InnerIndicators.Add(SenkouB = new IchimokuSenkouBLineIndicator(Kijun) { Length = 52 });
            InnerIndicators.Add(SenkouB26 = new IchimokuSenkouB26LineIndicator(Kijun) { Length = 52 });
            InnerIndicators.Add(Chinkou = new IchimokuChinkouLineIndicator { Length = kijun.Length });
        }

        public IchimokuLineIndicator Tenkan { get; }

        public IchimokuLineIndicator Kijun { get; }

        public IchimokuSenkouALineIndicator SenkouA { get; }

        public IchimokuSenkouA26LineIndicator SenkouA26 { get; }

        public IchimokuSenkouBLineIndicator SenkouB { get; }

        public IchimokuSenkouB26LineIndicator SenkouB26 { get; }

        public IchimokuChinkouLineIndicator Chinkou { get; }
    }
}

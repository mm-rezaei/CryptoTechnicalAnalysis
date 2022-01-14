using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public abstract class TradeSubOrderModeAuxiliaryBase
    {
        public TradeSubOrderModeAuxiliaryBase(float primaryPrice)
        {
            PrimaryPrice = primaryPrice;
        }

        protected float PrimaryPrice { get; set; }

        public abstract float TargetPrice { get; }

        public abstract TradeSubOrderModes TradeSubOrderMode { get; }

        public abstract bool TraversePrice(float fromPrice, float toPrice, out float matchedPrice);
    }
}

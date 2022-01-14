
namespace TechnicalAnalysisTools.Trading.Ui.Win.Auxiliaries
{
    internal abstract class TradeSubOrderModeAuxiliaryBase
    {
        public TradeSubOrderModeAuxiliaryBase(decimal primaryPrice)
        {
            PrimaryPrice = primaryPrice;
        }

        protected decimal PrimaryPrice { get; set; }

        public abstract bool CheckPrice(decimal price);
    }
}

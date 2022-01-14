using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class TradeAllocatedBalanceAuxiliary
    {
        public TradeAllocatedBalanceAuxiliary(PositionTypes position, float allocatedQuoteBalance, byte leverage, float marketFeePercent)
        {
            Position = position;

            AllocatedQuoteBalance = allocatedQuoteBalance;

            Leverage = leverage;

            MarketFee = marketFeePercent / 100f;
        }

        public PositionTypes Position { get; private set; }

        public float AllocatedQuoteBalance { get; private set; }

        public byte Leverage { get; private set; }

        public float MarketFee { get; private set; }

        public float EnterPrice { get; private set; }

        public float EnterLeveragedBalance { get; private set; }

        public float EnterLeveragedQuoteBalance { get; private set; }

        public float EnterMarketFee { get; private set; }

        public float ExitMarketFee { get; private set; }

        public float TotalMarketFee
        {
            get
            {
                return EnterMarketFee + ExitMarketFee;
            }
        }

        public float Profit { get; private set; }

        public float DeallocatedQuoteBalance
        {
            get
            {
                return AllocatedQuoteBalance + Profit;
            }
        }

        private void EstimateByExitPrice(float exitPrice, out float exitMarketFee, out float profit)
        {
            var enterLeveragedQuoteBalance = EnterPrice * EnterLeveragedBalance;
            var exitLeveragedQuoteBalance = exitPrice * EnterLeveragedBalance;

            exitMarketFee = exitLeveragedQuoteBalance * MarketFee;

            if (Position == PositionTypes.Long)
            {
                profit = exitLeveragedQuoteBalance - enterLeveragedQuoteBalance - EnterMarketFee - exitMarketFee;
            }
            else
            {
                profit = enterLeveragedQuoteBalance - exitLeveragedQuoteBalance - EnterMarketFee - exitMarketFee;
            }
        }

        public void SetEnterPrice(float enterPrice)
        {
            EnterPrice = enterPrice;

            EnterLeveragedQuoteBalance = (1f - MarketFee) * (AllocatedQuoteBalance * Leverage);

            EnterMarketFee = (AllocatedQuoteBalance * Leverage) - EnterLeveragedQuoteBalance;

            EnterLeveragedBalance = EnterLeveragedQuoteBalance / enterPrice;
        }

        public float GetLiquidPrice()
        {
            float result;

            var enterLeveragedQuoteBalance = EnterPrice * EnterLeveragedBalance;

            if (Position == PositionTypes.Long)
            {
                result = (enterLeveragedQuoteBalance + EnterMarketFee - AllocatedQuoteBalance) / EnterLeveragedBalance / (1 - MarketFee);
            }
            else
            {
                result = (AllocatedQuoteBalance + enterLeveragedQuoteBalance - EnterMarketFee) / (EnterLeveragedBalance + (EnterLeveragedBalance * MarketFee));
            }

            return result;
        }

        public void SetExitPrice(float exitPrice)
        {
            float exitMarketFee;
            float profit;

            EstimateByExitPrice(exitPrice, out exitMarketFee, out profit);

            ExitMarketFee = exitMarketFee;
            Profit = profit;
        }

        public void EstimateByExitPrice(float exitPrice, out float profit, out float totalMarketFee, out float deallocatedQuoteBalance)
        {
            float exitMarketFee;

            EstimateByExitPrice(exitPrice, out exitMarketFee, out profit);

            totalMarketFee = EnterMarketFee + exitMarketFee;

            deallocatedQuoteBalance = AllocatedQuoteBalance + profit;
        }
    }
}

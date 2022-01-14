using System.Collections.Generic;
using System.Threading;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class TradeWalletAuxiliary
    {
        private Semaphore WalletSemaphore { get; } = new Semaphore(1, 1);

        private float MarketFeePercent { get; set; }

        private byte Leverage { get; set; }

        private float SaveProfitPercent { get; set; }

        private TradeAmountModes TradeAmountMode { get; set; }

        private float TradeAmountPercent { get; set; }

        private float TradeAmountFixedValue { get; set; }

        private int TotalSuccessfulTradeCount { get; set; }

        public float TotalSavedProfit { get; private set; }

        public float TotalPaiedMarketFee { get; private set; }

        public float TotalPayableBalance { get; private set; }

        public float TotalWalletBalance
        {
            get
            {
                return TotalSavedProfit + TotalPayableBalance;
            }
        }

        public int WonTradeCount { get; private set; }

        public float WonTradePercent
        {
            get
            {
                return ((float)WonTradeCount) / ((float)TotalSuccessfulTradeCount) * 100f;
            }
        }

        private void EstimateSuccessfulTrade(float totalMarketFee, float profit, float deallocatedQuoteBalance, ref int totalSuccessfulTradeCount, ref int wonTradeCount, ref float totalPaiedMarketFee, ref float totalSavedProfit, ref float totalPayableBalance)
        {
            totalSuccessfulTradeCount++;

            totalPaiedMarketFee += totalMarketFee;

            if (profit > 0)
            {
                wonTradeCount++;

                var saveprofit = SaveProfitPercent / 100f * profit;

                totalSavedProfit += saveprofit;

                totalPayableBalance += deallocatedQuoteBalance - saveprofit;
            }
            else
            {
                if (deallocatedQuoteBalance > 0)
                {
                    totalPayableBalance += deallocatedQuoteBalance;
                }
            }
        }

        public void Init(StrategyTestDataModel strategyTestData)
        {
            //
            MarketFeePercent = strategyTestData.MarketFeePercent;
            Leverage = strategyTestData.Leverage;
            SaveProfitPercent = strategyTestData.SaveProfitPercentOfWinPosition;
            TradeAmountMode = strategyTestData.TradeAmountMode;
            TradeAmountPercent = strategyTestData.TradeAmountPercent;
            TradeAmountFixedValue = strategyTestData.TradeAmountFixedValue;
            TotalSuccessfulTradeCount = 0;

            //
            TotalSavedProfit = 0;
            TotalPaiedMarketFee = 0;
            TotalPayableBalance = strategyTestData.InitialBaseCoinDeposit;
        }

        public TradeAllocatedBalanceAuxiliary AllocateForFixedAmount(PositionTypes position)
        {
            var result = 0f;

            WalletSemaphore.WaitOne();

            try
            {
                var amount = 0f;

                if (TradeAmountMode == TradeAmountModes.Fixed)
                {
                    amount = TradeAmountFixedValue;
                }
                else if (TradeAmountMode == TradeAmountModes.PercentWithMinimumFixed)
                {
                    if (TotalPayableBalance > 0)
                    {
                        amount = TradeAmountPercent / 100f * TotalPayableBalance;

                        if (amount < TradeAmountFixedValue)
                        {
                            amount = TradeAmountFixedValue;
                        }
                    }
                }
                else if (TradeAmountMode == TradeAmountModes.PercentWithMaximumFixed)
                {
                    if (TotalPayableBalance > 0)
                    {
                        amount = TradeAmountPercent / 100f * TotalPayableBalance;

                        if (amount > TradeAmountFixedValue)
                        {
                            amount = TradeAmountFixedValue;
                        }
                    }
                }
                else if (TradeAmountMode == TradeAmountModes.Percent)
                {
                    if (TotalPayableBalance > 0)
                    {
                        amount = TradeAmountPercent / 100f * TotalPayableBalance;
                    }
                }

                if (amount <= TotalPayableBalance)
                {
                    result = amount;
                }
                else
                {
                    result = TotalPayableBalance;
                }

                TotalPayableBalance -= result;
            }
            catch
            {
                result = 0;
            }
            finally
            {
                WalletSemaphore.Release();
            }

            if (result <= 0)
            {
                return null;
            }
            else
            {
                return new TradeAllocatedBalanceAuxiliary(position, result, Leverage, MarketFeePercent);
            }
        }

        public TradeAllocatedBalanceAuxiliary AllocateForGridFirstStepAmount(PositionTypes position, int stepCountForGridAllocating)
        {
            var result = 0f;

            WalletSemaphore.WaitOne();

            try
            {
                var amount = 0f;

                if (TradeAmountMode == TradeAmountModes.Fixed)
                {
                    amount = TradeAmountFixedValue / stepCountForGridAllocating;
                }
                else if (TradeAmountMode == TradeAmountModes.PercentWithMinimumFixed)
                {
                    if (TotalPayableBalance > 0)
                    {
                        amount = TradeAmountPercent / 100f * TotalPayableBalance / stepCountForGridAllocating;

                        if (amount < (TradeAmountFixedValue / stepCountForGridAllocating))
                        {
                            amount = TradeAmountFixedValue / stepCountForGridAllocating;
                        }
                    }
                }
                else if (TradeAmountMode == TradeAmountModes.PercentWithMaximumFixed)
                {
                    if (TotalPayableBalance > 0)
                    {
                        amount = TradeAmountPercent / 100f * TotalPayableBalance / stepCountForGridAllocating;

                        if (amount > (TradeAmountFixedValue / stepCountForGridAllocating))
                        {
                            amount = TradeAmountFixedValue / stepCountForGridAllocating;
                        }
                    }
                }
                else if (TradeAmountMode == TradeAmountModes.Percent)
                {
                    if (TotalPayableBalance > 0)
                    {
                        amount = TradeAmountPercent / 100f * TotalPayableBalance / stepCountForGridAllocating;
                    }
                }

                if (amount <= TotalPayableBalance)
                {
                    result = amount;
                }
                else
                {
                    result = TotalPayableBalance;
                }

                TotalPayableBalance -= result;
            }
            catch
            {
                result = 0;
            }
            finally
            {
                WalletSemaphore.Release();
            }

            if (result <= 0)
            {
                return null;
            }
            else
            {
                return new TradeAllocatedBalanceAuxiliary(position, result, Leverage, MarketFeePercent);
            }
        }

        public TradeAllocatedBalanceAuxiliary AllocateForGridNextStepAmount(PositionTypes position, float nextStepAmount)
        {
            var result = 0f;

            WalletSemaphore.WaitOne();

            try
            {
                if (nextStepAmount <= TotalPayableBalance)
                {
                    result = nextStepAmount;
                }
                else
                {
                    result = TotalPayableBalance;
                }

                TotalPayableBalance -= result;
            }
            catch
            {
                result = 0;
            }
            finally
            {
                WalletSemaphore.Release();
            }

            if (result <= 0)
            {
                return null;
            }
            else
            {
                return new TradeAllocatedBalanceAuxiliary(position, result, Leverage, MarketFeePercent);
            }
        }

        public void DeallocateSuccessfulTrade(TradeAllocatedBalanceAuxiliary tradeAllocatedBalanceDataObject)
        {
            WalletSemaphore.WaitOne();

            try
            {
                var totalSuccessfulTradeCount = TotalSuccessfulTradeCount;
                var wonTradeCount = WonTradeCount;
                var totalPaiedMarketFee = TotalPaiedMarketFee;
                var totalSavedProfit = TotalSavedProfit;
                var totalPayableBalance = TotalPayableBalance;

                EstimateSuccessfulTrade(tradeAllocatedBalanceDataObject.TotalMarketFee, tradeAllocatedBalanceDataObject.Profit, tradeAllocatedBalanceDataObject.DeallocatedQuoteBalance, ref totalSuccessfulTradeCount, ref wonTradeCount, ref totalPaiedMarketFee, ref totalSavedProfit, ref totalPayableBalance);

                TotalSuccessfulTradeCount = totalSuccessfulTradeCount;
                WonTradeCount = wonTradeCount;
                TotalPaiedMarketFee = totalPaiedMarketFee;
                TotalSavedProfit = totalSavedProfit;
                TotalPayableBalance = totalPayableBalance;
            }
            catch
            {

            }
            finally
            {
                WalletSemaphore.Release();
            }
        }

        public void DeallocateFailedTrade(TradeAllocatedBalanceAuxiliary tradeAllocatedBalanceDataObject)
        {
            WalletSemaphore.WaitOne();

            try
            {
                TotalPayableBalance += tradeAllocatedBalanceDataObject.AllocatedQuoteBalance;
            }
            catch
            {

            }
            finally
            {
                WalletSemaphore.Release();
            }
        }

        public IList<TradeAllocatedBalanceAuxiliary> SplitTradeAllocatedBalance(TradeAllocatedBalanceAuxiliary tradeAllocatedBalanceDataObject, int splitCount)
        {
            var result = new List<TradeAllocatedBalanceAuxiliary>();

            var allocatedQuoteBalance = tradeAllocatedBalanceDataObject.AllocatedQuoteBalance / splitCount;

            for (var index = 0; index < splitCount; index++)
            {
                var newtradeAllocatedBalance = new TradeAllocatedBalanceAuxiliary(tradeAllocatedBalanceDataObject.Position, allocatedQuoteBalance, Leverage, MarketFeePercent);

                newtradeAllocatedBalance.SetEnterPrice(tradeAllocatedBalanceDataObject.EnterPrice);

                result.Add(newtradeAllocatedBalance);
            }

            return result;
        }

        public void EstimateFinalWalletByTradeAllocatedBalances(IEnumerable<TradeAllocatedBalanceAuxiliary> pendingForEnterTradeAllocatedBalances, IEnumerable<TradeAllocatedBalanceAuxiliary> notPendingForEnterTradeAllocatedBalances, float currentPrice, out float totalSavedProfit, out float totalPaiedMarketFee, out float totalWalletBalance, out int wonTradeCount, out float wonTradePercent)
        {
            var totalSuccessfulTradeCount = TotalSuccessfulTradeCount;
            var totalPayableBalance = TotalPayableBalance;

            totalSavedProfit = TotalSavedProfit;
            totalPaiedMarketFee = TotalPaiedMarketFee;
            wonTradeCount = WonTradeCount;

            foreach (var tradeAllocated in pendingForEnterTradeAllocatedBalances)
            {
                totalPayableBalance += tradeAllocated.AllocatedQuoteBalance;
            }

            foreach (var tradeAllocated in notPendingForEnterTradeAllocatedBalances)
            {
                //
                float profit;
                float totalMarketFee;
                float deallocatedQuoteBalance;

                tradeAllocated.EstimateByExitPrice(currentPrice, out profit, out totalMarketFee, out deallocatedQuoteBalance);

                //
                EstimateSuccessfulTrade(totalMarketFee, profit, deallocatedQuoteBalance, ref totalSuccessfulTradeCount, ref wonTradeCount, ref totalPaiedMarketFee, ref totalSavedProfit, ref totalPayableBalance);
            }

            totalWalletBalance = totalSavedProfit + totalPayableBalance;
            wonTradePercent = ((float)wonTradeCount) / ((float)totalSuccessfulTradeCount) * 100f;
        }
    }
}

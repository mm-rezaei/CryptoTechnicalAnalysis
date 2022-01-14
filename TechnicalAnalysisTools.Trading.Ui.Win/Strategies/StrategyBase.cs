using Binance.Net.Objects.Spot.MarketStream;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Trading.Ui.Win.Delegates;
using TechnicalAnalysisTools.Trading.Ui.Win.Services;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Strategies
{
    internal abstract class StrategyBase
    {
        public StrategyBase(BinanceSpotClientService binanceSpotClient, SymbolTypes symbol, decimal usdtAmount)
        {
            BinanceSpotClient = binanceSpotClient;
            Symbol = symbol;
            UsdtAmount = usdtAmount;
        }

        protected BinanceSpotClientService BinanceSpotClient { get; }

        protected decimal UsdtAmount { get; }

        public SymbolTypes Symbol { get; }

        protected void OnLogReceived(string log)
        {
            LogReceived?.Invoke(log);
        }

        public abstract void CheckPrice(BinanceStreamTick candle);

        public event LogReceivedHandler LogReceived;
    }
}

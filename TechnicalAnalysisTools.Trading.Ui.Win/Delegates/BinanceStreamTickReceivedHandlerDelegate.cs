using Binance.Net.Objects.Spot.MarketStream;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Delegates
{
    internal delegate void BinanceStreamTickReceivedHandler(SymbolTypes symbol, BinanceStreamTick tick);
}

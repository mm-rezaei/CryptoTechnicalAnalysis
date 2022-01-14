using Binance.Net.Objects.Spot.MarketData;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Delegates
{
    public delegate void BinanceKlineUpdatesReceivedHandler(SymbolTypes symbol, BinanceSpotKline kline);
}

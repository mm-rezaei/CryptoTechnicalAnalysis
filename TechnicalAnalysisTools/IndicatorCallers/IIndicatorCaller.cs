using Ecng.Serialization;
using StockSharp.Algo.Candles;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public interface IIndicatorCaller
    {
        bool ProcessCandle(Candle candle, CandleDataModel candleDataModel);

        void LoadSetting(SettingsStorage settings);

        SettingsStorage SaveSetting();
    }
}

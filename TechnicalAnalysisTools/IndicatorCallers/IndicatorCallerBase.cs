using Ecng.Serialization;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public abstract class IndicatorCallerBase<TIndicatorType> : IIndicatorCaller where TIndicatorType : IIndicator
    {
        protected IndicatorCallerBase(TIndicatorType indicator)
        {
            Indicator = indicator;
        }

        protected TIndicatorType Indicator { get; set; }

        protected abstract bool OnProcess(Candle candle, CandleDataModel candleDataModel);

        public bool ProcessCandle(Candle candle, CandleDataModel candleDataModel)
        {
            return OnProcess(candle, candleDataModel);
        }

        public void LoadSetting(SettingsStorage settings)
        {
            Indicator.Load(settings);
        }

        public SettingsStorage SaveSetting()
        {
            return Indicator.Save();
        }
    }
}

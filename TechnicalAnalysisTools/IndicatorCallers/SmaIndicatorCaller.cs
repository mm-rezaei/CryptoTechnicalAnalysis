using System.Linq;
using System.Reflection;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class SmaIndicatorCaller : IndicatorCallerBase<SmaIndicator>
    {
        public SmaIndicatorCaller(SmaIndicator indicator) : base(indicator)
        {
            var fieldName = string.Format("Sma{0}Value", indicator.Length);

            SmaValueFieldInfo = typeof(CandleDataModel).GetFields(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == fieldName).FirstOrDefault();
        }

        private FieldInfo SmaValueFieldInfo { get; set; } = null;

        protected override bool OnProcess(Candle candle, CandleDataModel candleDataModel)
        {
            var result = false;

            if (SmaValueFieldInfo != null)
            {
                var indicatorValue = Indicator.Process(candle.ClosePrice, candle.State == StockSharp.Messages.CandleStates.Finished);

                if (Indicator.IsFormed)
                {
                    if (!indicatorValue.IsEmpty)
                    {
                        SmaValueFieldInfo.SetValue(candleDataModel, indicatorValue.GetValue<float>());

                        result = true;
                    }
                }
                else
                {
                    SmaValueFieldInfo.SetValue(candleDataModel, 0);

                    result = true;
                }
            }

            return result;
        }
    }
}


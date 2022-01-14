using System.Linq;
using System.Reflection;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using TechnicalAnalysisTools.Indicators;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.IndicatorCallers
{
    public class EmaIndicatorCaller : IndicatorCallerBase<EmaIndicator>
    {
        public EmaIndicatorCaller(EmaIndicator indicator) : base(indicator)
        {
            var fieldName = string.Format("Ema{0}Value", indicator.Length);

            EmaValueFieldInfo = typeof(CandleDataModel).GetFields(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == fieldName).FirstOrDefault();
        }

        private FieldInfo EmaValueFieldInfo { get; set; } = null;

        protected override bool OnProcess(Candle candle, CandleDataModel candleDataModel)
        {
            var result = false;

            if (EmaValueFieldInfo != null)
            {
                var indicatorValue = Indicator.Process(candle.ClosePrice, candle.State == StockSharp.Messages.CandleStates.Finished);

                if (Indicator.IsFormed)
                {
                    if (!indicatorValue.IsEmpty)
                    {
                        EmaValueFieldInfo.SetValue(candleDataModel, indicatorValue.GetValue<float>());

                        result = true;
                    }
                }
                else
                {
                    EmaValueFieldInfo.SetValue(candleDataModel, 0);

                    result = true;
                }
            }

            return result;
        }
    }
}


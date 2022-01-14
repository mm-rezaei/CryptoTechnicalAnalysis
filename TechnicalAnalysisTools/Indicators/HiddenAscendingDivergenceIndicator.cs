using System;
using System.Collections.Generic;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.Indicators
{
    public class HiddenAscendingDivergenceIndicator : DivergenceIndicatorBase
    {
        public HiddenAscendingDivergenceIndicator(IList<CandleDataModel> candles, Func<CandleDataModel, float> priceFunction, Func<CandleDataModel, float> indicatorValueFunction, float? indicatorValueBaseLine = null, float tolerateLineBreaking = 0, int divergenceCandlesDistance = 6) : base(candles, priceFunction, indicatorValueFunction, indicatorValueBaseLine, tolerateLineBreaking, divergenceCandlesDistance)
        {

        }

        protected override DivergenceTypes DivergenceType
        {
            get
            {
                return DivergenceTypes.HiddenAscending;
            }
        }
    }
}

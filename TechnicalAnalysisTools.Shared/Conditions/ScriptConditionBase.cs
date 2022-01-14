using System;
using System.Collections.Generic;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public abstract class ScriptConditionBase
    {
        protected abstract List<TimeFrames> SupportedTimeFrames { get; }

        protected bool IsThisMinuteCandleFirstTimeFrameCandle(DateTime candleDateTime, TimeFrames timeFrame)
        {
            return TimeFrameHelper.IsThisMinuteCandleFirstTimeFrameCandle(candleDateTime, timeFrame);
        }

        protected bool IsThisMinuteCandleLastTimeFrameCandle(DateTime candleDateTime, TimeFrames timeFrame)
        {
            return TimeFrameHelper.IsThisMinuteCandleLastTimeFrameCandle(candleDateTime, timeFrame);
        }

        protected abstract bool CalculateByScript(OperationCandleRequestedHandler operationCandleRequested, SymbolTypes symbol, TimeFrames timeFrame, int candleNumber);

        public bool Calculate(OperationCandleRequestedHandler operationCandleRequested, SymbolTypes symbol, TimeFrames timeFrame, int candleNumber)
        {
            var result = false;

            if (SupportedTimeFrames.Contains(timeFrame))
            {
                result = CalculateByScript(operationCandleRequested, symbol, timeFrame, candleNumber);
            }

            return result;
        }
    }
}

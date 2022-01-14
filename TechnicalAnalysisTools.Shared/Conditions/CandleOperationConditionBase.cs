using System;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    public abstract class CandleOperationConditionBase : ConditionBase
    {
        static CandleOperationConditionBase()
        {
            FieldInfos = typeof(CandleDataModel).GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
        }

        static FieldInfo[] FieldInfos { get; }

        public CandleOperationConditionBase(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber)
        {
            Symbol = symbol;

            TimeFrame = timeFrame;

            CandleNumber = candleNumber;
        }

        private protected TimeFrames TimeFrame { get; set; }

        private protected int CandleNumber { get; set; }

        public SymbolTypes Symbol { get; private set; }

        private protected TimeFrames GetTimeFrame(TimeFrames? timeFrame)
        {
            if (timeFrame.HasValue)
            {
                return timeFrame.Value;
            }
            else
            {
                return TimeFrame;
            }
        }

        private protected int GetCandleNumber(int? candleNumber)
        {
            if (candleNumber.HasValue)
            {
                return candleNumber.Value;
            }
            else
            {
                return CandleNumber;
            }
        }

        private protected CandleDataModel RequestOperationCandle(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame, int? candleNumber, int plusCandleNumber = 0)
        {
            CandleDataModel result = null;

            if (operationCandleRequested == null)
            {
                if (OperationCandleRequested != null)
                {
                    result = OperationCandleRequested(Symbol, GetTimeFrame(timeFrame), GetCandleNumber(candleNumber) + plusCandleNumber);
                }
            }
            else
            {
                result = operationCandleRequested(Symbol, GetTimeFrame(timeFrame), GetCandleNumber(candleNumber) + plusCandleNumber);
            }

            return result;
        }

        protected bool IsThisMinuteCandleFirstTimeFrameCandle(DateTime candleDateTime, TimeFrames timeFrame)
        {
            return TimeFrameHelper.IsThisMinuteCandleFirstTimeFrameCandle(candleDateTime, timeFrame);
        }

        protected bool IsThisMinuteCandleLastTimeFrameCandle(DateTime candleDateTime, TimeFrames timeFrame)
        {
            return TimeFrameHelper.IsThisMinuteCandleLastTimeFrameCandle(candleDateTime, timeFrame);
        }

        private protected object ReadFieldValue(CandleDataModel candle, string fieldName)
        {
            object result = null;

            var fieldInfo = FieldInfos.Where(p => p.Name == fieldName).First();

            if (fieldInfo != null)
            {
                result = fieldInfo.GetValue(candle);
            }

            return result;
        }
    }
}

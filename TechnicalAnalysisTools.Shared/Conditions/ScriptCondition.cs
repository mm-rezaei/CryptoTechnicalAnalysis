using System;
using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Conditions
{
    [OperationParameter(0, "Script Name", typeof(string))]
    public class ScriptCondition : CandleOperationConditionBase
    {
        public ScriptCondition(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber, string scriptName) : base(symbol, timeFrame, candleNumber)
        {
            try
            {
                var dllPath = scriptName + ".dll";

                var assembly = AssemblyHelper.GetAssembly(dllPath);

                if (assembly != null)
                {
                    var scriptConditionType = assembly.GetType("TechnicalAnalysisTools.ScriptCondition.ScriptCondition");

                    if (scriptConditionType != null)
                    {
                        var scriptConditionObject = Activator.CreateInstance(scriptConditionType);

                        if (scriptConditionObject != null && scriptConditionObject is ScriptConditionBase)
                        {
                            ScriptConditionObject = (ScriptConditionBase)scriptConditionObject;
                        }
                    }
                }
            }
            catch
            {
                ScriptConditionObject = null;
            }
        }

        private ScriptConditionBase ScriptConditionObject { get; set; }

        public override bool Calculate(OperationCandleRequestedHandler operationCandleRequested, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            bool result = false;

            AreNeededCandlesAvailable = true;

            try
            {
                if (ScriptConditionObject != null)
                {
                    var targetOperationCandleRequested = OperationCandleRequested;
                    var targetSymbol = Symbol;
                    var targetTimeFrame = TimeFrame;
                    var targetCandleNumber = CandleNumber;

                    if (operationCandleRequested != null)
                    {
                        targetOperationCandleRequested = operationCandleRequested;
                    }

                    if (timeFrame.HasValue)
                    {
                        targetTimeFrame = timeFrame.Value;
                    }

                    if (candleNumber.HasValue)
                    {
                        targetCandleNumber = candleNumber.Value;
                    }

                    result = ScriptConditionObject.Calculate(targetOperationCandleRequested, targetSymbol, targetTimeFrame, targetCandleNumber);
                }
            }
            catch
            {
                result = false;
            }

            OnConditionResultEvaluated(result);

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TechnicalAnalysisTools.Shared.Conditions;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public static class AlarmHelper
    {
        private static AlarmItemDataModel GetModifiledAlarmItemDataModel(AlarmItemDataModel item)
        {
            AlarmItemDataModel result = item;

            if (OperationConditionHelper.IsLogicalOperation(item.Operation))
            {
                for (var index = item.Items.Count - 1; index >= 0; index--)
                {
                    var modifiedItem = GetModifiledAlarmItemDataModel(item.Items[index]);

                    if (modifiedItem == null)
                    {
                        item.Items.RemoveAt(index);
                    }
                }

                if (item.Items.Count == 0)
                {
                    result = null;
                }
            }

            return result;
        }

        private static string GetAlarmItemString(AlarmItemDataModel item, int indentation)
        {
            var result = "";

            if (item != null)
            {
                //
                var indentationText = "";
                var indentationPlusText = "\t";

                for (var index = 0; index < indentation; index++)
                {
                    indentationText += "\t";
                    indentationPlusText += "\t";
                }

                //
                if (OperationConditionHelper.IsNonLogicalOperation(item.Operation))
                {
                    result += "-" + item.Symbol.ToString() + ":" + item.CandleNumber + ":" + item.TimeFrame.ToString() + ":" + item.Operation;
                }
                else
                {
                    result += "-" + item.Operation;
                }

                var parameters = OperationConditionHelper.GetOperationParameter(item.Operation);

                if (parameters.Length != 0 && parameters.Length == item.Parameters.Count)
                {
                    result += " (";

                    var parametersText = "";

                    for (var index = 0; index < parameters.Length; index++)
                    {
                        if (parametersText != "")
                        {
                            parametersText += ", ";
                        }

                        parametersText += parameters[index].Name + "=" + item.Parameters[index].ToString();
                    }

                    result += parametersText;

                    result += ")";
                }

                if (OperationConditionHelper.IsLogicalOperation(item.Operation))
                {
                    result += Environment.NewLine;

                    result += indentationText + "{" + Environment.NewLine;

                    foreach (var child in item.Items)
                    {
                        var childText = GetAlarmItemString(child, indentation + 1);

                        if (childText != "")
                        {
                            result += indentationPlusText + childText + Environment.NewLine;
                        }
                    }

                    result += indentationText + "}";
                }

            }

            return result;
        }

        private static object[] ParseParameterValues(string parametersText, ConditionOperations operation)
        {
            object[] result = null;

            var operationParameters = OperationConditionHelper.GetOperationParameter(operation);

            var parameterParts = parametersText.Split(',').Select(p => p.Trim()).ToArray();

            if (operationParameters.Length == parameterParts.Length)
            {
                result = new object[operationParameters.Length];

                for (var parameterIndex = 0; parameterIndex < parameterParts.Length; parameterIndex++)
                {
                    if (parameterParts[parameterIndex].Split('=').Length == 2)
                    {
                        var parameterText = parameterParts[parameterIndex].Split('=')[1].Trim();

                        var parameterType = operationParameters[parameterIndex].Type;

                        if (parameterType.IsEnum)
                        {
                            object value = null;

                            try
                            {
                                value = Enum.Parse(parameterType, parameterText);
                            }
                            catch
                            {
                                result = null;

                                break;
                            }

                            result[parameterIndex] = value;
                        }
                        else if (parameterType == typeof(string))
                        {
                            if (!string.IsNullOrWhiteSpace(parameterText) && !parameterText.Contains(" "))
                            {
                                result[parameterIndex] = parameterText;
                            }
                            else
                            {
                                result = null;

                                break;
                            }
                        }
                        else if (parameterType == typeof(int))
                        {
                            int value;

                            if (int.TryParse(parameterText, out value))
                            {
                                result[parameterIndex] = value;
                            }
                            else
                            {
                                result = null;

                                break;
                            }
                        }
                        else if (parameterType == typeof(float))
                        {
                            float value;

                            if (float.TryParse(parameterText, out value))
                            {
                                result[parameterIndex] = value;
                            }
                            else
                            {
                                result = null;

                                break;
                            }
                        }
                        else if (parameterType == typeof(byte))
                        {
                            byte value;

                            if (byte.TryParse(parameterText, out value))
                            {
                                result[parameterIndex] = value;
                            }
                            else
                            {
                                result = null;

                                break;
                            }
                        }
                        else
                        {
                            result = null;

                            break;
                        }
                    }
                    else
                    {
                        result = null;

                        break;
                    }
                }
            }

            return result;
        }

        private static AlarmItemDataModel ParseAlarmItemDataModel(string operationLine, ConditionOperations[] validOperations, AlarmItemDataModel parent)
        {
            AlarmItemDataModel result = null;

            if (operationLine[0] == '-')
            {
                string operationText = "";
                string parametersText = "";

                if (operationLine.Contains("("))
                {
                    var operationParts = operationLine.Substring(1).Replace(")", "").Split('(').Select(p => p.Trim()).ToArray();

                    if (operationParts.Length == 2 && operationParts[1].Length != 0)
                    {
                        operationText = operationParts[0];
                        parametersText = operationParts[1];
                    }
                }
                else
                {
                    operationText = operationLine.Substring(1);
                    parametersText = "";
                }

                if (operationText != "")
                {
                    ConditionOperations operationValue;

                    if (Enum.TryParse(operationText, out operationValue))
                    {
                        if (validOperations.Contains(operationValue))
                        {
                            //
                            object[] parameterValues = null;

                            if (parametersText != "")
                            {
                                parameterValues = ParseParameterValues(parametersText, operationValue);
                            }

                            //
                            bool resultShouldBeCreate;

                            var operationParametersCount = OperationConditionHelper.GetOperationParameter(operationValue).Length;

                            if (parameterValues == null)
                            {
                                if (operationParametersCount == 0)
                                {
                                    resultShouldBeCreate = true;
                                }
                                else
                                {
                                    resultShouldBeCreate = false;
                                }
                            }
                            else
                            {
                                if (operationParametersCount == parameterValues.Length)
                                {
                                    resultShouldBeCreate = true;
                                }
                                else
                                {
                                    resultShouldBeCreate = false;
                                }
                            }

                            //
                            if (resultShouldBeCreate)
                            {
                                result = new AlarmItemDataModel(parent)
                                {
                                    Operation = operationValue
                                };

                                if (parameterValues != null)
                                {
                                    foreach (var p in parameterValues)
                                    {
                                        result.Parameters.Add(p);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static int GetClosedBraceIndex(string[] lines, int index)
        {
            var result = index;

            if (index < lines.Length)
            {
                var stack = new Stack<string>();

                if (lines[index] == "{")
                {
                    stack.Push("{");

                    for (index++; index < lines.Length; index++)
                    {
                        if (lines[index] == "{")
                        {
                            stack.Push("{");
                        }
                        else if (lines[index] == "}")
                        {
                            stack.Pop();

                            if (stack.Count == 0)
                            {
                                result = index;

                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static AlarmItemDataModel GetAlarmItemDataModel(string[] lines, AlarmItemDataModel parent)
        {
            AlarmItemDataModel result = null;

            if (lines.Length > 0)
            {
                if (lines[0].Contains(":"))
                {
                    if (lines.Length == 1 && lines[0][0] == '-')
                    {
                        var parts = lines[0].Substring(1).Split(':').ToArray();

                        var symbolText = parts[0];
                        var candleNumberText = parts[1];
                        var timeFrameText = parts[2];

                        SymbolTypes symbolValue;
                        int candleNumberValue;
                        TimeFrames timeFrameValue;

                        if (Enum.TryParse(symbolText, out symbolValue) && int.TryParse(candleNumberText, out candleNumberValue) && Enum.TryParse(timeFrameText, out timeFrameValue))
                        {
                            result = ParseAlarmItemDataModel("-" + parts[3], OperationConditionHelper.NonLogicalOperations, parent);

                            if (result != null)
                            {
                                result.Symbol = symbolValue;
                                result.TimeFrame = timeFrameValue;
                                result.CandleNumber = candleNumberValue;
                            }
                        }
                    }
                }
                else
                {
                    if (lines.Length >= 4)
                    {
                        result = ParseAlarmItemDataModel(lines[0], OperationConditionHelper.LogicalOperations, parent);

                        var closedBraceIndex = GetClosedBraceIndex(lines, 1);

                        if (closedBraceIndex == lines.Length - 1)
                        {
                            var bodyLines = lines.Skip(2).Take(lines.Length - 3).ToArray();

                            if (bodyLines.Length > 0)
                            {
                                for (var index = 0; index < bodyLines.Length; index++)
                                {
                                    if (bodyLines[index].Contains(":"))
                                    {
                                        var childLines = bodyLines.Skip(index).Take(1).ToArray();

                                        var child = GetAlarmItemDataModel(childLines, result);

                                        if (child != null)
                                        {
                                            result.Items.Add(child);
                                        }
                                        else
                                        {
                                            result = null;

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        var childClosedBraceIndex = GetClosedBraceIndex(bodyLines, index + 1);

                                        if (childClosedBraceIndex != index + 1)
                                        {
                                            var childLines = bodyLines.Skip(index).Take(childClosedBraceIndex - index + 1).ToArray();

                                            var child = GetAlarmItemDataModel(childLines, result);

                                            if (child != null)
                                            {
                                                result.Items.Add(child);
                                            }
                                            else
                                            {
                                                result = null;

                                                break;
                                            }

                                            index = childClosedBraceIndex;
                                        }
                                        else
                                        {
                                            result = null;

                                            break;
                                        }
                                    }
                                }
                            }

                            if (result != null)
                            {
                                if (OperationConditionHelper.UnaryOperations.Contains(result.Operation))
                                {
                                    if (result.Items.Count > 1)
                                    {
                                        result = null;
                                    }
                                }

                                if (OperationConditionHelper.PeriodicOperations.Contains(result.Operation))
                                {
                                    if (OperationConditionHelper.NumberPeriodicOperations.Contains(result.Operation))
                                    {
                                        var isParentNumberPeriodicOperation = false;

                                        var currentParent = result.Parent;

                                        while (currentParent != null)
                                        {
                                            if (OperationConditionHelper.NumberPeriodicOperations.Contains(currentParent.Operation))
                                            {
                                                isParentNumberPeriodicOperation = true;

                                                break;
                                            }

                                            currentParent = currentParent.Parent;
                                        }

                                        if (isParentNumberPeriodicOperation)
                                        {
                                            result = null;
                                        }
                                    }
                                    else if (OperationConditionHelper.TimeFramePeriodicOperations.Contains(result.Operation))
                                    {
                                        var isParentTimeFramePeriodicOperation = false;

                                        var currentParent = result.Parent;

                                        while (currentParent != null)
                                        {
                                            if (OperationConditionHelper.TimeFramePeriodicOperations.Contains(currentParent.Operation))
                                            {
                                                isParentTimeFramePeriodicOperation = true;

                                                break;
                                            }

                                            currentParent = currentParent.Parent;
                                        }

                                        if (isParentTimeFramePeriodicOperation)
                                        {
                                            result = null;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = null;
                        }
                    }
                }
            }

            return result;
        }

        public static string ConvertAlarmItemToString(AlarmItemDataModel item, string name, SymbolTypes symbol, PositionTypes position)
        {
            var result = "";

            var modifiedAlarmItem = GetModifiledAlarmItemDataModel((AlarmItemDataModel)item.Clone());

            if (modifiedAlarmItem != null)
            {
                result += "#" + name + Environment.NewLine;
                result += "#" + symbol + Environment.NewLine;
                result += "#" + position + Environment.NewLine;
                result += "#Condition" + Environment.NewLine;
                result += GetAlarmItemString(modifiedAlarmItem, 0);
            }

            return result;
        }

        public static AlarmItemDataModel ConvertStringToAlarmItem(string text, ref string name, ref SymbolTypes symbol, ref PositionTypes position)
        {
            AlarmItemDataModel result = null;

            if (!string.IsNullOrWhiteSpace(text))
            {
                var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(p => p.Replace("\t", " ").Trim().Replace("  ", " ")).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

                if (lines.Length >= 8)
                {
                    if (lines[0][0] == '#' && lines[1][0] == '#' && lines[2][0] == '#' && lines[3] == "#Condition")
                    {
                        var nameText = lines[0].Substring(1);
                        var symbolText = lines[1].Substring(1);
                        var positionText = lines[2].Substring(1);

                        SymbolTypes symbolValue;
                        PositionTypes positionValue;

                        if (Enum.TryParse(symbolText, out symbolValue) && Enum.TryParse(positionText, out positionValue))
                        {
                            var alarmItem = GetAlarmItemDataModel(lines.Skip(4).ToArray(), null);

                            if (alarmItem != null)
                            {
                                name = nameText;
                                symbol = symbolValue;
                                position = positionValue;

                                result = alarmItem;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static AlarmItemDataModel ExpandPeriodicAlarmItem(AlarmItemDataModel alarmItem, AlarmItemDataModel parent, TimeFrames? timeFrame = null, int? candleNumber = null)
        {
            AlarmItemDataModel result;

            if (OperationConditionHelper.TimeFramePeriodicOperations.Contains(alarmItem.Operation))
            {
                //
                ConditionOperations operation;

                switch (alarmItem.Operation)
                {
                    case ConditionOperations.AndForTimeFrames:
                        operation = ConditionOperations.And;
                        break;
                    case ConditionOperations.OrForTimeFrames:
                        operation = ConditionOperations.Or;
                        break;
                    case ConditionOperations.TrueCountForTimeFrames:
                        operation = ConditionOperations.TrueCount;
                        break;
                    case ConditionOperations.FalseCountForTimeFrames:
                        operation = ConditionOperations.FalseCount;
                        break;
                    default:
                        throw new Exception("TimeFramePeriodic operation is not valid.");
                }

                //
                var lowerTimeFrame = (TimeFrames)alarmItem.Parameters[0];
                var upperTimeFrame = (TimeFrames)alarmItem.Parameters[1];

                result = new AlarmItemDataModel(parent) { Operation = operation };

                foreach (var value in alarmItem.Parameters.Skip(2))
                {
                    result.Parameters.Add(value);
                }

                foreach (var i in alarmItem.Items)
                {
                    foreach (var selectedTimeFrame in OperationConditionHelper.TimeFrames.Where(p => p >= lowerTimeFrame && p <= upperTimeFrame))
                    {
                        var item = ExpandPeriodicAlarmItem(i, result, selectedTimeFrame, candleNumber);

                        result.Items.Add(item);
                    }
                }
            }
            else if (OperationConditionHelper.NumberPeriodicOperations.Contains(alarmItem.Operation))
            {
                //
                ConditionOperations operation;

                switch (alarmItem.Operation)
                {
                    case ConditionOperations.AndForNumbers:
                        operation = ConditionOperations.And;
                        break;
                    case ConditionOperations.OrForNumbers:
                        operation = ConditionOperations.Or;
                        break;
                    case ConditionOperations.TrueCountForNumbers:
                        operation = ConditionOperations.TrueCount;
                        break;
                    case ConditionOperations.FalseCountForNumbers:
                        operation = ConditionOperations.FalseCount;
                        break;
                    default:
                        throw new Exception("NumberPeriodic operation is not valid.");
                }

                //
                var lowerNumber = (int)alarmItem.Parameters[0];
                var upperNumber = (int)alarmItem.Parameters[1];

                result = new AlarmItemDataModel(parent) { Operation = operation };

                foreach (var value in alarmItem.Parameters.Skip(2))
                {
                    result.Parameters.Add(value);
                }

                if (lowerNumber <= upperNumber)
                {
                    foreach (var i in alarmItem.Items)
                    {
                        foreach (var selectedNumber in Enumerable.Range(lowerNumber, upperNumber - lowerNumber + 1))
                        {
                            var item = ExpandPeriodicAlarmItem(i, result, timeFrame, selectedNumber);

                            result.Items.Add(item);
                        }
                    }
                }
            }
            else if (OperationConditionHelper.LogicalOperations.Contains(alarmItem.Operation))
            {
                result = new AlarmItemDataModel(parent) { Operation = alarmItem.Operation };

                foreach (var value in alarmItem.Parameters)
                {
                    result.Parameters.Add(value);
                }

                foreach (var i in alarmItem.Items)
                {
                    var item = ExpandPeriodicAlarmItem(i, result, timeFrame, candleNumber);

                    result.Items.Add(item);
                }
            }
            else if (OperationConditionHelper.NonLogicalOperations.Contains(alarmItem.Operation))
            {
                var formatter = new BinaryFormatter();

                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, alarmItem);

                    stream.Seek(0, SeekOrigin.Begin);

                    result = (AlarmItemDataModel)formatter.Deserialize(stream);
                }

                result.Id = Guid.NewGuid();

                result.Parent = parent;

                if (timeFrame.HasValue)
                {
                    result.TimeFrame = timeFrame.Value;
                }

                if (candleNumber.HasValue)
                {
                    result.CandleNumber = candleNumber.Value;
                }

                result.Parameters.Clear();
                result.Items.Clear();

                foreach (var p in alarmItem.Parameters)
                {
                    result.Parameters.Add(p);
                }
            }
            else
            {
                throw new Exception("Operation is not valid.");
            }

            return result;
        }

        public static ICondition CreateConditionFromAlarmItemDataModel(AlarmItemDataModel item, bool shortCircuit, OperationCandleRequestedHandler operationCandleRequested, ConditionResultEvaluatedHandler conditionResultEvaluated = null)
        {
            ICondition result = null;

            var type = Type.GetType("TechnicalAnalysisTools.Shared.Conditions." + item.Operation + "Condition");

            if (OperationConditionHelper.IsLogicalOperation(item.Operation))
            {
                //
                var conditions = new ICondition[item.Items.Count];

                for (var index = 0; index < item.Items.Count; index++)
                {
                    conditions[index] = CreateConditionFromAlarmItemDataModel(item.Items[index], shortCircuit, operationCandleRequested, conditionResultEvaluated);
                }

                //
                var parameters = new object[item.Parameters.Count + 1];

                parameters[0] = conditions;

                for (var index = 0; index < item.Parameters.Count; index++)
                {
                    parameters[index + 1] = item.Parameters[index];
                }

                try
                {
                    result = (ICondition)Activator.CreateInstance(type, parameters);

                    result.Id = item.Id;

                    if (conditionResultEvaluated != null)
                    {
                        result.ConditionResultEvaluated += conditionResultEvaluated;
                    }
                }
                catch
                {
                    result = null;
                }

                if (result != null)
                {
                    if (result is LogicalOperationConditionBase)
                    {
                        var logicalOperationCondition = result as LogicalOperationConditionBase;

                        logicalOperationCondition.SetShortCircuit(shortCircuit);
                    }
                }
            }
            else
            {
                var parameters = new object[item.Parameters.Count + 3];

                parameters[0] = item.Symbol;
                parameters[1] = item.TimeFrame;
                parameters[2] = item.CandleNumber;

                for (var index = 0; index < item.Parameters.Count; index++)
                {
                    parameters[index + 3] = item.Parameters[index];
                }

                try
                {
                    result = (ICondition)Activator.CreateInstance(type, parameters);

                    result.Id = item.Id;

                    result.OperationCandleRequested += operationCandleRequested;

                    if (conditionResultEvaluated != null)
                    {
                        result.ConditionResultEvaluated += conditionResultEvaluated;
                    }
                }
                catch
                {
                    result = null;
                }
            }

            return result;
        }

        public static SymbolTypes[] GetNeededSymbols(ICondition condition)
        {
            var result = new List<SymbolTypes>();

            if (condition != null)
            {
                var stack = new Stack<ICondition>();

                stack.Push(condition);

                while (stack.Count != 0)
                {
                    //
                    var currentCondition = stack.Pop();

                    //
                    if (currentCondition is LogicalOperationConditionBase)
                    {
                        foreach (var child in ((LogicalOperationConditionBase)currentCondition).Conditions)
                        {
                            stack.Push(child);
                        }

                    }
                    else if (currentCondition is CandleOperationConditionBase)
                    {
                        var currentCandleCondition = currentCondition as CandleOperationConditionBase;

                        if (!result.Contains(currentCandleCondition.Symbol))
                        {
                            result.Add(currentCandleCondition.Symbol);
                        }
                    }
                    else
                    {
                        throw new Exception("Condition is not valid.");
                    }
                }
            }

            return result.ToArray();
        }
    }
}

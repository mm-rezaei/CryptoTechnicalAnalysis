using System;
using System.Linq;
using TechnicalAnalysisTools.Shared.Attributes;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public static class OperationConditionHelper
    {
        private static SymbolTypes[] symbols { get; } = (SymbolTypes[])Enum.GetValues(typeof(SymbolTypes));

        private static TimeFrames[] timeFrames { get; } = TimeFrameHelper.TimeFramesList;

        private static PositionTypes[] positions { get; } = (PositionTypes[])Enum.GetValues(typeof(PositionTypes));

        private static int[] candleNumbers { get; } = Enumerable.Range(0, 51).ToArray();

        private static ConditionOperations[] unaryOperations { get; } = new ConditionOperations[] {
                ConditionOperations.AndForNumbers,
                ConditionOperations.AndForTimeFrames,
                ConditionOperations.OrForNumbers,
                ConditionOperations.OrForTimeFrames,
                ConditionOperations.TrueCountForNumbers,
                ConditionOperations.TrueCountForTimeFrames,
                ConditionOperations.FalseCountForNumbers,
                ConditionOperations.FalseCountForTimeFrames,
                ConditionOperations.Not };

        private static ConditionOperations[] periodicOperations { get; } = new ConditionOperations[] {
                ConditionOperations.AndForNumbers,
                ConditionOperations.AndForTimeFrames,
                ConditionOperations.OrForNumbers,
                ConditionOperations.OrForTimeFrames,
                ConditionOperations.TrueCountForNumbers,
                ConditionOperations.TrueCountForTimeFrames,
                ConditionOperations.FalseCountForNumbers,
                ConditionOperations.FalseCountForTimeFrames };

        private static ConditionOperations[] numberPeriodicOperations { get; } = new ConditionOperations[] {
                ConditionOperations.AndForNumbers,
                ConditionOperations.OrForNumbers,
                ConditionOperations.TrueCountForNumbers,
                ConditionOperations.FalseCountForNumbers };

        private static ConditionOperations[] timeFramePeriodicOperations { get; } = new ConditionOperations[] {
                ConditionOperations.AndForTimeFrames,
                ConditionOperations.OrForTimeFrames,
                ConditionOperations.TrueCountForTimeFrames,
                ConditionOperations.FalseCountForTimeFrames };

        private static ConditionOperations[] logicalOperations { get; } = new ConditionOperations[] {
                ConditionOperations.And,
                ConditionOperations.AndForNumbers,
                ConditionOperations.AndForTimeFrames,
                ConditionOperations.Or,
                ConditionOperations.OrForNumbers,
                ConditionOperations.OrForTimeFrames,
                ConditionOperations.TrueCount,
                ConditionOperations.TrueCountForNumbers,
                ConditionOperations.TrueCountForTimeFrames,
                ConditionOperations.FalseCount,
                ConditionOperations.FalseCountForNumbers,
                ConditionOperations.FalseCountForTimeFrames,
                ConditionOperations.Not };

        private static ConditionOperations[] nonLogicalOperations { get; set; }

        public static SymbolTypes[] Symbols
        {
            get
            {
                return symbols;
            }
        }

        public static TimeFrames[] TimeFrames
        {
            get
            {
                return timeFrames;
            }
        }

        public static PositionTypes[] Positions
        {
            get
            {
                return positions;
            }
        }

        public static int[] CandleNumbers
        {
            get
            {
                return candleNumbers;
            }
        }

        public static ConditionOperations[] UnaryOperations
        {
            get
            {
                return unaryOperations;
            }
        }

        public static ConditionOperations[] PeriodicOperations
        {
            get
            {
                return periodicOperations;
            }
        }

        public static ConditionOperations[] NumberPeriodicOperations
        {
            get
            {
                return numberPeriodicOperations;
            }
        }

        public static int CandleNumberOrdinalOfConstructor
        {
            get { return 2; }
        }

        public static ConditionOperations[] TimeFramePeriodicOperations
        {
            get
            {
                return timeFramePeriodicOperations;
            }
        }

        public static int TimeFrameOrdinalOfConstructor
        {
            get { return 1; }
        }

        public static ConditionOperations[] LogicalOperations
        {
            get
            {
                return logicalOperations;
            }
        }

        public static ConditionOperations[] NonLogicalOperations
        {
            get
            {
                if (nonLogicalOperations == null)
                {
                    var operations = LogicalOperations;

                    nonLogicalOperations = ((ConditionOperations[])Enum.GetValues(typeof(ConditionOperations))).Where(p => operations.All(lp => p != lp)).ToArray();
                }

                return nonLogicalOperations;
            }
        }

        public static SymbolTypes GetDefaultSymbol
        {
            get
            {
                return Symbols[0];
            }
        }

        public static TimeFrames DefaultTimeFrame
        {
            get
            {
                return TimeFrames[0];
            }
        }

        public static PositionTypes DefaultPosition
        {
            get
            {
                return Positions[0];
            }
        }

        public static int DefaultCandleNumber
        {
            get
            {
                return CandleNumbers[0];
            }
        }

        public static ConditionOperations DefaultLogicalOperation
        {
            get
            {
                return LogicalOperations[0];
            }
        }

        public static ConditionOperations DefaultNonLogicalOperation
        {
            get
            {
                return NonLogicalOperations[0];
            }
        }

        public static bool IsLogicalOperation(ConditionOperations operation)
        {
            return LogicalOperations.Any(p => p == operation);
        }

        public static bool IsNonLogicalOperation(ConditionOperations operation)
        {
            return NonLogicalOperations.Any(p => p == operation);
        }

        public static OperationParameterAttribute[] GetOperationParameter(ConditionOperations operation)
        {
            var classType = Type.GetType(string.Format("TechnicalAnalysisTools.Shared.Conditions.{0}Condition", operation.ToString()));

            return classType.GetCustomAttributes(true).Where(p => p is OperationParameterAttribute).Select(p => p as OperationParameterAttribute).OrderBy(p => p.Ordinal).ToArray();
        }
    }
}

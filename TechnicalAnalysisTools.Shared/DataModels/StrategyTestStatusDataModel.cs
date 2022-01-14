using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class StrategyTestStatusDataModel
    {
        static StrategyTestStatusDataModel()
        {
            PropertyInfos = typeof(StrategyTestStatusDataModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToArray();
        }

        private static PropertyInfo[] PropertyInfos { get; set; }

        public Guid Id { get; set; }

        public List<CandleDataModel> Candles { get; set; } = new List<CandleDataModel>();

        public StrategyTestStatusTypes StrategyTestStatusType { get; set; }

        public List<StrategyTestLogDataModel> StrategyTestLogs { get; set; } = new List<StrategyTestLogDataModel>();

        public List<StrategyTestOrderDataModel> StrategyTestOrders { get; set; } = new List<StrategyTestOrderDataModel>();

        public StrategyTestReportDataModel StrategyTestReport { get; set; } = new StrategyTestReportDataModel();

        public float TotalBalance { get; set; }

        public float Progress { get; set; }

        public string Message { get; set; }

        public static string FieldNamesToString()
        {
            var result = "";

            foreach (var property in PropertyInfos)
            {
                if (property.Name == nameof(Candles) || property.Name == nameof(StrategyTestLogs) || property.Name == nameof(StrategyTestOrders) || property.Name == nameof(StrategyTestReport))
                {
                    continue;
                }

                if (result != "")
                {
                    result += ",";
                }

                result += property.Name;
            }

            return result;
        }

        public override string ToString()
        {
            var result = "";

            foreach (var property in PropertyInfos)
            {
                if (property.Name == nameof(Candles) || property.Name == nameof(StrategyTestLogs) || property.Name == nameof(StrategyTestOrders) || property.Name == nameof(StrategyTestReport))
                {
                    continue;
                }

                if (result != "")
                {
                    result += ",";
                }

                result += property.GetValue(this);
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataObjects
{
    [Serializable]
    public class UiClientInitializedDataObject
    {
        public Dictionary<CommandTypes, bool> MenuItemsStatus { get; set; }

        public ServerStatusDataModel ServerStatus { get; set; }

        public List<SymbolAlarmDataModel> Alarms { get; set; }

        public List<SymbolAlarmDataModel> AlarmsHistory { get; set; }

        public List<SymbolTypes> SupportedSymbols { get; set; }

        public List<SymbolDataModel> MarketData { get; set; }
    }
}

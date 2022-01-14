using System.Collections.Generic;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.Shared.Delegates
{
    public delegate void AlarmsReceivedHandler(List<SymbolAlarmDataModel> alarms);
}

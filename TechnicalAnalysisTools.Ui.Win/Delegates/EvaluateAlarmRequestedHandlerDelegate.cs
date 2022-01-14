using System;
using System.Threading.Tasks;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.Ui.Win.Delegates
{
    internal delegate Task<AlarmItemDataModel> EvaluateAlarmRequestedHandler(Guid id, DateTime datetime);
}

using System;
using System.Threading.Tasks;

namespace TechnicalAnalysisTools.Ui.Win.Delegates
{
    internal delegate Task<string> AlarmScriptRequestedHandler(Guid id);
}

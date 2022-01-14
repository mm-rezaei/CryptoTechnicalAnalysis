using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Ui.Win.Delegates
{
    internal delegate Task<List<SymbolDataModel>> LiveHistoryRequestedHandler(DateTime? datetime, SymbolTypes[] symbols);
}

using System;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.Delegates
{
    public delegate bool StrategyTestStatusChangedHandler(Guid sessionId, StrategyTestStatusDataModel strategyTestStatus);
}

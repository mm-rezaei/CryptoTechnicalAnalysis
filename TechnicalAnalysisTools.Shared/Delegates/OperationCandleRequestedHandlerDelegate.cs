using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Delegates
{
    public delegate CandleDataModel OperationCandleRequestedHandler(SymbolTypes symbol, TimeFrames timeFrame, int candleNumber);
}

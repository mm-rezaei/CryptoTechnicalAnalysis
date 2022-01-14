using System.Threading.Tasks;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.Ui.Win.Delegates
{
    internal delegate Task<bool> StrategyTestRequestedHandler(StrategyTestDataModel strategyTest);
}

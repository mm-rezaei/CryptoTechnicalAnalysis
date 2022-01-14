using System.Threading.Tasks;
using TechnicalAnalysisTools.Ui.Win.DataModels;

namespace TechnicalAnalysisTools.Ui.Win.Delegates
{
    internal delegate Task<bool> SessionEstablishmentRequestHandler(SessionEstablishmentDataModel sessionEstablishment);
}

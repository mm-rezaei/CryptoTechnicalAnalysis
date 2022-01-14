using System.Data.Entity;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.DataAdapters
{
    public class CryptoHistoricalDataAdapter : DbContext
    {
        public CryptoHistoricalDataAdapter() : base(ServerConstantHelper.CryptoHistoricalConnectionString)
        {
        }
    }
}
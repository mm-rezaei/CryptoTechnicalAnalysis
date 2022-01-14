using System;
using System.IO;

namespace TechnicalAnalysisTools.Ui.Win.Helpers
{
    internal static class ClientAddressHelper
    {
        static ClientAddressHelper()
        {
            ClientInformationFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Shared.Data", "ChanelSettings", "ClientInformation.txt");

            LayoutDataFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Shared.Data", "LayoutData");
        }

        public static string ClientInformationFile { get; }

        public static string LayoutDataFolder { get; }
    }
}

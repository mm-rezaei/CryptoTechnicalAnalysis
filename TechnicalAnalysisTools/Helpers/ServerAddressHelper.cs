using System;
using System.IO;

namespace TechnicalAnalysisTools.Helpers
{
    public static class ServerAddressHelper
    {
        static ServerAddressHelper()
        {
            ServerLogsFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "ChanelSettings", "ServerLogs.txt");

            SessionEstablishmentFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "ChanelSettings", "SessionEstablishment.txt");

            HistoricalDataFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "HistoricalData");

            MilestoneDataFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "MilestoneData");

            AlarmDataFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "AlarmData");

            AlarmDataBackupFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "AlarmDataBackup");

            CompiledAlarmDataFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "CompiledAlarmData");

            CompiledAlarmDataBackupFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "CompiledAlarmDataBackup");

            AlarmHistoryFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "ChanelSettings", "AlarmHistory.txt");

            SymbolsListFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "ChanelSettings", "SymbolsList.txt");

            UserDataFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "UserData");

            StrategyTestDataFolder = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Data", "StrategyTestData");
        }

        public static string ServerLogsFile { get; }

        public static string SessionEstablishmentFile { get; }

        public static string HistoricalDataFolder { get; }

        public static string MilestoneDataFolder { get; }

        public static string AlarmDataFolder { get; }

        public static string AlarmDataBackupFolder { get; }

        public static string CompiledAlarmDataFolder { get; }

        public static string CompiledAlarmDataBackupFolder { get; }

        public static string AlarmHistoryFile { get; }

        public static string SymbolsListFile { get; }

        public static string UserDataFolder { get; }

        public static string StrategyTestDataFolder { get; }
    }
}

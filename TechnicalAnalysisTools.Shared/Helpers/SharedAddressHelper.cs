using System;
using System.IO;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public static class SharedAddressHelper
    {
        static SharedAddressHelper()
        {
            AlarmWavFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Shared.Data", "RingtoneData", "ConditionAlarm.wav");

            BinanceConnectionOnWavFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Shared.Data", "RingtoneData", "BinanceConnectionOn.wav");

            BinanceConnectionOffWavFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Shared.Data", "RingtoneData", "BinanceConnectionOff.wav");

            ChanelConnectionOnWavFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Shared.Data", "RingtoneData", "ChanelConnectionOn.wav");

            ChanelConnectionOffWavFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).FullName, "TechnicalAnalysisTools.Shared.Data", "RingtoneData", "ChanelConnectionOff.wav");
        }

        public static string AlarmWavFile { get; }

        public static string BinanceConnectionOnWavFile { get; }

        public static string BinanceConnectionOffWavFile { get; }

        public static string ChanelConnectionOnWavFile { get; }

        public static string ChanelConnectionOffWavFile { get; }
    }
}

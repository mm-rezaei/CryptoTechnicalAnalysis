using System;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public class DateTimeHelper
    {
        public static DateTime ConvertSecondsToDateTime(int seconds)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return origin.AddSeconds(seconds);
        }

        public static int ConvertDateTimeToSeconds(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return (int)date.Subtract(origin).TotalSeconds;
        }

        public static string ConvertDateTimeToString(DateTime date)
        {
            return date.ToString("yyyy/MM/dd HH:mm");
        }
    }
}

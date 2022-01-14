using System;
using System.Collections.Generic;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public class StockExchangesTimeHelper
    {
        static StockExchangesTimeHelper()
        {
            TimeZoneIdDictionary = new Dictionary<StockExchanges, string>();
            WeeklyOffDayDictionary = new Dictionary<StockExchanges, List<DayOfWeek>>();
            OpenCloseMinuteInDayDictionary = new Dictionary<StockExchanges, Tuple<int, int>>();

            TimeZoneIdDictionary.Add(StockExchanges.NewYork, "Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Japan, "Tokyo Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.London, "GMT Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.HongKong, "China Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Euronext, "Central European Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Toronto, "Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.China, "China Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.India, "India Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Germany, "Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Swiss, "Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Korea, "Korea Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Denmark, "Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Sweden, "Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Iceland, "GMT Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Australian, "AUS Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.SouthAfrica, "South Africa Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Spain, "Eastern Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Singapore, "Singapore Standard Time");
            TimeZoneIdDictionary.Add(StockExchanges.Moscow, "Russian Standard Time");

            WeeklyOffDayDictionary.Add(StockExchanges.NewYork, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Japan, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.London, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.HongKong, new List<DayOfWeek>() { DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Euronext, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Toronto, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.China, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.India, new List<DayOfWeek>() { DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Germany, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Swiss, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Korea, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Denmark, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Sweden, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Iceland, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Australian, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.SouthAfrica, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Spain, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Singapore, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });
            WeeklyOffDayDictionary.Add(StockExchanges.Moscow, new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Sunday });

            OpenCloseMinuteInDayDictionary.Add(StockExchanges.NewYork, new Tuple<int, int>(9 * 60 + 30, 16 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Japan, new Tuple<int, int>(9 * 60, 15 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.London, new Tuple<int, int>(8 * 60, 16 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.HongKong, new Tuple<int, int>(9 * 60 + 30, 16 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Euronext, new Tuple<int, int>(9 * 60, 17 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Toronto, new Tuple<int, int>(9 * 60 + 30, 16 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.China, new Tuple<int, int>(9 * 60 + 30, 15 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.India, new Tuple<int, int>(9 * 60 + 15, 15 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Germany, new Tuple<int, int>(8 * 60, 22 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Swiss, new Tuple<int, int>(9 * 60, 17 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Korea, new Tuple<int, int>(9 * 60, 15 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Denmark, new Tuple<int, int>(9 * 60, 17 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Sweden, new Tuple<int, int>(9 * 60, 17 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Iceland, new Tuple<int, int>(9 * 60 + 30, 15 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Australian, new Tuple<int, int>(10 * 60, 16 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.SouthAfrica, new Tuple<int, int>(9 * 60, 17 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Spain, new Tuple<int, int>(9 * 60, 17 * 60 + 30));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Singapore, new Tuple<int, int>(9 * 60, 17 * 60));
            OpenCloseMinuteInDayDictionary.Add(StockExchanges.Moscow, new Tuple<int, int>(7 * 60, 23 * 60 + 50));
        }

        private static Dictionary<StockExchanges, string> TimeZoneIdDictionary { get; }

        private static Dictionary<StockExchanges, List<DayOfWeek>> WeeklyOffDayDictionary { get; }

        private static Dictionary<StockExchanges, Tuple<int, int>> OpenCloseMinuteInDayDictionary { get; }

        private static DateTime ConvertUniversalToLocal(DateTime universalTime, string timeZoneById)
        {
            var remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneById);

            return TimeZoneInfo.ConvertTimeFromUtc(universalTime, remoteTimeZone);
        }

        public static bool IsStockExchangeOpen(StockExchanges stockExchange, DateTime datetime)
        {
            var result = false;

            try
            {
                var timeZoneId = TimeZoneIdDictionary[stockExchange];

                var localDateTime = ConvertUniversalToLocal(datetime, timeZoneId);

                if (stockExchange == StockExchanges.Forex)
                {
                    return IsStockExchangeOpen(StockExchanges.Australian, datetime) || IsStockExchangeOpen(StockExchanges.Japan, datetime) || IsStockExchangeOpen(StockExchanges.London, datetime) || IsStockExchangeOpen(StockExchanges.NewYork, datetime);
                }
                else
                {
                    if (!WeeklyOffDayDictionary[stockExchange].Contains(localDateTime.DayOfWeek))
                    {
                        var openCloseMinuteInDay = OpenCloseMinuteInDayDictionary[stockExchange];

                        var minuteInDay = localDateTime.Hour * 60 + localDateTime.Minute;

                        if (openCloseMinuteInDay.Item1 <= minuteInDay && minuteInDay <= openCloseMinuteInDay.Item2)
                        {
                            result = true;
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}

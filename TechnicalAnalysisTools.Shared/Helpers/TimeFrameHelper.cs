using System;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public static class TimeFrameHelper
    {
        private static TimeFrames[] _TimeFramesList;

        public static TimeFrames[] TimeFramesList
        {
            get
            {
                if (_TimeFramesList == null)
                {
                    _TimeFramesList = (TimeFrames[])Enum.GetValues(typeof(TimeFrames));
                }

                return _TimeFramesList;
            }
        }

        public static DateTime GetOpenDateTimeOfSpesificCandle(TimeFrames timeFrame, DateTime dateTime)
        {
            DateTime result;

            switch (timeFrame)
            {
                case TimeFrames.Minute1:
                    {
                        result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
                    }
                    break;
                case TimeFrames.Minute3:
                case TimeFrames.Minute5:
                case TimeFrames.Minute15:
                case TimeFrames.Minute30:
                    {
                        var minutes = (int)timeFrame;

                        result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute - (dateTime.Minute % minutes), 0);
                    }
                    break;
                case TimeFrames.Hour1:
                case TimeFrames.Hour2:
                case TimeFrames.Hour4:
                case TimeFrames.Hour6:
                case TimeFrames.Hour8:
                case TimeFrames.Hour12:
                    {
                        var hours = ((int)timeFrame) / 60;

                        result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour - (dateTime.Hour % hours), 0, 0);
                    }
                    break;
                case TimeFrames.Day1:
                    {
                        result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
                    }
                    break;
                case TimeFrames.Day3:
                    {
                        result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);

                        var baseDateTime = new DateTime(1999, 12, 30, 0, 0, 0);

                        var totalDays = (result - baseDateTime).TotalDays;

                        while (totalDays % 3 != 0)
                        {
                            result = result.AddDays(-1);

                            totalDays = (result - baseDateTime).TotalDays;
                        }
                    }
                    break;
                case TimeFrames.Week1:
                    {
                        result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);

                        while (result.DayOfWeek != DayOfWeek.Monday)
                        {
                            result = result.AddDays(-1);
                        }
                    }
                    break;
                case TimeFrames.Month1:
                    {
                        result = new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0);
                    }
                    break;
                default:
                    {
                        throw new Exception("TimeFrame is not valid.");
                    }
            }

            return result;
        }

        public static DateTime GetCloseDateTimeOfSpesificCandle(TimeFrames timeFrame, DateTime dateTime)
        {
            var result = GetOpenDateTimeOfSpesificCandle(timeFrame, dateTime);

            switch (timeFrame)
            {
                case TimeFrames.Minute1:
                case TimeFrames.Minute3:
                case TimeFrames.Minute5:
                case TimeFrames.Minute15:
                case TimeFrames.Minute30:
                case TimeFrames.Hour1:
                case TimeFrames.Hour2:
                case TimeFrames.Hour4:
                case TimeFrames.Hour6:
                case TimeFrames.Hour8:
                case TimeFrames.Hour12:
                case TimeFrames.Day1:
                case TimeFrames.Day3:
                case TimeFrames.Week1:
                    {
                        result = result.AddMinutes(((int)timeFrame) - 1);
                    }
                    break;
                case TimeFrames.Month1:
                    {
                        result = result.AddMonths(1);
                        result = result.AddMinutes(-1);
                    }
                    break;
                default:
                    {
                        throw new Exception("TimeFrame is not valid.");
                    }
            }

            return result;
        }

        public static bool IsThisMinuteCandleFirstTimeFrameCandle(DateTime candleDateTime, TimeFrames timeFrame)
        {
            bool result = false;

            if (timeFrame == TimeFrames.Minute1)
            {
                result = true;
            }
            else if (timeFrame == TimeFrames.Minute3 || timeFrame == TimeFrames.Minute5 || timeFrame == TimeFrames.Minute15 || timeFrame == TimeFrames.Minute30)
            {
                if (candleDateTime.Minute % ((int)timeFrame) == 0)
                {
                    result = true;
                }
            }
            else if (timeFrame == TimeFrames.Hour1 || timeFrame == TimeFrames.Hour2 || timeFrame == TimeFrames.Hour4 || timeFrame == TimeFrames.Hour6 || timeFrame == TimeFrames.Hour8 || timeFrame == TimeFrames.Hour12)
            {
                var totalMinutes = (candleDateTime.Hour * 60) + candleDateTime.Minute;

                if (totalMinutes % ((int)timeFrame) == 0)
                {
                    result = true;
                }
            }
            else if (timeFrame == TimeFrames.Day1)
            {
                if (candleDateTime.Hour == 0 && candleDateTime.Minute == 0)
                {
                    result = true;
                }
            }
            else if (timeFrame == TimeFrames.Day3)
            {
                var baseDateTime = new DateTime(1999, 12, 30, 0, 0, 0);

                if (candleDateTime.Hour == 0 && candleDateTime.Minute == 0)
                {
                    var totalDays = (candleDateTime - baseDateTime).TotalDays;

                    if (totalDays % 3 == 0)
                    {
                        result = true;
                    }
                }
            }
            else if (timeFrame == TimeFrames.Week1)
            {
                if (candleDateTime.Hour == 0 && candleDateTime.Minute == 0)
                {
                    if (candleDateTime.DayOfWeek == DayOfWeek.Monday)
                    {
                        result = true;
                    }
                }
            }
            else if (timeFrame == TimeFrames.Month1)
            {
                if (candleDateTime.Day == 1 && candleDateTime.Hour == 0 && candleDateTime.Minute == 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public static bool IsThisMinuteCandleLastTimeFrameCandle(DateTime candleDateTime, TimeFrames timeFrame)
        {
            bool result = IsThisMinuteCandleFirstTimeFrameCandle(candleDateTime.AddMinutes(1), timeFrame);

            return result;
        }
    }
}

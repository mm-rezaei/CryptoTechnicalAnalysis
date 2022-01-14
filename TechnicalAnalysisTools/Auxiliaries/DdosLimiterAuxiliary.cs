using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class DdosLimiterAuxiliary
    {
        public DdosLimiterAuxiliary(double numItems, double timePeriod)
        {
            NumItems = numItems;
            TimePeriod = timePeriod;
            RatePerSecond = numItems / timePeriod;
        }

        private double NumItems { get; set; }

        private double RatePerSecond { get; set; }

        private ConcurrentDictionary<object, RateInfo> RateTable { get; set; } = new ConcurrentDictionary<object, RateInfo>();

        private double TimePeriod { get; set; }

        private struct RateInfo
        {
            private readonly double allowance;
            private readonly DateTime lastCheckTime;

            public RateInfo(DateTime lastCheckTime, double allowance)
            {
                this.lastCheckTime = lastCheckTime;
                this.allowance = allowance;
            }

            public DateTime LastCheckTime
            {
                get
                {
                    return lastCheckTime;
                }
            }

            public double Allowance
            {
                get
                {
                    return allowance;
                }
            }
        }

        public double Count
        {
            get
            {
                return NumItems;
            }
        }

        public double Per
        {
            get
            {
                return TimePeriod;
            }
        }

        public bool IsPermitted(object key)
        {
            var result = true;

            var now = DateTime.UtcNow;

            RateTable.AddOrUpdate(
                key,
                k => new RateInfo(now, NumItems - 1d),
                (k, rateInfo) =>
                {
                    var timePassedSeconds =
                                      (now - rateInfo.LastCheckTime).TotalSeconds;
                    var newAllowance =
                                      Math.Min(rateInfo.Allowance
                                                + timePassedSeconds
                                                * RatePerSecond,
                               NumItems);
                    if (newAllowance < 1d)
                    {
                        result = false;
                    }
                    else
                    {
                        newAllowance -= 1d;
                    }
                    return new RateInfo(now, newAllowance);
                });

            var expiredKeys = RateTable
                   .Where(kvp =>
                       (now - kvp.Value.LastCheckTime) >
                       TimeSpan.FromSeconds(TimePeriod))
                   .Select(k => k.Key);

            foreach (var expiredKey in expiredKeys)
            {
                Reset(expiredKey);
            }

            return result;
        }

        public void Reset(object key)
        {
            RateInfo rateInfo;

            while (!RateTable.TryRemove(key, out rateInfo))
            {
                Thread.Sleep(0);
            }
        }
    }
}

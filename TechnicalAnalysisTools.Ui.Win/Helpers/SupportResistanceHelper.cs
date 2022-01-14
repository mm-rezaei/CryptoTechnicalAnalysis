using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Helpers
{
    internal class SupportResistanceHelper
    {
        private static PropertyInfo[] SymbolTimeFrameDataModelPropertyInfos { get; set; }

        public static void FillSupportsResistances(SymbolDataModel symbolDataModel)
        {
            //
            if (SymbolTimeFrameDataModelPropertyInfos == null)
            {
                SymbolTimeFrameDataModelPropertyInfos = typeof(SymbolTimeFrameDataModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToArray();
            }

            //
            var list = symbolDataModel.SupportsResistances.ToList();

            var lastPriceRecord = list.FirstOrDefault(p => p.Name == SymbolSupportsResistancesDataModel.CurrentPriceName);

            if (lastPriceRecord == null)
            {
                lastPriceRecord = new SymbolSupportsResistancesDataModel()
                {
                    Name = SymbolSupportsResistancesDataModel.CurrentPriceName,
                    TimeFrame = "",
                    Percent = 0
                };

                list.Add(lastPriceRecord);
                symbolDataModel.SupportsResistances.Add(lastPriceRecord);
            }

            lastPriceRecord.Price = symbolDataModel.Close;

            //
            var fieldLabels = new[] { "Sma{0}Value", "Ema{0}Value" };

            var periods = new int[] { 9, 20, 26, 30, 40, 50, 100, 200 };

            foreach (var timeFrame in TimeFrameHelper.TimeFramesList)
            {
                foreach (var fieldLable in fieldLabels)
                {
                    foreach (var period in periods)
                    {
                        //
                        var fieldName = string.Format(fieldLable, period);

                        var periodRecord = list.FirstOrDefault(p => p.Name == fieldName && p.TimeFrame == timeFrame.ToString());

                        if (periodRecord == null)
                        {
                            periodRecord = new SymbolSupportsResistancesDataModel()
                            {
                                Name = fieldName,
                                TimeFrame = timeFrame.ToString()
                            };

                            list.Add(periodRecord);
                            symbolDataModel.SupportsResistances.Add(periodRecord);
                        }

                        //
                        var timeFrameRecord = symbolDataModel.SymbolTimeFrames.FirstOrDefault(p => p.TimeFrame == timeFrame);

                        if (timeFrameRecord != null)
                        {
                            var propertyInfo = SymbolTimeFrameDataModelPropertyInfos.First(p => p.Name == fieldName);

                            periodRecord.Price = (float)propertyInfo.GetValue(timeFrameRecord);
                            periodRecord.Percent = ((periodRecord.Price / symbolDataModel.Close) - 1) * 100;
                        }
                        else
                        {
                            list.Remove(periodRecord);
                        }
                    }
                }
            }
        }
    }
}

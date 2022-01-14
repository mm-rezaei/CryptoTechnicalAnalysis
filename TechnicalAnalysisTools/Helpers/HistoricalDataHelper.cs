using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Helpers
{
    public class HistoricalDataHelper
    {
        private static int DetermineFirstIndexOfTimeFrame(IList<CandleDataModel> entities, int index, TimeFrames timeFrame)
        {
            int result = index;

            if (entities != null && entities.Count != 0 && timeFrame != TimeFrames.Minute1)
            {
                if (timeFrame == TimeFrames.Minute3 || timeFrame == TimeFrames.Minute5 || timeFrame == TimeFrames.Minute15 || timeFrame == TimeFrames.Minute30)
                {
                    for (; result < entities.Count; result++)
                    {
                        var dateTime = DateTimeHelper.ConvertSecondsToDateTime(entities[result].OpenTimeStamp);

                        if (dateTime.Minute % ((int)timeFrame) == 0)
                        {
                            break;
                        }
                    }
                }
                else if (timeFrame == TimeFrames.Hour1 || timeFrame == TimeFrames.Hour2 || timeFrame == TimeFrames.Hour4 || timeFrame == TimeFrames.Hour6 || timeFrame == TimeFrames.Hour8 || timeFrame == TimeFrames.Hour12)
                {
                    for (; result < entities.Count; result++)
                    {
                        var dateTime = DateTimeHelper.ConvertSecondsToDateTime(entities[result].OpenTimeStamp);

                        var totalMinutes = (dateTime.Hour * 60) + dateTime.Minute;

                        if (totalMinutes % ((int)timeFrame) == 0)
                        {
                            break;
                        }
                    }
                }
                else if (timeFrame == TimeFrames.Day1)
                {
                    for (; result < entities.Count; result++)
                    {
                        var dateTime = DateTimeHelper.ConvertSecondsToDateTime(entities[result].OpenTimeStamp);

                        if (dateTime.Hour == 0 && dateTime.Minute == 0)
                        {
                            break;
                        }
                    }
                }
                else if (timeFrame == TimeFrames.Day3)
                {
                    var baseDateTime = new DateTime(1999, 12, 30, 0, 0, 0);

                    for (; result < entities.Count; result++)
                    {
                        var dateTime = DateTimeHelper.ConvertSecondsToDateTime(entities[result].OpenTimeStamp);

                        if (dateTime.Hour == 0 && dateTime.Minute == 0)
                        {
                            var totalDays = (dateTime - baseDateTime).TotalDays;

                            if (totalDays % 3 == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (timeFrame == TimeFrames.Week1)
                {
                    for (; result < entities.Count; result++)
                    {
                        var dateTime = DateTimeHelper.ConvertSecondsToDateTime(entities[result].OpenTimeStamp);

                        if (dateTime.Hour == 0 && dateTime.Minute == 0)
                        {
                            if (dateTime.DayOfWeek == DayOfWeek.Monday)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (timeFrame == TimeFrames.Month1)
                {
                    for (; result < entities.Count; result++)
                    {
                        var dateTime = DateTimeHelper.ConvertSecondsToDateTime(entities[result].OpenTimeStamp);

                        if (dateTime.Day == 1 && dateTime.Hour == 0 && dateTime.Minute == 0)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private static int NormalizedDateTimeStamp(int dateTimeStamp)
        {
            return dateTimeStamp - (dateTimeStamp % 60);
        }

        private static CandleDataModel CreateTimeFrameCandle(IList<CandleDataModel> selectedMinuteEntities)
        {
            var entity = new CandleDataModel();

            entity.MomentaryTimeStamp = selectedMinuteEntities[0].MomentaryTimeStamp;
            entity.OpenTimeStamp = selectedMinuteEntities[0].OpenTimeStamp;
            entity.Open = selectedMinuteEntities[0].Open;
            entity.High = selectedMinuteEntities.Select(p => p.High).Max();
            entity.Low = selectedMinuteEntities.Select(p => p.Low).Min();
            entity.Close = selectedMinuteEntities[selectedMinuteEntities.Count - 1].Close;
            entity.Volume = selectedMinuteEntities.Sum(p => p.Volume);
            entity.QuoteVolume = selectedMinuteEntities.Sum(p => p.QuoteVolume);
            entity.NumberOfTrades = selectedMinuteEntities.Sum(p => p.NumberOfTrades);
            entity.TakerVolume = selectedMinuteEntities.Sum(p => p.TakerVolume);
            entity.TakerQuoteVolume = selectedMinuteEntities.Sum(p => p.TakerQuoteVolume);

            return entity;
        }

        public static IList<CandleDataModel> ExcelToMinute1Candles(string filePath)
        {
            var lines = File.ReadAllLines(filePath).ToList();

            lines.RemoveAt(0);

            CandleDataModel lastEntity = null;

            IList<CandleDataModel> result = new List<CandleDataModel>(lines.Count);

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    //
                    var entity = new CandleDataModel();

                    var words = line.Split(',');

                    entity.MomentaryTimeStamp = NormalizedDateTimeStamp((int)(Convert.ToDouble(words[(int)ExcelFields.DateTime]) / 1000000D));
                    entity.OpenTimeStamp = entity.MomentaryTimeStamp;
                    entity.Open = Convert.ToSingle(words[(int)ExcelFields.Open]);
                    entity.High = Convert.ToSingle(words[(int)ExcelFields.High]);
                    entity.Low = Convert.ToSingle(words[(int)ExcelFields.Low]);
                    entity.Close = Convert.ToSingle(words[(int)ExcelFields.Close]);
                    entity.Volume = Convert.ToSingle(words[(int)ExcelFields.Volume]);
                    entity.QuoteVolume = Convert.ToSingle(words[(int)ExcelFields.QuoteVolume]);
                    entity.NumberOfTrades = Convert.ToSingle(words[(int)ExcelFields.NumberOfTrades]);
                    entity.TakerVolume = Convert.ToSingle(words[(int)ExcelFields.TakerVolume]);
                    entity.TakerQuoteVolume = Convert.ToSingle(words[(int)ExcelFields.TakerQuoteVolume]);

                    if (lastEntity != null)
                    {
                        var lastEntityDateTime = DateTimeHelper.ConvertSecondsToDateTime(lastEntity.OpenTimeStamp);
                        var currentEntityDateTime = DateTimeHelper.ConvertSecondsToDateTime(entity.OpenTimeStamp);

                        if ((currentEntityDateTime - lastEntityDateTime).TotalSeconds > 60)
                        {
                            while ((currentEntityDateTime - lastEntityDateTime).TotalSeconds > 60)
                            {
                                var lastClosePrice = lastEntity.Close;
                                var lostDateTime = lastEntityDateTime.AddMinutes(1);

                                var lostEntity = new CandleDataModel();

                                lostEntity.MomentaryTimeStamp = DateTimeHelper.ConvertDateTimeToSeconds(lostDateTime);
                                lostEntity.OpenTimeStamp = lostEntity.MomentaryTimeStamp;
                                lostEntity.Open = lastClosePrice;
                                lostEntity.High = lastClosePrice;
                                lostEntity.Low = lastClosePrice;
                                lostEntity.Close = lastClosePrice;
                                lostEntity.Volume = 0;
                                lostEntity.QuoteVolume = 0;
                                lostEntity.NumberOfTrades = 0;
                                lostEntity.TakerVolume = 0;
                                lostEntity.TakerQuoteVolume = 0;

                                result.Add(lostEntity);

                                lastEntity = lostEntity;
                                lastEntityDateTime = lostDateTime;
                            }
                        }
                    }

                    //
                    var lastAddedEntity = result.LastOrDefault();

                    if (lastAddedEntity == null)
                    {
                        result.Add(entity);

                        lastEntity = entity;
                    }
                    else
                    {
                        if (lastAddedEntity.OpenTimeStamp != entity.OpenTimeStamp)
                        {
                            result.Add(entity);

                            lastEntity = entity;
                        }
                    }

                }
            }

            return result;
        }

        public static int CountMinute1Candles(string filePath)
        {
            return File.ReadAllLines(filePath).Length;
        }

        public static IList<CandleDataModel> CreateTimeFrameCandlesFromMinute1Candles(IList<CandleDataModel> entitiesM1, TimeFrames timeFrame)
        {
            var result = new List<CandleDataModel>(entitiesM1.Count / (int)timeFrame);

            var currentIndex = DetermineFirstIndexOfTimeFrame(entitiesM1, 0, timeFrame);

            if (currentIndex != 0)
            {
                result.Add(CreateTimeFrameCandle(entitiesM1.Take(currentIndex).ToList()));
            }

            while (currentIndex < entitiesM1.Count)
            {
                //
                var selectedMinuteEntities = new List<CandleDataModel>();

                selectedMinuteEntities.Add(entitiesM1[currentIndex]);

                currentIndex++;

                if (currentIndex < entitiesM1.Count)
                {
                    var nextIndex = DetermineFirstIndexOfTimeFrame(entitiesM1, currentIndex, timeFrame);

                    for (; currentIndex < nextIndex; currentIndex++)
                    {
                        selectedMinuteEntities.Add(entitiesM1[currentIndex]);
                    }
                }

                //
                var entity = CreateTimeFrameCandle(selectedMinuteEntities);

                result.Add(entity);
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using StockSharp.Algo.Candles;
using StockSharp.Messages;

namespace TechnicalAnalysisTools.Helpers
{
    public static class TimeFrameCandleHelper
    {
        public static byte[] ToByteArray(TimeFrameCandle candle)
        {
            byte[] result;

            var dictionary = new Dictionary<string, object>();

            dictionary["OpenPrice"] = candle.OpenPrice;
            dictionary["HighPrice"] = candle.HighPrice;
            dictionary["LowPrice"] = candle.LowPrice;
            dictionary["ClosePrice"] = candle.ClosePrice;
            dictionary["TotalVolume"] = candle.TotalVolume;
            dictionary["State"] = candle.State.ToString();

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(memoryStream, dictionary);

                result = memoryStream.ToArray();
            }

            return result;
        }

        public static byte[] CandleArrayToByteArray(TimeFrameCandle[] candles)
        {
            byte[] result;

            var list = new List<byte[]>();

            foreach (var candle in candles)
            {
                list.Add(ToByteArray(candle));
            }

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(memoryStream, list);

                result = memoryStream.ToArray();
            }

            return result;
        }

        public static TimeFrameCandle FromByteArray(byte[] candleByteArray)
        {
            var result = new TimeFrameCandle();

            Dictionary<string, object> dictionary;

            using (var memoryStream = new MemoryStream(candleByteArray))
            {
                var formatter = new BinaryFormatter();

                dictionary = (Dictionary<string, object>)formatter.Deserialize(memoryStream);
            }

            result.OpenPrice = Convert.ToDecimal(dictionary["OpenPrice"]);
            result.HighPrice = Convert.ToDecimal(dictionary["HighPrice"]);
            result.LowPrice = Convert.ToDecimal(dictionary["LowPrice"]);
            result.ClosePrice = Convert.ToDecimal(dictionary["ClosePrice"]);
            result.TotalVolume = Convert.ToDecimal(dictionary["TotalVolume"]);
            result.State = (CandleStates)Enum.Parse(typeof(CandleStates), dictionary["State"].ToString());

            return result;
        }

        public static TimeFrameCandle[] CandleArrayFromByteArray(byte[] candlesByteArray)
        {
            TimeFrameCandle[] result;

            List<byte[]> list;

            using (var memoryStream = new MemoryStream(candlesByteArray))
            {
                var formatter = new BinaryFormatter();

                list = (List<byte[]>)formatter.Deserialize(memoryStream);
            }

            result = new TimeFrameCandle[list.Count];

            for (var index = 0; index < list.Count; index++)
            {
                result[index] = FromByteArray(list[index]);
            }

            return result;
        }
    }
}

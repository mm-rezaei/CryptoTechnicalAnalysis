using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.DataAdapters;
using TechnicalAnalysisTools.Services;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Helpers
{
    public static class DatabaseHelper
    {
        static DatabaseHelper()
        {
            FieldInfos = typeof(CandleDataModel).GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
        }

        private static FieldInfo[] FieldInfos { get; }

        public static string GetSymbolTableName(SymbolTypes symbol, TimeFrames timeFrame)
        {
            return string.Format("{0}-{1}", symbol.ToString(), timeFrame.ToString());
        }

        public static string[] GetTablesList()
        {
            string[] result;

            using (var context = new CryptoHistoricalDataAdapter())
            {
                result = context.Database.SqlQuery<string>("SELECT name FROM sys.tables ORDER BY name").ToArray();
            }

            return result;
        }

        public static void CreateCandleDataModelTable(string tableName)
        {
            using (var context = new CryptoHistoricalDataAdapter())
            {
                var createTableScript = "";

                createTableScript += "CREATE TABLE [dbo].[{0}](" + Environment.NewLine;
                createTableScript += "	[MomentaryTimeStamp] [int] NOT NULL," + Environment.NewLine;
                createTableScript += "	[OpenTimeStamp] [int] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Open] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[High] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Low] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Close] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Volume] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[QuoteVolume] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[TakerVolume] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[TakerQuoteVolume] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[NumberOfTrades] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[CandleType] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[BollingerBandsBasis] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[BollingerUpper] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[BollingerLower] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema9Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema20Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema26Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema30Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema40Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema50Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema100Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Ema200Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[IchimokuTenkanSen] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[IchimokuKijunSen] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[IchimokuSenkouSpanA] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[IchimokuSenkouSpanB] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[IchimokuChikouSpan] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[IchimokuSenkouSpanA26] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[IchimokuSenkouSpanB26] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[MacdValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[MacdSignal] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[MacdHistogram] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[MfiValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RsiValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma9Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma20Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma26Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma30Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma40Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma50Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma100Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[Sma200Value] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[StochKValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[StochDValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[StochRsiKValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[StochRsiDValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[WilliamsRValue] [real] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularAscendingRsiDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularAscendingStochasticKValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularAscendingStochasticDValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularAscendingMacdValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularAscendingMacdSignalDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularAscendingMacdHistogramDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularDescendingRsiDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularDescendingStochasticKValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularDescendingStochasticDValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularDescendingMacdValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularDescendingMacdSignalDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[RegularDescendingMacdHistogramDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenAscendingRsiDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenAscendingStochasticKValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenAscendingStochasticDValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenAscendingMacdValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenAscendingMacdSignalDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenAscendingMacdHistogramDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenDescendingRsiDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenDescendingStochasticKValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenDescendingStochasticDValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenDescendingMacdValueDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenDescendingMacdSignalDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	[HiddenDescendingMacdHistogramDivergence] [tinyint] NOT NULL," + Environment.NewLine;
                createTableScript += "	CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED" + Environment.NewLine;
                createTableScript += "	(" + Environment.NewLine;
                createTableScript += "		[MomentaryTimeStamp] ASC" + Environment.NewLine;
                createTableScript += "	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]" + Environment.NewLine;
                createTableScript += "	) ON [PRIMARY]";

                createTableScript = string.Format(createTableScript, tableName);

                context.Database.ExecuteSqlCommand(createTableScript);
            }
        }

        public static DateTime? GetFirstDateTime(string tableName)
        {
            DateTime? result;

            using (var context = new CryptoHistoricalDataAdapter())
            {
                context.Database.Connection.Open();

                using (var command = context.Database.Connection.CreateCommand())
                {
                    command.CommandText = string.Format("select MIN(MomentaryTimeStamp) from dbo.[{0}]", tableName);

                    var value = command.ExecuteScalar();

                    if (value == DBNull.Value)
                    {
                        result = null;
                    }
                    else
                    {
                        result = DateTimeHelper.ConvertSecondsToDateTime((int)value);
                    }
                }

                context.Database.Connection.Close();
            }

            return result;
        }

        public static DateTime? GetLastDateTime(string tableName)
        {
            DateTime? result;

            using (var context = new CryptoHistoricalDataAdapter())
            {
                context.Database.Connection.Open();

                using (var command = context.Database.Connection.CreateCommand())
                {
                    command.CommandText = string.Format("select MAX(MomentaryTimeStamp) from dbo.[{0}]", tableName);

                    var value = command.ExecuteScalar();

                    if (value == DBNull.Value)
                    {
                        result = null;
                    }
                    else
                    {
                        result = DateTimeHelper.ConvertSecondsToDateTime((int)value);
                    }
                }

                context.Database.Connection.Close();
            }

            return result;
        }

        public static void SaveCandleDataModelsToDatabase(string tableName, List<CandleDataModel> candleDataModelsToSaveToDatabase)
        {
            using (var context = new CryptoHistoricalDataAdapter())
            {
                context.Database.Connection.Open();

                var bulkCopy = new SqlBulkCopy((SqlConnection)context.Database.Connection) { BulkCopyTimeout = int.MaxValue };

                bulkCopy.DestinationTableName = "dbo.[" + tableName + "]";
                bulkCopy.ColumnMappings.Add("MomentaryTimeStamp", "MomentaryTimeStamp");
                bulkCopy.ColumnMappings.Add("OpenTimeStamp", "OpenTimeStamp");
                bulkCopy.ColumnMappings.Add("Open", "Open");
                bulkCopy.ColumnMappings.Add("High", "High");
                bulkCopy.ColumnMappings.Add("Low", "Low");
                bulkCopy.ColumnMappings.Add("Close", "Close");
                bulkCopy.ColumnMappings.Add("Volume", "Volume");
                bulkCopy.ColumnMappings.Add("QuoteVolume", "QuoteVolume");
                bulkCopy.ColumnMappings.Add("TakerVolume", "TakerVolume");
                bulkCopy.ColumnMappings.Add("TakerQuoteVolume", "TakerQuoteVolume");
                bulkCopy.ColumnMappings.Add("NumberOfTrades", "NumberOfTrades");
                bulkCopy.ColumnMappings.Add("CandleType", "CandleType");
                bulkCopy.ColumnMappings.Add("BollingerBandsBasis", "BollingerBandsBasis");
                bulkCopy.ColumnMappings.Add("BollingerUpper", "BollingerUpper");
                bulkCopy.ColumnMappings.Add("BollingerLower", "BollingerLower");
                bulkCopy.ColumnMappings.Add("Ema9Value", "Ema9Value");
                bulkCopy.ColumnMappings.Add("Ema20Value", "Ema20Value");
                bulkCopy.ColumnMappings.Add("Ema26Value", "Ema26Value");
                bulkCopy.ColumnMappings.Add("Ema30Value", "Ema30Value");
                bulkCopy.ColumnMappings.Add("Ema40Value", "Ema40Value");
                bulkCopy.ColumnMappings.Add("Ema50Value", "Ema50Value");
                bulkCopy.ColumnMappings.Add("Ema100Value", "Ema100Value");
                bulkCopy.ColumnMappings.Add("Ema200Value", "Ema200Value");
                bulkCopy.ColumnMappings.Add("IchimokuTenkanSen", "IchimokuTenkanSen");
                bulkCopy.ColumnMappings.Add("IchimokuKijunSen", "IchimokuKijunSen");
                bulkCopy.ColumnMappings.Add("IchimokuSenkouSpanA", "IchimokuSenkouSpanA");
                bulkCopy.ColumnMappings.Add("IchimokuSenkouSpanB", "IchimokuSenkouSpanB");
                bulkCopy.ColumnMappings.Add("IchimokuChikouSpan", "IchimokuChikouSpan");
                bulkCopy.ColumnMappings.Add("IchimokuSenkouSpanA", "IchimokuSenkouSpanA26");
                bulkCopy.ColumnMappings.Add("IchimokuSenkouSpanB", "IchimokuSenkouSpanB26");
                bulkCopy.ColumnMappings.Add("MacdValue", "MacdValue");
                bulkCopy.ColumnMappings.Add("MacdSignal", "MacdSignal");
                bulkCopy.ColumnMappings.Add("MacdHistogram", "MacdHistogram");
                bulkCopy.ColumnMappings.Add("MfiValue", "MfiValue");
                bulkCopy.ColumnMappings.Add("RsiValue", "RsiValue");
                bulkCopy.ColumnMappings.Add("Sma9Value", "Sma9Value");
                bulkCopy.ColumnMappings.Add("Sma20Value", "Sma20Value");
                bulkCopy.ColumnMappings.Add("Sma26Value", "Sma26Value");
                bulkCopy.ColumnMappings.Add("Sma30Value", "Sma30Value");
                bulkCopy.ColumnMappings.Add("Sma40Value", "Sma40Value");
                bulkCopy.ColumnMappings.Add("Sma50Value", "Sma50Value");
                bulkCopy.ColumnMappings.Add("Sma100Value", "Sma100Value");
                bulkCopy.ColumnMappings.Add("Sma200Value", "Sma200Value");
                bulkCopy.ColumnMappings.Add("StochKValue", "StochKValue");
                bulkCopy.ColumnMappings.Add("StochDValue", "StochDValue");
                bulkCopy.ColumnMappings.Add("StochRsiKValue", "StochRsiKValue");
                bulkCopy.ColumnMappings.Add("StochRsiDValue", "StochRsiDValue");
                bulkCopy.ColumnMappings.Add("WilliamsRValue", "WilliamsRValue");
                bulkCopy.ColumnMappings.Add("RegularAscendingRsiDivergence", "RegularAscendingRsiDivergence");
                bulkCopy.ColumnMappings.Add("RegularAscendingStochasticKValueDivergence", "RegularAscendingStochasticKValueDivergence");
                bulkCopy.ColumnMappings.Add("RegularAscendingStochasticDValueDivergence", "RegularAscendingStochasticDValueDivergence");
                bulkCopy.ColumnMappings.Add("RegularAscendingMacdValueDivergence", "RegularAscendingMacdValueDivergence");
                bulkCopy.ColumnMappings.Add("RegularAscendingMacdSignalDivergence", "RegularAscendingMacdSignalDivergence");
                bulkCopy.ColumnMappings.Add("RegularAscendingMacdHistogramDivergence", "RegularAscendingMacdHistogramDivergence");
                bulkCopy.ColumnMappings.Add("RegularDescendingRsiDivergence", "RegularDescendingRsiDivergence");
                bulkCopy.ColumnMappings.Add("RegularDescendingStochasticKValueDivergence", "RegularDescendingStochasticKValueDivergence");
                bulkCopy.ColumnMappings.Add("RegularDescendingStochasticDValueDivergence", "RegularDescendingStochasticDValueDivergence");
                bulkCopy.ColumnMappings.Add("RegularDescendingMacdValueDivergence", "RegularDescendingMacdValueDivergence");
                bulkCopy.ColumnMappings.Add("RegularDescendingMacdSignalDivergence", "RegularDescendingMacdSignalDivergence");
                bulkCopy.ColumnMappings.Add("RegularDescendingMacdHistogramDivergence", "RegularDescendingMacdHistogramDivergence");
                bulkCopy.ColumnMappings.Add("HiddenAscendingRsiDivergence", "HiddenAscendingRsiDivergence");
                bulkCopy.ColumnMappings.Add("HiddenAscendingStochasticKValueDivergence", "HiddenAscendingStochasticKValueDivergence");
                bulkCopy.ColumnMappings.Add("HiddenAscendingStochasticDValueDivergence", "HiddenAscendingStochasticDValueDivergence");
                bulkCopy.ColumnMappings.Add("HiddenAscendingMacdValueDivergence", "HiddenAscendingMacdValueDivergence");
                bulkCopy.ColumnMappings.Add("HiddenAscendingMacdSignalDivergence", "HiddenAscendingMacdSignalDivergence");
                bulkCopy.ColumnMappings.Add("HiddenAscendingMacdHistogramDivergence", "HiddenAscendingMacdHistogramDivergence");
                bulkCopy.ColumnMappings.Add("HiddenDescendingRsiDivergence", "HiddenDescendingRsiDivergence");
                bulkCopy.ColumnMappings.Add("HiddenDescendingStochasticKValueDivergence", "HiddenDescendingStochasticKValueDivergence");
                bulkCopy.ColumnMappings.Add("HiddenDescendingStochasticDValueDivergence", "HiddenDescendingStochasticDValueDivergence");
                bulkCopy.ColumnMappings.Add("HiddenDescendingMacdValueDivergence", "HiddenDescendingMacdValueDivergence");
                bulkCopy.ColumnMappings.Add("HiddenDescendingMacdSignalDivergence", "HiddenDescendingMacdSignalDivergence");
                bulkCopy.ColumnMappings.Add("HiddenDescendingMacdHistogramDivergence", "HiddenDescendingMacdHistogramDivergence");

                using (var dataReader = new ObjectDataReaderService<CandleDataModel>(candleDataModelsToSaveToDatabase))
                {
                    bulkCopy.WriteToServer(dataReader);
                }

                context.Database.Connection.Close();
            }
        }

        public static CandleDataModel ReadCandleDataModelsFromDatabaseByMomentaryTimeStamp(string tableName, int momentaryTimeStamp)
        {
            CandleDataModel result = null;

            if (GetTablesList().Contains(tableName))
            {
                var script = string.Format("SELECT TOP 1 * FROM dbo.[{0}] WHERE [MomentaryTimeStamp] = {1}", tableName, momentaryTimeStamp.ToString());

                using (var context = new CryptoHistoricalDataAdapter())
                {
                    var connection = (SqlConnection)context.Database.Connection;

                    connection.Open();

                    try
                    {
                        using (var command = new SqlCommand(script, connection))
                        {
                            var reader = command.ExecuteReader();

                            if (reader.HasRows)
                            {
                                //
                                var fields = FieldInfos.Select(p => new { fieldInfo = p, ordinal = reader.GetOrdinal(p.Name) }).ToArray();

                                //
                                while (reader.Read())
                                {
                                    result = new CandleDataModel();

                                    foreach (var field in fields)
                                    {
                                        field.fieldInfo.SetValue(result, reader.GetValue(field.ordinal));
                                    }

                                    break;
                                }
                            }

                            reader.Close();
                        }
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return result;
        }

        public static CandleDataModel ReadCandleDataModelsFromDatabaseByMomentaryDateTime(string tableName, DateTime momentaryDateTime)
        {
            return ReadCandleDataModelsFromDatabaseByMomentaryTimeStamp(tableName, DateTimeHelper.ConvertDateTimeToSeconds(momentaryDateTime));
        }
    }
}

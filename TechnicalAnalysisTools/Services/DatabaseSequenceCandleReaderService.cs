using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.DataAdapters;
using TechnicalAnalysisTools.Shared.DataModels;

namespace TechnicalAnalysisTools.Services
{
    public class DatabaseSequenceCandleReaderService
    {
        static DatabaseSequenceCandleReaderService()
        {
            FieldInfos = typeof(CandleDataModel).GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
        }

        public DatabaseSequenceCandleReaderService(string script)
        {
            Script = script;
        }

        private static FieldInfo[] FieldInfos { get; }

        private string Script { get; }

        private CryptoHistoricalDataAdapter Context { get; set; }

        private SqlDataReader DataReader { get; set; }

        public bool Start()
        {
            var result = false;

            Context = new CryptoHistoricalDataAdapter();

            var connection = (SqlConnection)Context.Database.Connection;

            connection.Open();

            try
            {
                using (var command = new SqlCommand(Script, connection))
                {
                    DataReader = command.ExecuteReader();

                    if (DataReader.HasRows)
                    {
                        result = true;
                    }
                    else
                    {
                        Stop();

                        result = false;
                    }
                }
            }
            catch
            {
                Stop();

                result = false;
            }

            return result;
        }

        public CandleDataModel Next()
        {
            CandleDataModel result = null;

            if (Context != null)
            {
                var fields = FieldInfos.Select(p => new { fieldInfo = p, ordinal = DataReader.GetOrdinal(p.Name) }).ToArray();

                if (DataReader.Read())
                {
                    result = new CandleDataModel();

                    foreach (var field in fields)
                    {
                        field.fieldInfo.SetValue(result, DataReader.GetValue(field.ordinal));
                    }
                }
            }

            return result;
        }

        public void Stop()
        {
            if (DataReader != null)
            {
                try
                {
                    DataReader.Close();
                }
                catch
                {

                }
                finally
                {
                    DataReader = null;
                }
            }

            if (Context != null)
            {
                if (Context.Database.Connection != null)
                {
                    try
                    {
                        var connection = (SqlConnection)Context.Database.Connection;

                        connection.Close();

                        if (Context != null)
                        {
                            Context.Dispose();
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        Context = null;
                    }
                }
            }
        }
    }
}

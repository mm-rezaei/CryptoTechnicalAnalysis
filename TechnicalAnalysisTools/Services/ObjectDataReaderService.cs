using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace TechnicalAnalysisTools.Services
{
    public class ObjectDataReaderService<TData> : IDataReader
    {
        public ObjectDataReaderService(IEnumerable<TData> data)
        {
            DataEnumerator = data.GetEnumerator();

            Fields = typeof(TData).GetFields().ToArray();

            OrdinalLookup = Fields.Select((p, i) => new { Index = i, Field = p }).ToArray().ToDictionary(p => p.Field.Name, p => p.Index, StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerator<TData> DataEnumerator { get; set; }

        private FieldInfo[] Fields { get; set; }

        private Dictionary<string, int> OrdinalLookup { get; set; }

        public void Close()
        {
            Dispose();
        }

        public int Depth
        {
            get { return 1; }
        }

        public DataTable GetSchemaTable()
        {
            return null;
        }

        public bool IsClosed
        {
            get { return DataEnumerator == null; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (DataEnumerator == null)
            {
                throw new ObjectDisposedException("ObjectDataReaderService");
            }

            return DataEnumerator.MoveNext();
        }

        public int RecordsAffected
        {
            get { return -1; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DataEnumerator != null)
                {
                    DataEnumerator.Dispose();
                    DataEnumerator = null;
                }
            }
        }

        public int FieldCount
        {
            get { return Fields.Length; }
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            int ordinal;

            if (!OrdinalLookup.TryGetValue(name, out ordinal))
            {
                throw new InvalidOperationException("Unknown parameter name " + name);
            }

            return ordinal;
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            if (DataEnumerator == null)
            {
                throw new ObjectDisposedException("ObjectDataReaderService");
            }

            return Fields[i].GetValue(DataEnumerator.Current);
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public object this[int i]
        {
            get { throw new NotImplementedException(); }
        }
    }
}

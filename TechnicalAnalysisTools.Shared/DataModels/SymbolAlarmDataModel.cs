using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class SymbolAlarmDataModel : INotifyPropertyChanged, ICloneable
    {
        static SymbolAlarmDataModel()
        {
            FieldInfos = typeof(SymbolAlarmDataModel).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(p => !p.IsNotSerialized).ToArray();
        }

        public SymbolAlarmDataModel(string filename, string username)
        {
            FileName = filename;

            Username = username;

            _Id = Guid.NewGuid();
        }

        private static FieldInfo[] FieldInfos;
        private Guid _Id;
        private string _Name;
        private SymbolTypes _Symbol;
        private PositionTypes _Position;
        private string _Username;
        private bool _Seen;
        private DateTime _LastAlarm;
        private int _NotSeenEnabledCount;
        private int _TotalEnabledCount;
        private float _Price;
        private bool _Enabled;
        [field: NonSerialized()]
        private string _FileName;

        public Guid Id
        {
            get { return _Id; }
        }

        public string Name
        {
            get { return _Name; }
            set { if (_Name != value) { _Name = value; OnPropertyChanged(nameof(Name)); } }
        }

        public SymbolTypes Symbol
        {
            get { return _Symbol; }
            set { if (_Symbol != value) { _Symbol = value; OnPropertyChanged(nameof(Symbol)); } }
        }

        public PositionTypes Position
        {
            get { return _Position; }
            set { if (_Position != value) { _Position = value; OnPropertyChanged(nameof(Position)); } }
        }

        public string Username
        {
            get { return _Username; }
            set { if (_Username != value) { _Username = value; OnPropertyChanged(nameof(Username)); } }
        }

        public bool Seen
        {
            get { return _Seen; }
            set { if (_Seen != value) { _Seen = value; OnPropertyChanged(nameof(Seen)); } }
        }

        public DateTime LastAlarm
        {
            get { return _LastAlarm; }
            set { if (_LastAlarm != value) { _LastAlarm = value; OnPropertyChanged(nameof(LastAlarm)); } }
        }

        public int TotalEnabledCount
        {
            get { return _TotalEnabledCount; }
            set { if (_TotalEnabledCount != value) { _TotalEnabledCount = value; OnPropertyChanged(nameof(TotalEnabledCount)); } }
        }

        public int NotSeenEnabledCount
        {
            get { return _NotSeenEnabledCount; }
            set { if (_NotSeenEnabledCount != value) { _NotSeenEnabledCount = value; OnPropertyChanged(nameof(NotSeenEnabledCount)); } }
        }

        public float Price
        {
            get { return _Price; }
            set { if (_Price != value) { _Price = value; OnPropertyChanged(nameof(Price)); } }
        }

        public bool Enabled
        {
            get { return _Enabled; }
            set { if (_Enabled != value) { _Enabled = value; OnPropertyChanged(nameof(Enabled)); } }
        }

        public string FileName
        {
            get { return _FileName; }
            set { if (_FileName != value) { _FileName = value; OnPropertyChanged(nameof(FileName)); } }
        }

        public SolidColorBrush AlarmForgroundColor
        {
            get
            {
                if (Enabled)
                {
                    if (!Seen)
                    {
                        if (Position == PositionTypes.Long)
                        {
                            return Brushes.White;
                        }
                        else if (Position == PositionTypes.Short)
                        {
                            return Brushes.White;
                        }
                        else
                        {
                            return Brushes.Black;
                        }
                    }
                    else
                    {
                        return Brushes.Black;
                    }
                }
                else
                {
                    return Brushes.Black;
                }
            }
        }

        public SolidColorBrush AlarmBackgroundColor
        {
            get
            {
                if (Enabled)
                {
                    if (!Seen)
                    {
                        if (Position == PositionTypes.Long)
                        {
                            return Brushes.Green;
                        }
                        else if (Position == PositionTypes.Short)
                        {
                            return Brushes.Red;
                        }
                        else
                        {
                            return Brushes.White;
                        }
                    }
                    else
                    {
                        return Brushes.White;
                    }
                }
                else
                {
                    return Brushes.Yellow;
                }
            }
        }

        public void TriggerAlarm(DateTime lastAlarm, float price)
        {
            Seen = false;
            LastAlarm = lastAlarm;
            NotSeenEnabledCount++;
            TotalEnabledCount++;
            Price = price;

            OnPropertyChanged(nameof(AlarmForgroundColor));
            OnPropertyChanged(nameof(AlarmBackgroundColor));
        }

        public void SeenAlarm()
        {
            Seen = true;
            NotSeenEnabledCount = 0;

            OnPropertyChanged(nameof(AlarmForgroundColor));
            OnPropertyChanged(nameof(AlarmBackgroundColor));
        }

        public void SetEnabled(bool enable)
        {
            Enabled = enable;

            if (!Enabled)
            {
                Seen = true;
            }

            OnPropertyChanged(nameof(AlarmForgroundColor));
            OnPropertyChanged(nameof(AlarmBackgroundColor));
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            SymbolAlarmDataModel result;

            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);

                stream.Seek(0, SeekOrigin.Begin);

                result = (SymbolAlarmDataModel)formatter.Deserialize(stream);
            }

            return result;
        }

        public override string ToString()
        {
            var result = "";

            foreach (var fieldInfo in FieldInfos)
            {
                if (result != "")
                {
                    result += ",";
                }

                if (fieldInfo.FieldType == typeof(DateTime))
                {
                    result += Convert.ToString(fieldInfo.GetValue(this));
                }
                else
                {
                    result += fieldInfo.GetValue(this).ToString();
                }
            }

            return result;
        }

        public static SymbolAlarmDataModel ParseSymbolAlarm(string alarm)
        {
            var result = new SymbolAlarmDataModel("", "");

            var parts = alarm.Split(',').ToArray();

            for (var index = 0; index < FieldInfos.Length; index++)
            {
                var fieldInfo = FieldInfos[index];

                var part = parts[index];

                if (fieldInfo.FieldType == typeof(string))
                {
                    fieldInfo.SetValue(result, part);
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    fieldInfo.SetValue(result, Convert.ToInt32(part));
                }
                else if (fieldInfo.FieldType == typeof(float))
                {
                    fieldInfo.SetValue(result, Convert.ToSingle(part));
                }
                else if (fieldInfo.FieldType == typeof(bool))
                {
                    fieldInfo.SetValue(result, Convert.ToBoolean(part));
                }
                else if (fieldInfo.FieldType == typeof(DateTime))
                {
                    fieldInfo.SetValue(result, Convert.ToDateTime(part));
                }
                else if (fieldInfo.FieldType == typeof(SymbolTypes))
                {
                    fieldInfo.SetValue(result, Enum.Parse(typeof(SymbolTypes), part));
                }
                else if (fieldInfo.FieldType == typeof(PositionTypes))
                {
                    fieldInfo.SetValue(result, Enum.Parse(typeof(PositionTypes), part));
                }
                else if (fieldInfo.FieldType == typeof(Guid))
                {
                    fieldInfo.SetValue(result, new Guid(part));
                }
                else
                {
                    throw new Exception("SymbolAlarm field type is invalid.");
                }
            }

            return result;
        }
    }
}

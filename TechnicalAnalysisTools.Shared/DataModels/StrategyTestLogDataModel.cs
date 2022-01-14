using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class StrategyTestLogDataModel : INotifyPropertyChanged
    {
        static StrategyTestLogDataModel()
        {
            PropertyInfos = typeof(StrategyTestLogDataModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToArray();
        }

        private static PropertyInfo[] PropertyInfos { get; set; }

        private int _ActionId;

        private int _OrderId;

        private int _SubOrderId;

        private DateTime _Time;

        private TradeSubOrderActions _SubOrderAction;

        private float _Price;

        public int ActionId
        {
            get { return _ActionId; }
            set
            {
                if (_ActionId != value)
                {
                    _ActionId = value;

                    OnPropertyChanged(nameof(ActionId));
                }
            }
        }

        public int OrderId
        {
            get { return _OrderId; }
            set
            {
                if (_OrderId != value)
                {
                    _OrderId = value;

                    OnPropertyChanged(nameof(OrderId));
                }
            }
        }

        public int SubOrderId
        {
            get { return _SubOrderId; }
            set
            {
                if (_SubOrderId != value)
                {
                    _SubOrderId = value;

                    OnPropertyChanged(nameof(SubOrderId));
                }
            }
        }

        public DateTime Time
        {
            get { return _Time; }
            set
            {
                if (_Time != value)
                {
                    _Time = value;

                    OnPropertyChanged(nameof(Time));
                }
            }
        }

        public TradeSubOrderActions SubOrderAction
        {
            get { return _SubOrderAction; }
            set
            {
                if (_SubOrderAction != value)
                {
                    _SubOrderAction = value;

                    OnPropertyChanged(nameof(SubOrderAction));
                }
            }
        }

        public float Price
        {
            get { return _Price; }
            set
            {
                if (_Price != value)
                {
                    _Price = value;

                    OnPropertyChanged(nameof(Price));
                }
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string FieldNamesToString()
        {
            var result = "";

            foreach (var property in PropertyInfos)
            {
                if (result != "")
                {
                    result += ",";
                }

                result += property.Name;
            }

            return result;
        }

        public override string ToString()
        {
            var result = "";

            foreach (var property in PropertyInfos)
            {
                if (result != "")
                {
                    result += ",";
                }

                result += property.GetValue(this);
            }

            return result;
        }
    }
}

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class StrategyTestOrderDataModel : INotifyPropertyChanged
    {
        static StrategyTestOrderDataModel()
        {
            PropertyInfos = typeof(StrategyTestOrderDataModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToArray();
        }

        private static PropertyInfo[] PropertyInfos { get; set; }

        private int _OrderId;

        private int _SubOrderId;

        private TradeSubOrderActions _LastTradeSubOrderAction;

        private DateTime _StartTime;

        private DateTime _EndTime;

        private bool _IsOpened;

        private bool _IsClosed;

        private bool _Won;

        private float _EnterPrice;

        private float _Size;

        private float _Fee;

        private float _ExitPrice;

        private float _Profit;

        private float _ProfitPercent;

        private float _SavedProfit;

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

        public TradeSubOrderActions LastTradeSubOrderAction
        {
            get { return _LastTradeSubOrderAction; }
            set
            {
                if (_LastTradeSubOrderAction != value)
                {
                    _LastTradeSubOrderAction = value;

                    OnPropertyChanged(nameof(LastTradeSubOrderAction));
                }
            }
        }

        public DateTime StartTime
        {
            get { return _StartTime; }
            set
            {
                if (_StartTime != value)
                {
                    _StartTime = value;

                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        public DateTime EndTime
        {
            get { return _EndTime; }
            set
            {
                if (_EndTime != value)
                {
                    _EndTime = value;

                    OnPropertyChanged(nameof(EndTime));
                }
            }
        }

        public bool IsOpened
        {
            get { return _IsOpened; }
            set
            {
                if (_IsOpened != value)
                {
                    _IsOpened = value;

                    OnPropertyChanged(nameof(IsOpened));
                }
            }
        }

        public bool IsClosed
        {
            get { return _IsClosed; }
            set
            {
                if (_IsClosed != value)
                {
                    _IsClosed = value;

                    OnPropertyChanged(nameof(IsClosed));
                }
            }
        }

        public bool Won
        {
            get { return _Won; }
            set
            {
                if (_Won != value)
                {
                    _Won = value;

                    OnPropertyChanged(nameof(Won));
                }
            }
        }

        public float EnterPrice
        {
            get { return _EnterPrice; }
            set
            {
                if (_EnterPrice != value)
                {
                    _EnterPrice = value;

                    OnPropertyChanged(nameof(EnterPrice));
                }
            }
        }

        public float Size
        {
            get { return _Size; }
            set
            {
                if (_Size != value)
                {
                    _Size = value;

                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public float Fee
        {
            get { return _Fee; }
            set
            {
                if (_Fee != value)
                {
                    _Fee = value;

                    OnPropertyChanged(nameof(Fee));
                }
            }
        }

        public float ExitPrice
        {
            get { return _ExitPrice; }
            set
            {
                if (_ExitPrice != value)
                {
                    _ExitPrice = value;

                    OnPropertyChanged(nameof(ExitPrice));
                }
            }
        }

        public float Profit
        {
            get { return _Profit; }
            set
            {
                if (_Profit != value)
                {
                    _Profit = value;

                    OnPropertyChanged(nameof(Profit));
                }
            }
        }

        public float ProfitPercent
        {
            get { return _ProfitPercent; }
            set
            {
                if (_ProfitPercent != value)
                {
                    _ProfitPercent = value;

                    OnPropertyChanged(nameof(ProfitPercent));
                }
            }
        }

        public float SavedProfit
        {
            get { return _SavedProfit; }
            set
            {
                if (_SavedProfit != value)
                {
                    _SavedProfit = value;

                    OnPropertyChanged(nameof(SavedProfit));
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

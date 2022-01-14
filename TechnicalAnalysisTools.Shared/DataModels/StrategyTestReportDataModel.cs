using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class StrategyTestReportDataModel : INotifyPropertyChanged
    {
        static StrategyTestReportDataModel()
        {
            PropertyInfos = typeof(StrategyTestReportDataModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToArray();
        }

        private static PropertyInfo[] PropertyInfos { get; set; }

        private int _OrderCount;

        private int _SubOrderCount;

        private int _ActionCount;
		
		private StrategyTestStatusTypes _LastStrategyTestStatus;

        private int _WonSubOrderCount;

        private float _WonSubOrderPercent;

        private float _InitialDeposit;

        private float _TotalFee;

        private float _TotalProfit;

        private float _TotalProfitPercent;

        private float _TotalSavedProfit;

        private float _TotalBalance;

        [Display(Order = 10)]
        public int OrderCount
        {
            get { return _OrderCount; }
            set
            {
                if (_OrderCount != value)
                {
                    _OrderCount = value;

                    OnPropertyChanged(nameof(OrderCount));
                }
            }
        }

        [Display(Order = 11)]
        public int SubOrderCount
        {
            get { return _SubOrderCount; }
            set
            {
                if (_SubOrderCount != value)
                {
                    _SubOrderCount = value;

                    OnPropertyChanged(nameof(SubOrderCount));
                }
            }
        }

        [Display(Order = 12)]
        public int ActionCount
        {
            get { return _ActionCount; }
            set
            {
                if (_ActionCount != value)
                {
                    _ActionCount = value;

                    OnPropertyChanged(nameof(ActionCount));
                }
            }
        }

        [Display(Order = 1)]
        public StrategyTestStatusTypes LastStrategyTestStatus
		{
            get { return _LastStrategyTestStatus; }
            set
            {
                if (_LastStrategyTestStatus != value)
                {
                    _LastStrategyTestStatus = value;

                    OnPropertyChanged(nameof(LastStrategyTestStatus));
                }
            }			
		}

        [Display(Order = 2)]
        public int WonSubOrderCount
        {
            get { return _WonSubOrderCount; }
            set
            {
                if (_WonSubOrderCount != value)
                {
                    _WonSubOrderCount = value;

                    OnPropertyChanged(nameof(WonSubOrderCount));
                }
            }
        }

        [Display(Order = 3)]
        public float WonSubOrderPercent
        {
            get { return _WonSubOrderPercent; }
            set
            {
                if (_WonSubOrderPercent != value)
                {
                    _WonSubOrderPercent = value;

                    OnPropertyChanged(nameof(WonSubOrderPercent));
                }
            }
        }

        [Display(Order = 4)]
        public float InitialDeposit
        {
            get { return _InitialDeposit; }
            set
            {
                if (_InitialDeposit != value)
                {
                    _InitialDeposit = value;

                    OnPropertyChanged(nameof(InitialDeposit));
                }
            }
        }

        [Display(Order = 5)]
        public float TotalFee
        {
            get { return _TotalFee; }
            set
            {
                if (_TotalFee != value)
                {
                    _TotalFee = value;

                    OnPropertyChanged(nameof(TotalFee));
                }
            }
        }

        [Display(Order = 6)]
        public float TotalProfit
        {
            get { return _TotalProfit; }
            set
            {
                if (_TotalProfit != value)
                {
                    _TotalProfit = value;

                    OnPropertyChanged(nameof(TotalProfit));
                }
            }
        }

        [Display(Order = 7)]
        public float TotalProfitPercent
        {
            get { return _TotalProfitPercent; }
            set
            {
                if (_TotalProfitPercent != value)
                {
                    _TotalProfitPercent = value;

                    OnPropertyChanged(nameof(TotalProfitPercent));
                }
            }
        }

        [Display(Order = 8)]
        public float TotalSavedProfit
        {
            get { return _TotalSavedProfit; }
            set
            {
                if (_TotalSavedProfit != value)
                {
                    _TotalSavedProfit = value;

                    OnPropertyChanged(nameof(TotalSavedProfit));
                }
            }
        }

        [Display(Order = 9)]
        public float TotalBalance
        {
            get { return _TotalBalance; }
            set
            {
                if (_TotalBalance != value)
                {
                    _TotalBalance = value;

                    OnPropertyChanged(nameof(TotalBalance));
                }
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetDefault()
        {
            OrderCount = 0;
            ActionCount = 0;
            SubOrderCount = 0;
            WonSubOrderCount = 0;
            WonSubOrderPercent = 0;
            InitialDeposit = 0;
            TotalFee = 0;
            TotalProfit = 0;
            TotalProfitPercent = 0;
            TotalSavedProfit = 0;
            TotalBalance = 0;
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

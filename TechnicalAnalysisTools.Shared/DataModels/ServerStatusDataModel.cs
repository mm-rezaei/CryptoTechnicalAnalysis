using System;
using System.ComponentModel;
using System.Windows;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class ServerStatusDataModel : INotifyPropertyChanged
    {
        private Visibility _GridControlMainVisibility = Visibility.Collapsed;

        private Visibility _GridLoadingDataVisibility = Visibility.Collapsed;

        private float _MainProgressValue = 0;

        private float _DetailsProgressValue = 0;

        private SymbolTypes _CurrentDetailSymbolType;

        private bool _SeviceWorking = false;

        private bool _AllSymbolsSync = true;

        private BinanceConnectionStatusModes _BinanceConnectionStatus = BinanceConnectionStatusModes.Bad;

        public Visibility GridControlMainVisibility
        {
            get { return _GridControlMainVisibility; }
            set
            {
                if (_GridControlMainVisibility != value)
                {
                    _GridControlMainVisibility = value;

                    OnPropertyChanged(nameof(GridControlMainVisibility));
                }
            }
        }

        public Visibility GridLoadingDataVisibility
        {
            get { return _GridLoadingDataVisibility; }
            set
            {
                if (_GridLoadingDataVisibility != value)
                {
                    _GridLoadingDataVisibility = value;

                    OnPropertyChanged(nameof(GridLoadingDataVisibility));
                }
            }
        }

        public float MainProgressValue
        {
            get { return _MainProgressValue; }
            set
            {
                if (_MainProgressValue != value)
                {
                    _MainProgressValue = value;

                    OnPropertyChanged(nameof(MainProgressValue));
                }
            }
        }

        public float DetailsProgressValue
        {
            get { return _DetailsProgressValue; }
            set
            {
                if (_DetailsProgressValue != value)
                {
                    _DetailsProgressValue = value;

                    OnPropertyChanged(nameof(DetailsProgressValue));
                }
            }
        }

        public SymbolTypes CurrentDetailSymbolType
        {
            get { return _CurrentDetailSymbolType; }
            set
            {
                if (_CurrentDetailSymbolType != value)
                {
                    _CurrentDetailSymbolType = value;

                    OnPropertyChanged(nameof(CurrentDetailSymbolType));
                }
            }
        }

        public bool SeviceWorking
        {
            get { return _SeviceWorking; }
            set
            {
                if (_SeviceWorking != value)
                {
                    _SeviceWorking = value;

                    OnPropertyChanged(nameof(SeviceWorking));
                }
            }
        }

        public bool AllSymbolsSync
        {
            get { return _AllSymbolsSync; }
            set
            {
                if (_AllSymbolsSync != value)
                {
                    _AllSymbolsSync = value;

                    OnPropertyChanged(nameof(AllSymbolsSync));
                }
            }
        }

        public BinanceConnectionStatusModes BinanceConnectionStatus
        {
            get { return _BinanceConnectionStatus; }
            set
            {
                if (_BinanceConnectionStatus != value)
                {
                    _BinanceConnectionStatus = value;

                    OnPropertyChanged(nameof(BinanceConnectionStatus));
                }
            }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

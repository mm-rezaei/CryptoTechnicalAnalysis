using System.Collections.ObjectModel;
using System.ComponentModel;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.DataModels
{
    public class SessionEstablishmentDataModel : INotifyPropertyChanged
    {
        public SessionEstablishmentDataModel()
        {
            SessionEstablishmentHelper.FillSessionEstablishment(this);
        }

        private string _Address;
        private int _Port;
        private bool _DatabaseSupport;

        public string Address
        {
            get { return _Address; }
            set { if (_Address != value) { _Address = value; OnPropertyChanged(nameof(Address)); } }
        }

        public int Port
        {
            get { return _Port; }
            set { if (_Port != value) { _Port = value; OnPropertyChanged(nameof(Port)); } }
        }

        public bool DatabaseSupport
        {
            get { return _DatabaseSupport; }
            set { if (_DatabaseSupport != value) { _DatabaseSupport = value; OnPropertyChanged(nameof(DatabaseSupport)); } }
        }

        public ObservableCollection<SessionEstablishmentItemDataModel> Clients { get; } = new ObservableCollection<SessionEstablishmentItemDataModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SessionEstablishmentItemDataModel : INotifyPropertyChanged
    {
        private UiClientTypes _ClientType;
        private string _Username;
        private string _Password;
        private bool _IsEnabled;

        public UiClientTypes ClientType
        {
            get { return _ClientType; }
            set { if (_ClientType != value) { _ClientType = value; OnPropertyChanged(nameof(ClientType)); } }
        }

        public string Username
        {
            get { return _Username; }
            set { if (_Username != value) { _Username = value; OnPropertyChanged(nameof(Username)); } }
        }

        public string Password
        {
            get { return _Password; }
            set { if (_Password != value) { _Password = value; OnPropertyChanged(nameof(Password)); } }
        }

        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { if (_IsEnabled != value) { _IsEnabled = value; OnPropertyChanged(nameof(IsEnabled)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

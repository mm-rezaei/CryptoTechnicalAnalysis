using System;
using System.ComponentModel;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Ui.Win.DataModels
{
    [Serializable]
    internal class SessionEstablishmentDataModel : INotifyPropertyChanged
    {
        public SessionEstablishmentDataModel()
        {
            Address = "127.0.0.1";
            Port = 8080;
            ClientType = UiClientTypes.Limited;
            Username = "";
            Password = "";
            IsAuthenticated = false;
        }

        private string _Address;
        private int _Port;
        private UiClientTypes _ClientType;
        private string _Username;
        private string _Password;
        private bool _IsAuthenticated;

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

        public bool IsAuthenticated
        {
            get { return _IsAuthenticated; }
            set { if (_IsAuthenticated != value) { _IsAuthenticated = value; OnPropertyChanged(nameof(IsAuthenticated)); } }
        }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

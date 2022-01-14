using System.ComponentModel;

namespace TechnicalAnalysisTools.DataModels
{
    public class ProcessInfoDataModel : INotifyPropertyChanged
    {
        private int _ProcessId;
        private string _ProcessName;
        private string _StartTime;
        private float _ProcessCpuUsage;
        private float _TotalCpuUsage;
        private long _MemoryUsage;
        private long _PeakMemoryUsage;
        private int _ActiveThreads;
        private int _SupportedSymbol;
        private int _ConnectedUser;

        public int ProcessId
        {
            get { return _ProcessId; }
            set { if (_ProcessId != value) { _ProcessId = value; OnPropertyChanged(nameof(ProcessId)); } }
        }

        public string ProcessName
        {
            get { return _ProcessName; }
            set { if (_ProcessName != value) { _ProcessName = value; OnPropertyChanged(nameof(ProcessName)); } }
        }

        public string StartTime
        {
            get { return _StartTime; }
            set { if (_StartTime != value) { _StartTime = value; OnPropertyChanged(nameof(StartTime)); } }
        }

        public float ProcessCpuUsage
        {
            get { return _ProcessCpuUsage; }
            set { if (_ProcessCpuUsage != value) { _ProcessCpuUsage = value; OnPropertyChanged(nameof(ProcessCpuUsage)); } }
        }

        public float TotalCpuUsage
        {
            get { return _TotalCpuUsage; }
            set { if (_TotalCpuUsage != value) { _TotalCpuUsage = value; OnPropertyChanged(nameof(TotalCpuUsage)); } }
        }

        public long MemoryUsage
        {
            get { return _MemoryUsage; }
            set { if (_MemoryUsage != value) { _MemoryUsage = value; OnPropertyChanged(nameof(MemoryUsage)); } }
        }

        public long PeakMemoryUsage
        {
            get { return _PeakMemoryUsage; }
            set { if (_PeakMemoryUsage != value) { _PeakMemoryUsage = value; OnPropertyChanged(nameof(PeakMemoryUsage)); } }
        }

        public int ActiveThreads
        {
            get { return _ActiveThreads; }
            set { if (_ActiveThreads != value) { _ActiveThreads = value; OnPropertyChanged(nameof(ActiveThreads)); } }
        }

        public int SupportedSymbol
        {
            get { return _SupportedSymbol; }
            set { if (_SupportedSymbol != value) { _SupportedSymbol = value; OnPropertyChanged(nameof(SupportedSymbol)); } }
        }

        public int ConnectedUser
        {
            get { return _ConnectedUser; }
            set { if (_ConnectedUser != value) { _ConnectedUser = value; OnPropertyChanged(nameof(ConnectedUser)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

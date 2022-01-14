using System.ComponentModel;
using System.Linq;
using System.Windows;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class SymbolSelectorWindow : Window
    {
        public SymbolSelectorWindow(SymbolTypes[] symbols)
        {
            InitializeComponent();

            SymbolSelectorItems = symbols.Select(p => new SymbolSelectorItem() { Checked = false, Symbol = p }).ToArray();

            ListBoxMain.ItemsSource = SymbolSelectorItems;
        }

        private bool Result { get; set; } = false;

        public SymbolSelectorItem[] SymbolSelectorItems { get; }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Result = true;

            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;

            Close();
        }

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach(var item in SymbolSelectorItems)
            {
                item.Checked = true;
            }
        }

        public bool ShowDialogWindow()
        {
            ShowDialog();

            return Result;
        }
    }

    internal class SymbolSelectorItem : INotifyPropertyChanged
    {
        private bool _Checked;

        private SymbolTypes _Symbol;

        public bool Checked
        {
            get { return _Checked; }
            set { if (_Checked != value) { _Checked = value; OnPropertyChanged(nameof(Checked)); } }
        }

        public SymbolTypes Symbol
        {
            get { return _Symbol; }
            set { if (_Symbol != value) { _Symbol = value; OnPropertyChanged(nameof(Symbol)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

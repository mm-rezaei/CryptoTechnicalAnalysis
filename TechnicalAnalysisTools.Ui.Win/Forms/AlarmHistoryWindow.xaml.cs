using System.Collections.ObjectModel;
using System.Windows;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Ui.Win.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class AlarmHistoryWindow : Window
    {
        public AlarmHistoryWindow(ObservableCollection<SymbolAlarmDataModel> alarms)
        {
            InitializeComponent();

            GridControlAlarms.ItemsSource = alarms;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutHelper.LoadLayout(this, GridControlAlarms);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutHelper.SaveLayout(this, GridControlAlarms);

            Hide();

            e.Cancel = true;
        }

        public void CloseAlarmHistoryWindow()
        {
            Closing -= Window_Closing;

            Close();
        }
    }
}

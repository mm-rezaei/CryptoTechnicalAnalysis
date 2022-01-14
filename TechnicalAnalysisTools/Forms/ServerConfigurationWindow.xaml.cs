using System.Windows;
using System.Windows.Input;
using TechnicalAnalysisTools.DataModels;
using TechnicalAnalysisTools.Delegates;

namespace TechnicalAnalysisTools.Forms
{
    public partial class ServerConfigurationWindow : Window
    {
        public ServerConfigurationWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();

            e.Cancel = true;
        }

        private void ListBoxClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxClients.SelectedItem != null)
            {
                var client = (SessionEstablishmentItemDataModel)ListBoxClients.SelectedItem;

                if (client.IsEnabled)
                {
                    client.IsEnabled = false;

                    DisablingClientRequested?.Invoke(client.Username);
                }
                else
                {
                    client.IsEnabled = true;
                }
            }
        }

        public void CloseServerConfigurationWindow()
        {
            Closing -= Window_Closing;

            Close();
        }

        public event DisablingClientRequestedHandler DisablingClientRequested;
    }
}

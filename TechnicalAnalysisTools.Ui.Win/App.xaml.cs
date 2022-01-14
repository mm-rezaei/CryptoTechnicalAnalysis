using System;
using System.Threading.Tasks;
using System.Windows;
using TechnicalAnalysisTools.Ui.Win.DataModels;
using TechnicalAnalysisTools.Ui.Win.Forms;

namespace TechnicalAnalysisTools.Ui.Win
{
    public partial class App : Application
    {
        private bool Connected { get; set; }

        private MainWindow MainForm { get; set; }

        private async Task<bool> LoginWindow_SessionEstablishmentRequest(SessionEstablishmentDataModel sessionEstablishment)
        {
            Connected = await MainForm.Connect(sessionEstablishment);

            return Connected;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainForm = new MainWindow();

            var loginWindow = new LoginWindow();

            loginWindow.SessionEstablishmentRequest += LoginWindow_SessionEstablishmentRequest;

            loginWindow.ShowDialog();

            if (Connected)
            {
                MainForm.ShowDialog();
            }

            Environment.Exit(0);
        }
    }
}

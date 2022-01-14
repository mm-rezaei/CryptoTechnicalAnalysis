using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechnicalAnalysisTools.Shared.Auxiliaries;
using TechnicalAnalysisTools.Ui.Win.DataModels;
using TechnicalAnalysisTools.Ui.Win.Delegates;
using TechnicalAnalysisTools.Ui.Win.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            SessionEstablishment = Resources["SessionEstablishment"] as SessionEstablishmentDataModel;

            InitializeWindow();
        }

        private SessionEstablishmentDataModel SessionEstablishment { get; }

        private void InitializeWindow()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Title += " " + fileVersionInfo.FileVersion.ToString();

            if (!InitializeSessionEstablishment(SessionEstablishment))
            {
                SessionEstablishment.Address = "127.0.0.1";
                SessionEstablishment.Port = 8080;
                SessionEstablishment.Username = "";
                SessionEstablishment.Password = "";
                SessionEstablishment.IsAuthenticated = false;
            }
        }

        private bool InitializeSessionEstablishment(SessionEstablishmentDataModel sessionEstablishment)
        {
            var result = false;

            if (File.Exists(ClientAddressHelper.ClientInformationFile))
            {
                try
                {
                    var aesEncryption = new AesEncryptionAuxiliary();

                    using (var memoryStream = new MemoryStream(aesEncryption.Decrypt(File.ReadAllBytes(ClientAddressHelper.ClientInformationFile))))
                    {
                        var formatter = new BinaryFormatter();

                        var storedSessionEstablishment = (SessionEstablishmentDataModel)formatter.Deserialize(memoryStream);

                        sessionEstablishment.Address = storedSessionEstablishment.Address;
                        sessionEstablishment.Port = storedSessionEstablishment.Port;
                        sessionEstablishment.Username = storedSessionEstablishment.Username;
                        sessionEstablishment.Password = "";
                        sessionEstablishment.IsAuthenticated = false;

                        result = true;
                    }
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }

        private void GridMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonConnect_Click(ButtonConnect, new RoutedEventArgs());

                e.Handled = true;
            }
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            GridMain.IsEnabled = false;

            TextBlockConnectionStatus.Text = "Connecting...";

            await Task.Run(() =>
            {
                if (SessionEstablishmentRequest != null)
                {
                    if (SessionEstablishmentRequest.Invoke(SessionEstablishment).Result)
                    {
                        //
                        var aesEncryption = new AesEncryptionAuxiliary();

                        using (var memoryStream = new MemoryStream())
                        {
                            var formatter = new BinaryFormatter();

                            formatter.Serialize(memoryStream, SessionEstablishment);

                            if (File.Exists(ClientAddressHelper.ClientInformationFile))
                            {
                                try
                                {
                                    File.Delete(ClientAddressHelper.ClientInformationFile);
                                }
                                catch
                                {

                                }
                            }

                            File.WriteAllBytes(ClientAddressHelper.ClientInformationFile, aesEncryption.Encrypt(memoryStream.ToArray()));
                        }

                        //
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            TextBlockConnectionStatus.Text = "Connected";

                            Close();
                        }));
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            TextBlockConnectionStatus.Text = "Disconnected";

                            GridMain.IsEnabled = true;
                        }));
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextBlockConnectionStatus.Text = "Error occured!";
                    }));
                }
            });
        }

        public event SessionEstablishmentRequestHandler SessionEstablishmentRequest;
    }
}

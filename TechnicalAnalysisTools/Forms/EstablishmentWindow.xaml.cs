using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TechnicalAnalysisTools.Auxiliaries;
using TechnicalAnalysisTools.DataModels;
using TechnicalAnalysisTools.DataObjects;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Services;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Forms
{
    public partial class EstablishmentWindow : Window
    {
        public EstablishmentWindow()
        {
            InitializeComponent();

            SessionEstablishment = Resources["SessionEstablishment"] as SessionEstablishmentDataModel;

            ServerConfiguration.DataContext = SessionEstablishment;
            ServerConfiguration.DisablingClientRequested += (username) =>
            {
                CryptoServer?.DisableClient(username);
            };

            InitializeWindow();

            ProcessInfo = Resources["ProcessInfo"] as ProcessInfoDataModel;
        }

        private Semaphore ServerLogSemaphore { get; set; }

        private string ServerLogs { get; set; }

        private SessionEstablishmentDataModel SessionEstablishment { get; }

        private CryptoServerService CryptoServer { get; set; }

        private bool ServerListenActived { get; set; }

        private DateTime? ServerStartTime { get; set; }

        private ProcessInfoDataModel ProcessInfo { get; }

        private PerformanceCounter ProcessCpuPerformanceCounter { get; set; }

        private PerformanceCounter TotalCpuPerformanceCounter { get; set; }

        private Thread ProcessMonitorThread { get; set; }

        private ServerConfigurationWindow ServerConfiguration { get; } = new ServerConfigurationWindow();

        private bool SessionEstablishmentControlsEnabled
        {
            set
            {
                ServerConfiguration.TextEditAddress.IsEnabled = value;
                ServerConfiguration.TextBoxPort.IsEnabled = value;
                ServerConfiguration.ComboBoxDatabaseSupport.IsEnabled = value;
            }
        }

        private void InitializeWindow()
        {
            //
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Title += " " + fileVersionInfo.FileVersion.ToString();

            //
            ServerListenActived = false;

            //
            ServerLogSemaphore = new Semaphore(1, 1);

            //
            ServerConfiguration.ComboBoxDatabaseSupport.ItemsSource = new List<bool> { false, true };
        }

        private void RefreshWindow()
        {
            Action EmptyDelegate = delegate () { };

            Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        private void WindowEstablishment_Loaded(object sender, RoutedEventArgs e)
        {
            //
            if (!NetworkEncryptionAuxiliary.CreatePublicKey())
            {
                MenuItemStartServer.IsEnabled = false;
                MenuItemStopServer.IsEnabled = false;
            }

            //
            ProcessMonitorThread = new Thread(() =>
            {
                while (true)
                {
                    //
                    ServerLogSemaphore.WaitOne();

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(ServerLogs))
                        {
                            Dispatcher.BeginInvoke(new Action<string>((log) =>
                            {
                                if (TextBoxLog.Text.Length > 100000)
                                {
                                    TextBoxLog.Text = "";
                                }

                                if (!string.IsNullOrWhiteSpace(TextBoxLog.Text))
                                {
                                    TextBoxLog.AppendText(Environment.NewLine);
                                }

                                TextBoxLog.AppendText(log);

                                if (!TextBoxLog.IsFocused)
                                {
                                    TextBoxLog.ScrollToEnd();
                                }
                            }), DispatcherPriority.Send, ServerLogs);

                            ServerLogs = "";
                        }
                    }
                    finally
                    {
                        ServerLogSemaphore.Release();
                    }

                    //
                    var currentTime = DateTime.UtcNow;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextBlockCurrentTime.Text = currentTime.DayOfWeek + " " + currentTime.ToString("yyyy/MM/dd HH:mm:ss");

                        if (ServerStartTime.HasValue)
                        {
                            ProcessInfo.StartTime = ServerStartTime.Value.DayOfWeek + " " + ServerStartTime.Value.ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        else
                        {
                            ProcessInfo.StartTime = "-";
                        }
                    }));

                    ProcessInfo.ConnectedUser = CryptoServer == null ? 0 : CryptoServer.GetConnectedUiClientsCount();

                    if (ProcessCpuPerformanceCounter == null)
                    {
                        ProcessCpuPerformanceCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true);
                        TotalCpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                    }

                    ProcessInfo.ProcessCpuUsage = (float)Math.Round(ProcessCpuPerformanceCounter.NextValue() / Environment.ProcessorCount, 2);
                    ProcessInfo.TotalCpuUsage = (float)Math.Round(TotalCpuPerformanceCounter.NextValue(), 2);

                    if (currentTime.Second == 0)
                    {
                        var currentProcessInfo = Process.GetCurrentProcess();

                        ProcessInfo.ProcessId = currentProcessInfo.Id;
                        ProcessInfo.ProcessName = currentProcessInfo.ProcessName;
                        ProcessInfo.MemoryUsage = currentProcessInfo.WorkingSet64 / (1024 * 1024);
                        ProcessInfo.PeakMemoryUsage = currentProcessInfo.PeakWorkingSet64 / (1024 * 1024);
                        ProcessInfo.ActiveThreads = currentProcessInfo.Threads.Count;
                        ProcessInfo.SupportedSymbol = CryptoServer == null ? 0 : CryptoServer.SupportedSymbols.Length;
                    }

                    Thread.Sleep(1000);
                }
            });

            ProcessMonitorThread.Start();
        }

        private void WindowEstablishment_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TextBoxLog.IsFocused)
            {
                TextBoxLog.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void WindowEstablishment_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                NetworkEncryptionAuxiliary.DestroyPublicKey();
            }
            catch
            {

            }

            try
            {
                ProcessMonitorThread?.Abort();
            }
            catch
            {

            }

            try
            {
                CryptoServer?.StopListen();
            }
            catch
            {

            }

            try
            {
                CryptoServer?.Stop();
            }
            catch
            {

            }

            ServerConfiguration.CloseServerConfigurationWindow();

            Environment.Exit(0);
        }

        private void CryptoServer_LogReceived(LogDataObject log)
        {
            var logMessage = string.Format("{0}\t{1}\t{2}", log.LogTime.ToString("yyyy/MM/dd HH:mm:ss"), log.Action, log.Message);

            ServerLogSemaphore.WaitOne();

            File.AppendAllLines(ServerAddressHelper.ServerLogsFile, logMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

            try
            {
                if (string.IsNullOrWhiteSpace(ServerLogs))
                {
                    ServerLogs = "";
                }
                else
                {
                    ServerLogs += Environment.NewLine;
                }

                ServerLogs += logMessage;
            }
            finally
            {
                ServerLogSemaphore.Release();
            }
        }

        private void CryptoServer_MainProgressValueChanged(float value, object detail)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressBarAllDataLoading.Value = value;
            }));

            if (value >= 100f)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    BorderAllDataLoading.Visibility = Visibility.Collapsed;
                }));
            }
        }

        private void CryptoServer_AutoSavingServiceWorkingNotified(bool seviceWorking)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (seviceWorking)
                {
                    TextBlockDataSaving.Foreground = Brushes.White;
                    TextBlockDataSaving.Background = Brushes.Red;
                    TextBlockDataSaving.Text = "Dont close window!";
                }
                else
                {
                    TextBlockDataSaving.Foreground = Brushes.Black;
                    TextBlockDataSaving.Background = Brushes.Transparent;
                    TextBlockDataSaving.Text = "";
                }
            }), DispatcherPriority.Send);
        }

        private void CryptoServer_StatusBarInformationReceived(Tuple<DateTime, bool> information)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (information.Item2)
                {
                    TextBlockSymbolSync.Foreground = Brushes.Black;
                    TextBlockSymbolSync.Background = Brushes.Transparent;
                    TextBlockSymbolSync.Text = "All symbols sync";
                }
                else
                {
                    TextBlockSymbolSync.Foreground = Brushes.White;
                    TextBlockSymbolSync.Background = Brushes.Red;
                    TextBlockSymbolSync.Text = "Symbols not sync";
                }
            }), DispatcherPriority.Send);
        }

        private void CryptoServer_BinanceConnectionStatus(BinanceConnectionStatusModes status)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (status)
                {
                    case BinanceConnectionStatusModes.Good:
                        {
                            EllipseServiceActivation.Fill = Brushes.Green;
                        }
                        break;
                    case BinanceConnectionStatusModes.NotGood:
                        {
                            EllipseServiceActivation.Fill = Brushes.Goldenrod;
                        }
                        break;
                    case BinanceConnectionStatusModes.Bad:
                        {
                            EllipseServiceActivation.Fill = Brushes.Red;
                        }
                        break;
                }
            }));
        }

        private void TextBoxLog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBoxLog.Text = "";
            }
            catch
            {

            }
        }

        private void TextBoxLog_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBoxLog.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            TextBoxLog.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        private void TextBoxLog_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBoxLog.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            TextBoxLog.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuItemServerConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ServerConfiguration.Show();
        }

        private void MenuItemStartServer_Click(object sender, RoutedEventArgs e)
        {
            MenuItemStartServer.IsEnabled = false;
            MenuItemStopServer.IsEnabled = false;
            SessionEstablishmentControlsEnabled = false;

            RefreshWindow();

            if (CryptoServer == null)
            {
                CryptoServer = new CryptoServerService(SessionEstablishment);

                CryptoServer.LogReceived += CryptoServer_LogReceived;
                CryptoServer.MainProgressValueChanged += CryptoServer_MainProgressValueChanged;
                CryptoServer.AutoSavingServiceWorkingNotified += CryptoServer_AutoSavingServiceWorkingNotified;
                CryptoServer.StatusBarInformationReceived += CryptoServer_StatusBarInformationReceived;
                CryptoServer.BinanceConnectionStatus += CryptoServer_BinanceConnectionStatus;

                CryptoServer.Start();
            }

            if (!ServerListenActived)
            {
                SessionEstablishmentHelper.FillSessionEstablishmentClients(SessionEstablishment);

                if (CryptoServer.StartListen())
                {
                    ServerListenActived = true;

                    ServerStartTime = DateTime.UtcNow;

                    MenuItemStopServer.IsEnabled = true;
                }
                else
                {
                    MenuItemStartServer.IsEnabled = true;
                    SessionEstablishmentControlsEnabled = true;

                    MessageBox.Show("Starting server is not possible. Contact administrator before retry again.", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemStopServer_Click(object sender, RoutedEventArgs e)
        {
            MenuItemStartServer.IsEnabled = false;
            MenuItemStopServer.IsEnabled = false;
            SessionEstablishmentControlsEnabled = false;

            RefreshWindow();

            if (ServerListenActived)
            {
                CryptoServer.StopListen();

                ServerListenActived = false;

                ServerStartTime = null;

                MenuItemStartServer.IsEnabled = true;
                SessionEstablishmentControlsEnabled = true;
            }
        }

        private void MenuItemAddCompiledScript_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Text file (*.dll)|*.dll";
            fileDialog.CheckFileExists = true;

            var fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult.HasValue && fileDialogResult.Value)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    var result = false;

                    try
                    {
                        //
                        var fileInfo = new FileInfo(fileDialog.FileName);

                        var compiledAlarmDataPath = Path.Combine(ServerAddressHelper.CompiledAlarmDataFolder, fileInfo.Name);
                        var compiledAlarmDataBackupPath = Path.Combine(ServerAddressHelper.CompiledAlarmDataBackupFolder, fileInfo.Name);

                        //
                        File.Copy(fileDialog.FileName, compiledAlarmDataPath);
                        File.Copy(fileDialog.FileName, compiledAlarmDataBackupPath);

                        if (AssemblyHelper.LoadAssemblyFile(compiledAlarmDataPath))
                        {
                            result = true;
                        }
                        else
                        {
                            File.Delete(compiledAlarmDataPath);
                            File.Delete(compiledAlarmDataBackupPath);
                        }
                    }
                    catch
                    {
                        result = false;
                    }

                    if (result)
                    {
                        MessageBox.Show("Loading compiled script file is successful.", "Successful Operation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Loading compiled script file is not successful. Modify it and try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MenuItemRemoveCompiledScript_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Text file (*.dll)|*.dll";
            fileDialog.CheckFileExists = true;

            var fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult.HasValue && fileDialogResult.Value)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    bool result;

                    try
                    {
                        result = AssemblyHelper.UnloadAssemblyFile(fileDialog.FileName);

                        if (result)
                        {
                            File.Delete(fileDialog.FileName);
                        }
                    }
                    catch
                    {
                        result = false;
                    }

                    if (result)
                    {
                        MessageBox.Show("Unloading compiled script file is successful.", "Successful Operation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Unloading compiled script file is not successful. Modify it and try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}

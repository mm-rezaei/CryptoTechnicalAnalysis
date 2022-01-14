using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.Data;
using DevExpress.Xpf.Bars;
using Microsoft.Win32;
using TechnicalAnalysisTools.Forms;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.DataObjects;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;
using TechnicalAnalysisTools.Ui.Win.DataModels;
using TechnicalAnalysisTools.Ui.Win.Helpers;
using TechnicalAnalysisTools.Ui.Win.Services;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitializeWindow();
        }

        private SessionEstablishmentDataModel SessionEstablishment { get; set; }

        private Semaphore CryptoClientConnectionSemaphore { get; } = new Semaphore(1, 1);

        private CryptoClientService CryptoClient { get; set; }

        private Thread ProcessMonitorThread { get; set; }

        private List<SymbolTypes> SupportedSymbols { get; } = new List<SymbolTypes>();

        private bool ViewScriptStatus { get; set; }

        private bool EvaluateAlarmStatus { get; set; }

        private ObservableCollection<SymbolDataModel> MarketData { get; } = new ObservableCollection<SymbolDataModel>();

        private ObservableCollection<SymbolAlarmDataModel> Alarms { get; } = new ObservableCollection<SymbolAlarmDataModel>();

        private ObservableCollection<SymbolAlarmDataModel> AlarmsHistory { get; } = new ObservableCollection<SymbolAlarmDataModel>();

        private List<StrategyTestDataModel> StrategyTests { get; } = new List<StrategyTestDataModel>();

        private HashSet<Guid> StopStrategyTestNotificationToServer { get; } = new HashSet<Guid>();

        private AlarmWindow AlarmWindow { get; set; }

        private AlarmHistoryWindow AlarmHistoryWindow { get; set; }

        private bool EnabledSound { get; set; } = true;

        private SymbolTypes DetailsProgressMessage
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var message = string.Format("'{0}' data set is loading...", value);

                    if (TextBlockSymbolDataLoadingMessage.Text != message)
                    {
                        TextBlockSymbolDataLoadingMessage.Text = message;
                    }
                }));
            }
        }

        private float DetailsProgressValue
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ProgressBarSymbolDataLoading.Value != value)
                    {
                        ProgressBarSymbolDataLoading.Value = value;
                    }
                }));
            }
        }

        private float MainProgressValue
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ProgressBarAllDataLoading.Value != value)
                    {
                        ProgressBarAllDataLoading.Value = value;
                    }
                }));
            }
        }

        private bool SeviceWorking
        {
            set
            {
                // Nothing
            }
        }

        private bool AllSymbolsSync
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (value)
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
        }

        private BinanceConnectionStatusModes BinanceConnectionStatus
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    switch (value)
                    {
                        case BinanceConnectionStatusModes.Good:
                            {
                                EllipseServiceActivation.Fill = Brushes.Green;

                                if (EnabledSound)
                                {
                                    var player = new SoundPlayer(SharedAddressHelper.BinanceConnectionOnWavFile);
                                    player.Play();
                                }
                            }
                            break;
                        case BinanceConnectionStatusModes.NotGood:
                            {
                                EllipseServiceActivation.Fill = Brushes.Goldenrod;

                                if (EnabledSound)
                                {
                                    var player = new SoundPlayer(SharedAddressHelper.BinanceConnectionOffWavFile);
                                    player.Play();
                                }
                            }
                            break;
                        case BinanceConnectionStatusModes.Bad:
                            {
                                EllipseServiceActivation.Fill = Brushes.Red;

                                if (EnabledSound)
                                {
                                    var player = new SoundPlayer(SharedAddressHelper.BinanceConnectionOffWavFile);
                                    player.Play();
                                }
                            }
                            break;
                    }
                }), DispatcherPriority.Send);
            }
        }

        private void InitializeWindow()
        {
            AlarmWindow = new AlarmWindow(Alarms);

            AlarmWindow.SeenAlarmsReceived += async (alarms) =>
            {
                if (alarms == null)
                {
                    await Task.Run(() => HandleSeenAllAlarm());
                }
                else if (alarms != null && alarms.Count != 0)
                {
                    await Task.Run(() => HandleSeenAlarm(alarms[0].Id));
                }
            };

            AlarmWindow.EnableDisableAlarmsReceived += async (alarms) =>
            {
                if (alarms != null && alarms.Count != 0)
                {
                    await Task.Run(() => HandleEnableDisableAlarm(alarms[0].Id));
                }
            };

            AlarmWindow.AlarmScriptRequested += async (id) =>
            {
                return await HandleReadAlarmScript(id);
            };

            AlarmWindow.EvaluateAlarmRequested += async (id, datetime) =>
            {
                return await HandleEvaluateAlarm(id, datetime);
            };

            AlarmWindow.RemovingAlarmsReceived += async (alarms) =>
            {
                if (alarms != null && alarms.Count != 0)
                {
                    await Task.Run(() => HandleDeleteAlarm(alarms[0].Id));
                }
            };

            AlarmHistoryWindow = new AlarmHistoryWindow(AlarmsHistory);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GridControlMain.ItemsSource = MarketData;

            LayoutHelper.LoadLayout(this, GridControlMain);
            LayoutHelper.LoadLayout(this, GridControlTimeFrames);
            LayoutHelper.LoadLayout(this, GridControlAlarms);

            ProcessMonitorThread = new Thread(() =>
            {
                while (true)
                {
                    var currentTime = DateTime.UtcNow;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextBlockCurrentTime.Text = currentTime.DayOfWeek + " " + currentTime.ToString("yyyy/MM/dd HH:mm:ss");
                    }));

                    Thread.Sleep(1000);
                }
            });

            ProcessMonitorThread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CryptoClient != null)
            {
                CryptoClient.Connected -= CryptoClient_Connected;
                CryptoClient.CommandDataReceived -= CryptoClient_CommandDataReceived;
                CryptoClient.Closed -= CryptoClient_Closed;
            }

            LayoutHelper.SaveLayout(this, GridControlMain);
            LayoutHelper.SaveLayout(this, GridControlTimeFrames);
            LayoutHelper.SaveLayout(this, GridControlAlarms);

            try
            {
                ProcessMonitorThread?.Abort();
            }
            catch
            {

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            AlarmWindow.CloseAlarmWindow();
            AlarmHistoryWindow.CloseAlarmHistoryWindow();

            Environment.Exit(0);
        }

        private async void MenuItemStart_Click(object sender, RoutedEventArgs e)
        {
            var result = true;

            MenuItemStart.IsEnabled = false;

            if (CryptoClient != null)
            {
                result = await CryptoClient.Send(new CommandDataObject() { Command = CommandTypes.StartTechnicalAnalysis }, false, null);
            }

            if (!result)
            {
                MenuItemStart.IsEnabled = true;
            }
        }

        private void MenuItemLiveHistory_Click(object sender, RoutedEventArgs e)
        {
            var symbolSelector = new SymbolSelectorWindow(SupportedSymbols.ToArray());

            if (symbolSelector.ShowDialogWindow())
            {
                var history = new LiveHistoryWindow(ViewScriptStatus, EvaluateAlarmStatus, symbolSelector.SymbolSelectorItems.Where(p => p.Checked).Select(p => p.Symbol).ToArray());

                history.AlarmScriptRequested += async (id) =>
                {
                    return await HandleReadAlarmScript(id);
                };

                history.LiveHistoryRequested += async (datetime, symbols) =>
                {
                    return await HandleLiveHistory(datetime, symbols);
                };

                history.EvaluateAlarmRequested += async (id, datetime) =>
                {
                    return await HandleEvaluateAlarm(id, datetime);
                };

                history.Show();
            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuItemShowAlarmWindow_Click(object sender, RoutedEventArgs e)
        {
            AlarmWindow.Show();

            AlarmWindow.Activate();
        }

        private void MenuItemShowAlarmHistoryWindow_Click(object sender, RoutedEventArgs e)
        {
            AlarmHistoryWindow.Show();

            AlarmHistoryWindow.Activate();
        }

        private void MenuItemNewAlarmScript_Click(object sender, RoutedEventArgs e)
        {
            var alarmFactory = new AlarmFactoryWindow();

            alarmFactory.Show();
        }

        private void MenuItemEditAlarmScript_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Text file (*.txt)|*.txt";
            fileDialog.CheckFileExists = true;

            var fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult.HasValue && fileDialogResult.Value)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    var script = File.ReadAllText(fileDialog.FileName);

                    string name = "";
                    SymbolTypes symbol = SymbolTypes.BtcUsdt;
                    PositionTypes position = PositionTypes.Long;

                    var alarm = AlarmHelper.ConvertStringToAlarmItem(script, ref name, ref symbol, ref position);

                    if (alarm != null)
                    {
                        var alarmFactory = new AlarmFactoryWindow(alarm, name, symbol, position);

                        alarmFactory.Show();
                    }
                    else
                    {
                        MessageBox.Show("The script file is not valid. Modify it and try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MenuItemBackTestNewStrategy_Click(object sender, RoutedEventArgs e)
        {
            var strategyTest = new StrategyTestWindow();

            strategyTest.StrategyTestRequested += async (strategyTestData) =>
            {
                return await HandleStrategyTest(strategyTestData);
            };

            strategyTest.StrategyTestStopRequested += (strategyId) =>
            {
                HandleStrategyTestStop(strategyId);
            };

            strategyTest.Show();
        }

        private async void MenuItemRunAlarmScripts_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Text file (*.txt)|*.txt";
            fileDialog.CheckFileExists = true;
            fileDialog.Multiselect = true;

            var fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult.HasValue && fileDialogResult.Value && fileDialog.FileNames.Length != 0)
            {
                foreach (var file in fileDialog.FileNames)
                {
                    if (File.Exists(file))
                    {
                        var script = File.ReadAllText(file);

                        var fileInfo = new FileInfo(file);

                        var filename = fileInfo.Name.Replace(fileInfo.Extension, "");

                        if (!await HandleRunAlarms(script, filename))
                        {
                            MessageBox.Show("The script file is not valid. Modify it and try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private async void MenuItemRunTemplateAlarmScript_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Text file (*.txt)|*.txt";
            fileDialog.CheckFileExists = true;

            var fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult.HasValue && fileDialogResult.Value)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    var symbolSelector = new SymbolSelectorWindow(SupportedSymbols.ToArray());

                    if (symbolSelector.ShowDialogWindow())
                    {
                        var script = File.ReadAllText(fileDialog.FileName);

                        var fileInfo = new FileInfo(fileDialog.FileName);

                        var filename = fileInfo.Name.Replace(fileInfo.Extension, "");

                        if (!await HandleRunTemplateAlarm(script, symbolSelector.SymbolSelectorItems.Where(p => p.Checked).Select(p => p.Symbol).ToArray(), filename))
                        {
                            MessageBox.Show("The script file is not valid. Modify it and try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private async void MenuItemSeenAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                await Task.Run(() => HandleSeenAlarm(alarm.Id));
            }
        }

        private async void MenuItemSeenAllAlarm_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => HandleSeenAllAlarm());
        }

        private async void MenuItemEnableDisableAlarm_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                await Task.Run(() => HandleEnableDisableAlarm(alarm.Id));
            }
        }

        private async void MenuItemViewScript_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                var script = await HandleReadAlarmScript(alarm.Id);

                var scriptViewer = new AlarmScriptViewerWindow(alarm.Name, script);

                scriptViewer.Show();
            }
        }

        private async void MenuItemEvaluateAlarmViewerWindow_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                //
                var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                var lastAlarm = MarketData.First(predicate => predicate.Symbol == alarm.Symbol).LastMinuteCandle;

                var evaluatedAlarmItem = await HandleEvaluateAlarm(alarm.Id, lastAlarm);

                //
                var alarmEvaluationViewerWindow = new AlarmEvaluationViewerWindow(evaluatedAlarmItem, alarm.Name, alarm.Symbol, lastAlarm);

                alarmEvaluationViewerWindow.Show();
            }
        }

        private async void MenuItemDeleteAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                await Task.Run(() => HandleDeleteAlarm(alarm.Id));
            }
        }

        private void ImageSound_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (EnabledSound)
            {
                EnabledSound = false;

                ImageSound.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/Mute.png"));
            }
            else
            {
                EnabledSound = true;

                ImageSound.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/Unmute.png"));
            }
        }

        private void CryptoClient_Connected(object sender, EventArgs e)
        {
            // Nothing
        }

        private void CryptoClient_CommandDataReceived(CommandDataObject command)
        {
            if (command != null)
            {
                switch (command.Command)
                {
                    case CommandTypes.ClientInitializedData:
                        {
                            if (command.Parameter != null && command.Parameter is UiClientInitializedDataObject)
                            {
                                HandleClientInitializedData((UiClientInitializedDataObject)command.Parameter);
                            }
                        }
                        break;
                    case CommandTypes.MenuItemChanged:
                        {
                            if (command.Parameter != null && command.Parameter is object[])
                            {
                                var parameters = (object[])command.Parameter;

                                if (parameters.Length == 2 && parameters[0] != null && parameters[1] != null && parameters[0] is CommandTypes && parameters[1] is bool)
                                {
                                    HandleMenuItemsStatusChanged((CommandTypes)parameters[0], (bool)parameters[1]);
                                }
                            }
                        }
                        break;
                    case CommandTypes.ServerStatusPropertyChanged:
                        {
                            if (command.Parameter != null && command.Parameter is object[])
                            {
                                var parameters = (object[])command.Parameter;

                                if (parameters.Length == 2 && parameters[0] != null && parameters[1] != null && parameters[0] is string)
                                {
                                    HandleServerStatusPropertyChanged((string)parameters[0], parameters[1]);
                                }
                            }
                        }
                        break;
                    case CommandTypes.MarketData:
                        {
                            if (command.Parameter != null && command.Parameter is List<SymbolDataModel>)
                            {
                                var marketData = (List<SymbolDataModel>)command.Parameter;

                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    foreach (var symbolDataModel in MarketData)
                                    {
                                        var receivedSymbolDataModel = marketData.FirstOrDefault(p => p.Symbol == symbolDataModel.Symbol);

                                        if (receivedSymbolDataModel != null)
                                        {
                                            ReflectionHelper.CopyValuableProperties(receivedSymbolDataModel, symbolDataModel);

                                            foreach (var symbolTimeFrameDataModel in symbolDataModel.SymbolTimeFrames)
                                            {
                                                var receivedSymbolTimeFrameDataModel = receivedSymbolDataModel.SymbolTimeFrames.FirstOrDefault(p => p.TimeFrame == symbolTimeFrameDataModel.TimeFrame);

                                                if (receivedSymbolTimeFrameDataModel != null)
                                                {
                                                    ReflectionHelper.CopyValuableProperties(receivedSymbolTimeFrameDataModel, symbolTimeFrameDataModel);
                                                }
                                            }

                                            symbolDataModel.SymbolAlarms.Clear();

                                            foreach (var alarm in receivedSymbolDataModel.SymbolAlarms)
                                            {
                                                symbolDataModel.SymbolAlarms.Add(alarm);
                                            }

                                            SupportResistanceHelper.FillSupportsResistances(symbolDataModel);

                                            GridControlSupportResistance.SortBy(GridControlSupportResistance.Columns["Price"], ColumnSortOrder.Descending);

                                            GridControlSupportResistance.RefreshData();
                                        }
                                    }
                                }), DispatcherPriority.Send);
                            }
                        }
                        break;
                    case CommandTypes.Alarms:
                        {
                            if (command.Parameter != null && command.Parameter is List<SymbolAlarmDataModel>)
                            {
                                var alarms = (List<SymbolAlarmDataModel>)command.Parameter;

                                HandleNewAlarmsReceived(alarms);
                            }
                        }
                        break;
                    case CommandTypes.AlarmsHistory:
                        {
                            if (command.Parameter != null && command.Parameter is List<SymbolAlarmDataModel>)
                            {
                                var alarms = (List<SymbolAlarmDataModel>)command.Parameter;

                                HandleNewAlarmsHistoryReceived(alarms);
                            }
                        }
                        break;
                    case CommandTypes.SeenAlarmResponse:
                        {
                            HandleSeenAlarmResponse(command);
                        }
                        break;
                    case CommandTypes.EnableDisableAlarmResponse:
                        {
                            HandleEnableDisableAlarmResponse(command);
                        }
                        break;
                    case CommandTypes.DeleteAlarmResponse:
                        {
                            HandleDeleteAlarmResponse(command);
                        }
                        break;
                    case CommandTypes.TestStrategyStatus:
                        {
                            HandleStrategyTestStatus(command);
                        }
                        break;
                    default:
                        {
                            throw new Exception("Unhandled received command.");
                        }
                }
            }
        }

        private async void CryptoClient_Closed(object sender, EventArgs e)
        {
            var cryptoClient = (CryptoClientService)sender;

            CryptoClientConnectionSemaphore.WaitOne();

            try
            {
                if (CryptoClient != null && cryptoClient == CryptoClient)
                {
                    //
                    var strategyTests = StrategyTests.ToArray();

                    StrategyTests.Clear();

                    foreach (var strategyTest in strategyTests)
                    {
                        strategyTest.OnStrategyTestStatusUpdated(new StrategyTestStatusDataModel() { Id = strategyTest.Id, StrategyTestStatusType = StrategyTestStatusTypes.Error });
                    }

                    //
                    CryptoClient = null;

                    if (EnabledSound)
                    {
                        var player = new SoundPlayer(SharedAddressHelper.ChanelConnectionOffWavFile);
                        player.Play();
                    }

                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (await Connect(SessionEstablishment))
                            {
                                break;
                            }
                        }
                    });
                }
            }
            finally
            {
                CryptoClientConnectionSemaphore.Release();
            }
        }

        private void HandleClientInitializedData(UiClientInitializedDataObject initializedDataObject)
        {
            if (initializedDataObject != null)
            {
                if (initializedDataObject.SupportedSymbols != null)
                {
                    SupportedSymbols.Clear();

                    SupportedSymbols.AddRange(initializedDataObject.SupportedSymbols);
                }

                if (initializedDataObject.MenuItemsStatus != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MenuItemStart.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.StartTechnicalAnalysis];
                        MenuItemLiveHistory.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.LiveHistory];
                        MenuItemShowAlarmWindow.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.Alarms];
                        MenuItemShowAlarmHistoryWindow.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.AlarmsHistory];
                        MenuItemNewAlarmScript.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.NewAlarm];
                        MenuItemEditAlarmScript.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.EditAlarm];
                        MenuItemRunAlarmScripts.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.RunAlarms];
                        MenuItemRunTemplateAlarmScript.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.RunTemplateAlarm];
                        MenuItemBackTestNewStrategy.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.TestNewStrategy];

                        MenuItemSeenAlarm.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.SeenAlarm];
                        MenuItemSeenAllAlarm.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.SeenAllAlarm];
                        MenuItemEnableDisableAlarm.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.EnableDisableAlarm];
                        MenuItemViewScript.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.ReadAlarmScript];
                        MenuItemEvaluateAlarmViewerWindow.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.EvaluateAlarm];
                        MenuItemDeleteAlarm.IsEnabled = initializedDataObject.MenuItemsStatus[CommandTypes.DeleteAlarm];

                        AlarmWindow.SetMenuItemStatus(CommandTypes.SeenAlarm, initializedDataObject.MenuItemsStatus[CommandTypes.SeenAlarm]);
                        AlarmWindow.SetMenuItemStatus(CommandTypes.SeenAllAlarm, initializedDataObject.MenuItemsStatus[CommandTypes.SeenAllAlarm]);
                        AlarmWindow.SetMenuItemStatus(CommandTypes.EnableDisableAlarm, initializedDataObject.MenuItemsStatus[CommandTypes.EnableDisableAlarm]);
                        AlarmWindow.SetMenuItemStatus(CommandTypes.ReadAlarmScript, initializedDataObject.MenuItemsStatus[CommandTypes.ReadAlarmScript]);
                        AlarmWindow.SetMenuItemStatus(CommandTypes.EvaluateAlarm, initializedDataObject.MenuItemsStatus[CommandTypes.EvaluateAlarm]);
                        AlarmWindow.SetMenuItemStatus(CommandTypes.DeleteAlarm, initializedDataObject.MenuItemsStatus[CommandTypes.DeleteAlarm]);

                        ViewScriptStatus = initializedDataObject.MenuItemsStatus[CommandTypes.ReadAlarmScript];
                        EvaluateAlarmStatus = initializedDataObject.MenuItemsStatus[CommandTypes.EvaluateAlarm];
                    }), DispatcherPriority.Send);
                }

                if (initializedDataObject.ServerStatus != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        GridControlMain.Visibility = initializedDataObject.ServerStatus.GridControlMainVisibility;
                        GridLoadingData.Visibility = initializedDataObject.ServerStatus.GridLoadingDataVisibility;
                        DetailsProgressMessage = initializedDataObject.ServerStatus.CurrentDetailSymbolType;
                        DetailsProgressValue = initializedDataObject.ServerStatus.DetailsProgressValue;
                        MainProgressValue = initializedDataObject.ServerStatus.MainProgressValue;
                        SeviceWorking = initializedDataObject.ServerStatus.SeviceWorking;
                        AllSymbolsSync = initializedDataObject.ServerStatus.AllSymbolsSync;
                        BinanceConnectionStatus = initializedDataObject.ServerStatus.BinanceConnectionStatus;
                    }), DispatcherPriority.Send);
                }

                if (initializedDataObject.Alarms != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Alarms.Clear();

                        foreach (var alarm in initializedDataObject.Alarms)
                        {
                            Alarms.Add(alarm);
                        }
                    }), DispatcherPriority.Send);
                }

                if (initializedDataObject.AlarmsHistory != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AlarmsHistory.Clear();

                        foreach (var alarm in initializedDataObject.AlarmsHistory)
                        {
                            AlarmsHistory.Add(alarm);
                        }
                    }), DispatcherPriority.Send);
                }

                if (initializedDataObject.MarketData != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MarketData.Clear();

                        foreach (var symbolDataModel in initializedDataObject.MarketData)
                        {
                            MarketData.Add(symbolDataModel);

                            symbolDataModel.SupportsResistances = new ObservableCollection<SymbolSupportsResistancesDataModel>();

                            SupportResistanceHelper.FillSupportsResistances(symbolDataModel);

                            GridControlSupportResistance.SortBy(GridControlSupportResistance.Columns["Price"], ColumnSortOrder.Descending);

                            GridControlSupportResistance.RefreshData();
                        }
                    }), DispatcherPriority.Send);
                }
            }
        }

        private async Task<bool> HandleRunAlarms(string script, string filename)
        {
            var result = false;

            if (CryptoClient != null)
            {
                var response = new CommandDataObject();

                result = await CryptoClient.Send(CommandHelper.RunAlarms(script, filename), true, response);

                if (result)
                {
                    if (response != null && response.Command == CommandTypes.RunAlarmsResponse && response.Parameter != null && response.Parameter is bool)
                    {
                        result = (bool)response.Parameter;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        private async Task<bool> HandleRunTemplateAlarm(string script, SymbolTypes[] symbols, string filename)
        {
            var result = false;

            if (CryptoClient != null)
            {
                var response = new CommandDataObject();

                result = await CryptoClient.Send(CommandHelper.RunTemplateAlarm(script, symbols, filename), true, response);

                if (result)
                {
                    if (response != null && response.Command == CommandTypes.RunTemplateAlarmResponse && response.Parameter != null && response.Parameter is bool)
                    {
                        result = (bool)response.Parameter;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        private async Task<string> HandleReadAlarmScript(Guid id)
        {
            var result = "";

            if (CryptoClient != null)
            {
                var response = new CommandDataObject();

                var sendResult = await CryptoClient.Send(new CommandDataObject() { Command = CommandTypes.ReadAlarmScript, Parameter = id }, true, response);

                if (sendResult)
                {
                    if (response != null && response.Command == CommandTypes.ReadAlarmScriptResponse && response.Parameter != null && response.Parameter is string)
                    {
                        result = (string)response.Parameter;
                    }
                }
            }

            return result;
        }

        private async Task<AlarmItemDataModel> HandleEvaluateAlarm(Guid id, DateTime datetime)
        {
            AlarmItemDataModel result = null;

            if (CryptoClient != null)
            {
                var response = new CommandDataObject();

                var sendResult = await CryptoClient.Send(CommandHelper.EvaluateAlarm(id, datetime), true, response);

                if (sendResult)
                {
                    if (response != null && response.Command == CommandTypes.EvaluateAlarmResponse && response.Parameter != null && response.Parameter is AlarmItemDataModel)
                    {
                        result = (AlarmItemDataModel)response.Parameter;
                    }
                }
            }

            return result;
        }

        private async void HandleSeenAlarm(Guid id)
        {
            if (CryptoClient != null)
            {
                await CryptoClient.Send(new CommandDataObject() { Command = CommandTypes.SeenAlarm, Parameter = id }, false, null);
            }
        }

        private void HandleSeenAlarmResponse(CommandDataObject response)
        {
            if (response != null && response.Command == CommandTypes.SeenAlarmResponse && response.Parameter != null && response.Parameter is Guid)
            {
                var id = (Guid)response.Parameter;

                foreach (var symbolDataModel in MarketData)
                {
                    foreach (var alarm in symbolDataModel.SymbolAlarms)
                    {
                        if (alarm.Id == id)
                        {
                            alarm.SeenAlarm();

                            break;
                        }
                    }
                }

                foreach (var alarm in Alarms)
                {
                    if (alarm.Id == id)
                    {
                        alarm.SeenAlarm();

                        break;
                    }
                }
            }
        }

        private async void HandleSeenAllAlarm()
        {
            if (CryptoClient != null)
            {
                await CryptoClient.Send(new CommandDataObject() { Command = CommandTypes.SeenAllAlarm, Parameter = null }, false, null);
            }
        }

        private async void HandleEnableDisableAlarm(Guid id)
        {
            if (CryptoClient != null)
            {
                var response = new CommandDataObject();

                var sendResult = await CryptoClient.Send(new CommandDataObject() { Command = CommandTypes.EnableDisableAlarm, Parameter = id }, true, response);

                if (sendResult)
                {
                    HandleEnableDisableAlarmResponse(response);
                }
            }
        }

        private void HandleEnableDisableAlarmResponse(CommandDataObject response)
        {
            if (response != null && response.Command == CommandTypes.EnableDisableAlarmResponse && response.Parameter != null && response.Parameter is Guid)
            {
                var id = (Guid)response.Parameter;

                foreach (var symbolDataModel in MarketData)
                {
                    foreach (var alarm in symbolDataModel.SymbolAlarms)
                    {
                        if (alarm.Id == id)
                        {
                            alarm.SetEnabled(!alarm.Enabled);

                            break;
                        }
                    }
                }

                foreach (var alarm in Alarms)
                {
                    if (alarm.Id == id)
                    {
                        alarm.SetEnabled(!alarm.Enabled);

                        break;
                    }
                }
            }
        }

        private async void HandleDeleteAlarm(Guid id)
        {
            if (CryptoClient != null)
            {
                var response = new CommandDataObject();

                var sendResult = await CryptoClient.Send(new CommandDataObject() { Command = CommandTypes.DeleteAlarm, Parameter = id }, true, response);

                if (sendResult)
                {
                    HandleDeleteAlarmResponse(response);
                }
            }
        }

        private void HandleDeleteAlarmResponse(CommandDataObject response)
        {
            if (response != null && response.Command == CommandTypes.DeleteAlarmResponse && response.Parameter != null && response.Parameter is Guid)
            {
                var id = (Guid)response.Parameter;

                foreach (var symbolDataModel in MarketData)
                {
                    foreach (var alarm in symbolDataModel.SymbolAlarms)
                    {
                        if (alarm.Id == id)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                symbolDataModel.SymbolAlarms.Remove(alarm);
                            }), DispatcherPriority.Send);

                            break;
                        }
                    }
                }

                foreach (var alarm in Alarms)
                {
                    if (alarm.Id == id)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Alarms.Remove(alarm);
                        }), DispatcherPriority.Send);

                        break;
                    }
                }
            }
        }

        private async Task<List<SymbolDataModel>> HandleLiveHistory(DateTime? datetime, SymbolTypes[] symbols)
        {
            List<SymbolDataModel> result = null;

            if (CryptoClient != null)
            {
                var response = new CommandDataObject();

                var request = new CommandDataObject() { Command = CommandTypes.LiveHistory };

                var parameters = new object[2];

                parameters[0] = symbols;

                if (datetime.HasValue)
                {
                    parameters[1] = datetime.Value;
                }

                request.Parameter = parameters;

                var sendResult = await CryptoClient.Send(request, true, response);

                if (sendResult)
                {
                    if (response != null && response.Command == CommandTypes.LiveHistoryResponse && response.Parameter != null && response.Parameter is List<SymbolDataModel>)
                    {
                        result = (List<SymbolDataModel>)response.Parameter;
                    }
                }
            }

            return result;
        }

        private void HandleServerStatusPropertyChanged(string propertyName, object value)
        {
            switch (propertyName)
            {
                case nameof(ServerStatusDataModel.GridControlMainVisibility):
                    if (value is Visibility)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            GridControlMain.Visibility = (Visibility)value;
                        }), DispatcherPriority.Send);
                    }
                    break;
                case nameof(ServerStatusDataModel.GridLoadingDataVisibility):
                    if (value is Visibility)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            GridLoadingData.Visibility = (Visibility)value;
                        }), DispatcherPriority.Send);
                    }
                    break;
                case nameof(ServerStatusDataModel.CurrentDetailSymbolType):
                    if (value is SymbolTypes)
                    {
                        DetailsProgressMessage = (SymbolTypes)value;
                    }
                    break;
                case nameof(ServerStatusDataModel.DetailsProgressValue):
                    if (value is float)
                    {
                        DetailsProgressValue = (float)value;
                    }
                    break;
                case nameof(ServerStatusDataModel.MainProgressValue):
                    if (value is float)
                    {
                        MainProgressValue = (float)value;
                    }
                    break;
                case nameof(ServerStatusDataModel.SeviceWorking):
                    if (value is bool)
                    {
                        SeviceWorking = (bool)value;
                    }
                    break;
                case nameof(ServerStatusDataModel.AllSymbolsSync):
                    if (value is bool)
                    {
                        AllSymbolsSync = (bool)value;
                    }
                    break;
                case nameof(ServerStatusDataModel.BinanceConnectionStatus):
                    if (value is BinanceConnectionStatusModes)
                    {
                        BinanceConnectionStatus = (BinanceConnectionStatusModes)value;
                    }
                    break;
            }
        }

        private void HandleNewAlarmsReceived(List<SymbolAlarmDataModel> alarms)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Alarms.Clear();

                foreach (var alarm in alarms)
                {
                    Alarms.Add(alarm);
                }

                AlarmWindow.ActiveAlarms();
            }), DispatcherPriority.Send);
        }

        private void HandleNewAlarmsHistoryReceived(List<SymbolAlarmDataModel> alarms)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var alarm in alarms)
                {
                    AlarmsHistory.Insert(0, alarm);
                }
            }), DispatcherPriority.Send);
        }

        private void HandleMenuItemsStatusChanged(CommandTypes command, bool status)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (command)
                {
                    case CommandTypes.StartTechnicalAnalysis:
                        MenuItemStart.IsEnabled = status;
                        break;
                    case CommandTypes.LiveHistory:
                        MenuItemLiveHistory.IsEnabled = status;
                        break;
                    case CommandTypes.Alarms:
                        MenuItemShowAlarmWindow.IsEnabled = status;
                        break;
                    case CommandTypes.AlarmsHistory:
                        MenuItemShowAlarmHistoryWindow.IsEnabled = status;
                        break;
                    case CommandTypes.NewAlarm:
                        MenuItemNewAlarmScript.IsEnabled = status;
                        break;
                    case CommandTypes.EditAlarm:
                        MenuItemEditAlarmScript.IsEnabled = status;
                        break;
                    case CommandTypes.RunAlarms:
                        MenuItemRunAlarmScripts.IsEnabled = status;
                        break;
                    case CommandTypes.RunTemplateAlarm:
                        MenuItemRunTemplateAlarmScript.IsEnabled = status;
                        break;
                    case CommandTypes.SeenAlarm:
                        MenuItemSeenAlarm.IsEnabled = status;
                        AlarmWindow.SetMenuItemStatus(CommandTypes.SeenAlarm, status);
                        break;
                    case CommandTypes.SeenAllAlarm:
                        MenuItemSeenAllAlarm.IsEnabled = status;
                        AlarmWindow.SetMenuItemStatus(CommandTypes.SeenAllAlarm, status);
                        break;
                    case CommandTypes.EnableDisableAlarm:
                        MenuItemEnableDisableAlarm.IsEnabled = status;
                        AlarmWindow.SetMenuItemStatus(CommandTypes.EnableDisableAlarm, status);
                        break;
                    case CommandTypes.ReadAlarmScript:
                        MenuItemViewScript.IsEnabled = status;
                        AlarmWindow.SetMenuItemStatus(CommandTypes.ReadAlarmScript, status);
                        ViewScriptStatus = status;
                        break;
                    case CommandTypes.EvaluateAlarm:
                        MenuItemEvaluateAlarmViewerWindow.IsEnabled = status;
                        AlarmWindow.SetMenuItemStatus(CommandTypes.EvaluateAlarm, status);
                        EvaluateAlarmStatus = status;
                        break;
                    case CommandTypes.DeleteAlarm:
                        MenuItemDeleteAlarm.IsEnabled = status;
                        AlarmWindow.SetMenuItemStatus(CommandTypes.DeleteAlarm, status);
                        break;
                    case CommandTypes.TestNewStrategy:
                        MenuItemBackTestNewStrategy.IsEnabled = status;
                        break;
                }
            }), DispatcherPriority.Send);
        }

        private async Task<bool> HandleStrategyTest(StrategyTestDataModel strategyTest)
        {
            var result = false;

            if (CryptoClient != null)
            {
                if (!StrategyTests.Contains(strategyTest))
                {
                    StrategyTests.Add(strategyTest);
                }

                //
                var response = new CommandDataObject();

                result = await CryptoClient.Send(CommandHelper.TestNewStrategy(strategyTest), true, response);

                if (result)
                {
                    if (response != null && response.Command == CommandTypes.TestNewStrategyResponse && response.Parameter != null && response.Parameter is bool)
                    {
                        result = (bool)response.Parameter;
                    }
                }

                //
                if (!result)
                {
                    StrategyTests.Remove(strategyTest);
                }
            }

            return result;
        }

        private async void HandleStrategyTestStop(Guid id)
        {
            if (CryptoClient != null)
            {
                await CryptoClient.Send(new CommandDataObject() { Command = CommandTypes.TestStrategyStop, Parameter = id }, false, null);
            }
        }

        private void HandleStrategyTestStatus(CommandDataObject response)
        {
            if (response != null && response.Command == CommandTypes.TestStrategyStatus && response.Parameter != null && response.Parameter is StrategyTestStatusDataModel)
            {
                var strategyTestStatus = (StrategyTestStatusDataModel)response.Parameter;

                var strategyTest = StrategyTests.FirstOrDefault(p => p.Id == strategyTestStatus.Id);

                if (strategyTest != null)
                {
                    strategyTest.OnStrategyTestStatusUpdated(strategyTestStatus);
                }
                else
                {
                    if (!StopStrategyTestNotificationToServer.Contains(strategyTestStatus.Id))
                    {
                        StopStrategyTestNotificationToServer.Add(strategyTestStatus.Id);

                        HandleStrategyTestStop(strategyTestStatus.Id);
                    }
                }
            }
        }

        public async Task<bool> Connect(SessionEstablishmentDataModel sessionEstablishment)
        {
            SessionEstablishment = sessionEstablishment;

            await Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBlockServerConnectionStatus.Foreground = Brushes.White;
                TextBlockServerConnectionStatus.Background = Brushes.Red;
                TextBlockServerConnectionStatus.Text = "Connecting...";
            }), DispatcherPriority.Send);

            await Task.Run(() => Thread.Sleep(1000));

            var client = new CryptoClientService(SessionEstablishment);

            client.Connected += CryptoClient_Connected;
            client.CommandDataReceived += CryptoClient_CommandDataReceived;

            var result = await client.Start();

            if (result)
            {
                client.Closed += CryptoClient_Closed;

                CryptoClient = client;

                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    var ipAddressBytes = IPAddress.Parse(SessionEstablishment.Address).GetAddressBytes();

                    Title = string.Format("{0} - {1} Version", "Chanel Trading Client", SessionEstablishment.ClientType);

                    TextBlockServerConnectionStatus.Foreground = Brushes.Green;
                    TextBlockServerConnectionStatus.Background = Brushes.Transparent;
                    TextBlockServerConnectionStatus.Text = "Connected";
                }), DispatcherPriority.Send);

                if (EnabledSound)
                {
                    var player = new SoundPlayer(SharedAddressHelper.ChanelConnectionOnWavFile);
                    player.Play();
                }
            }
            else
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBlockServerConnectionStatus.Foreground = Brushes.White;
                    TextBlockServerConnectionStatus.Background = Brushes.Red;
                    TextBlockServerConnectionStatus.Text = "Disconnected";
                }), DispatcherPriority.Send);

                await Task.Run(() => Thread.Sleep(1000));
            }

            return result;
        }
    }
}

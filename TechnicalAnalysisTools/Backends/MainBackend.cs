using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Services;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.DataObjects;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Backends
{
    public class MainBackend
    {
        public MainBackend(bool databaseSupport, ReaderWriterLock initializeNewSessionLock)
        {
            InitializeBackend(databaseSupport, initializeNewSessionLock);

            InitializeMenuItemsStatus();

            InitializeCommandPermissions();
        }

        private bool DatabaseSupport { get; set; }

        private MainFormVisualizerService MainFormVisualizer { get; } = new MainFormVisualizerService();

        private CancellationTokenSource VisualizerCancellationToken { get; set; }

        private Dictionary<string, LiveHistoryService> LiveHistories { get; set; }

        private SemaphoreSlim LiveHistoriesSemaphore { get; } = new SemaphoreSlim(1, 1);

        private Dictionary<CommandTypes, bool> AdminMenuItemsStatus { get; } = new Dictionary<CommandTypes, bool>();

        private Dictionary<CommandTypes, bool> GoldMenuItemsStatus { get; } = new Dictionary<CommandTypes, bool>();

        private Dictionary<CommandTypes, bool> StandardMenuItemsStatus { get; } = new Dictionary<CommandTypes, bool>();

        private Dictionary<CommandTypes, bool> ViewMenuItemsStatus { get; } = new Dictionary<CommandTypes, bool>();

        private Dictionary<CommandTypes, bool> AdminCommandPermissions { get; } = new Dictionary<CommandTypes, bool>();

        private Dictionary<CommandTypes, bool> GoldCommandPermissions { get; } = new Dictionary<CommandTypes, bool>();

        private Dictionary<CommandTypes, bool> StandardCommandPermissions { get; } = new Dictionary<CommandTypes, bool>();

        private Dictionary<CommandTypes, bool> ViewCommandPermissions { get; } = new Dictionary<CommandTypes, bool>();

        private ServerStatusDataModel ServerStatus { get; } = new ServerStatusDataModel();

        private List<SymbolAlarmDataModel> Alarms { get; set; } = new List<SymbolAlarmDataModel>();

        private List<SymbolAlarmDataModel> AlarmsHistory { get; set; } = new List<SymbolAlarmDataModel>();

        private ReaderWriterLock InitializeNewSessionLock { get; set; }

        public List<SymbolTypes> SupportedSymbols
        {
            get
            {
                return (List<SymbolTypes>)MainFormVisualizer.SupportedSymbols;
            }
        }

        private ObservableCollection<SymbolDataModel> MarketData
        {
            get
            {
                return MainFormVisualizer.MarketData;
            }
        }

        private ConcurrentDictionary<Guid, StrategyTestService> StrategyTestServices { get; set; } = new ConcurrentDictionary<Guid, StrategyTestService>();

        private void InitializeBackend(bool databaseSupport, ReaderWriterLock initializeNewSessionLock)
        {
            DatabaseSupport = databaseSupport;

            InitializeNewSessionLock = initializeNewSessionLock;

            MainFormVisualizer.Start();

            if (File.Exists(ServerAddressHelper.AlarmHistoryFile))
            {
                var lines = File.ReadAllLines(ServerAddressHelper.AlarmHistoryFile);

                AlarmsHistory.AddRange(lines.Skip(lines.Length - 1000).Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => SymbolAlarmDataModel.ParseSymbolAlarm(p)));
            }
        }

        private void InitializeMenuItemsStatus()
        {
            AdminMenuItemsStatus.Add(CommandTypes.StartTechnicalAnalysis, true);
            AdminMenuItemsStatus.Add(CommandTypes.LiveHistory, false);
            AdminMenuItemsStatus.Add(CommandTypes.Alarms, false);
            AdminMenuItemsStatus.Add(CommandTypes.AlarmsHistory, true);
            AdminMenuItemsStatus.Add(CommandTypes.NewAlarm, true);
            AdminMenuItemsStatus.Add(CommandTypes.EditAlarm, true);
            AdminMenuItemsStatus.Add(CommandTypes.RunAlarms, false);
            AdminMenuItemsStatus.Add(CommandTypes.RunTemplateAlarm, false);
            AdminMenuItemsStatus.Add(CommandTypes.SeenAlarm, false);
            AdminMenuItemsStatus.Add(CommandTypes.SeenAllAlarm, false);
            AdminMenuItemsStatus.Add(CommandTypes.EnableDisableAlarm, false);
            AdminMenuItemsStatus.Add(CommandTypes.ReadAlarmScript, false);
            AdminMenuItemsStatus.Add(CommandTypes.EvaluateAlarm, false);
            AdminMenuItemsStatus.Add(CommandTypes.DeleteAlarm, false);
            AdminMenuItemsStatus.Add(CommandTypes.TestNewStrategy, false);

            GoldMenuItemsStatus.Add(CommandTypes.StartTechnicalAnalysis, false);
            GoldMenuItemsStatus.Add(CommandTypes.LiveHistory, false);
            GoldMenuItemsStatus.Add(CommandTypes.Alarms, false);
            GoldMenuItemsStatus.Add(CommandTypes.AlarmsHistory, true);
            GoldMenuItemsStatus.Add(CommandTypes.NewAlarm, true);
            GoldMenuItemsStatus.Add(CommandTypes.EditAlarm, true);
            GoldMenuItemsStatus.Add(CommandTypes.RunAlarms, false);
            GoldMenuItemsStatus.Add(CommandTypes.RunTemplateAlarm, false);
            GoldMenuItemsStatus.Add(CommandTypes.SeenAlarm, false);
            GoldMenuItemsStatus.Add(CommandTypes.SeenAllAlarm, false);
            GoldMenuItemsStatus.Add(CommandTypes.EnableDisableAlarm, false);
            GoldMenuItemsStatus.Add(CommandTypes.ReadAlarmScript, false);
            GoldMenuItemsStatus.Add(CommandTypes.EvaluateAlarm, false);
            GoldMenuItemsStatus.Add(CommandTypes.DeleteAlarm, false);
            GoldMenuItemsStatus.Add(CommandTypes.TestNewStrategy, false);

            StandardMenuItemsStatus.Add(CommandTypes.StartTechnicalAnalysis, false);
            StandardMenuItemsStatus.Add(CommandTypes.LiveHistory, false);
            StandardMenuItemsStatus.Add(CommandTypes.Alarms, false);
            StandardMenuItemsStatus.Add(CommandTypes.AlarmsHistory, true);
            StandardMenuItemsStatus.Add(CommandTypes.NewAlarm, true);
            StandardMenuItemsStatus.Add(CommandTypes.EditAlarm, true);
            StandardMenuItemsStatus.Add(CommandTypes.RunAlarms, false);
            StandardMenuItemsStatus.Add(CommandTypes.RunTemplateAlarm, false);
            StandardMenuItemsStatus.Add(CommandTypes.SeenAlarm, false);
            StandardMenuItemsStatus.Add(CommandTypes.SeenAllAlarm, false);
            StandardMenuItemsStatus.Add(CommandTypes.EnableDisableAlarm, false);
            StandardMenuItemsStatus.Add(CommandTypes.ReadAlarmScript, false);
            StandardMenuItemsStatus.Add(CommandTypes.EvaluateAlarm, false);
            StandardMenuItemsStatus.Add(CommandTypes.DeleteAlarm, false);
            StandardMenuItemsStatus.Add(CommandTypes.TestNewStrategy, false);

            ViewMenuItemsStatus.Add(CommandTypes.StartTechnicalAnalysis, false);
            ViewMenuItemsStatus.Add(CommandTypes.LiveHistory, false);
            ViewMenuItemsStatus.Add(CommandTypes.Alarms, false);
            ViewMenuItemsStatus.Add(CommandTypes.AlarmsHistory, false);
            ViewMenuItemsStatus.Add(CommandTypes.NewAlarm, true);
            ViewMenuItemsStatus.Add(CommandTypes.EditAlarm, true);
            ViewMenuItemsStatus.Add(CommandTypes.RunAlarms, false);
            ViewMenuItemsStatus.Add(CommandTypes.RunTemplateAlarm, false);
            ViewMenuItemsStatus.Add(CommandTypes.SeenAlarm, false);
            ViewMenuItemsStatus.Add(CommandTypes.SeenAllAlarm, false);
            ViewMenuItemsStatus.Add(CommandTypes.EnableDisableAlarm, false);
            ViewMenuItemsStatus.Add(CommandTypes.ReadAlarmScript, false);
            ViewMenuItemsStatus.Add(CommandTypes.EvaluateAlarm, false);
            ViewMenuItemsStatus.Add(CommandTypes.DeleteAlarm, false);
            ViewMenuItemsStatus.Add(CommandTypes.TestNewStrategy, false);
        }

        private void InitializeCommandPermissions()
        {
            AdminCommandPermissions.Add(CommandTypes.StartTechnicalAnalysis, true);
            AdminCommandPermissions.Add(CommandTypes.LiveHistory, true);
            AdminCommandPermissions.Add(CommandTypes.RunAlarms, true);
            AdminCommandPermissions.Add(CommandTypes.RunTemplateAlarm, true);
            AdminCommandPermissions.Add(CommandTypes.ReadAlarmScript, true);
            AdminCommandPermissions.Add(CommandTypes.EvaluateAlarm, true);
            AdminCommandPermissions.Add(CommandTypes.SeenAlarm, true);
            AdminCommandPermissions.Add(CommandTypes.SeenAllAlarm, true);
            AdminCommandPermissions.Add(CommandTypes.EnableDisableAlarm, true);
            AdminCommandPermissions.Add(CommandTypes.DeleteAlarm, true);
            AdminCommandPermissions.Add(CommandTypes.TestNewStrategy, true);
            AdminCommandPermissions.Add(CommandTypes.TestStrategyStop, true);

            GoldCommandPermissions.Add(CommandTypes.StartTechnicalAnalysis, false);
            GoldCommandPermissions.Add(CommandTypes.LiveHistory, true);
            GoldCommandPermissions.Add(CommandTypes.RunAlarms, true);
            GoldCommandPermissions.Add(CommandTypes.RunTemplateAlarm, true);
            GoldCommandPermissions.Add(CommandTypes.ReadAlarmScript, true);
            GoldCommandPermissions.Add(CommandTypes.EvaluateAlarm, true);
            GoldCommandPermissions.Add(CommandTypes.SeenAlarm, true);
            GoldCommandPermissions.Add(CommandTypes.SeenAllAlarm, true);
            GoldCommandPermissions.Add(CommandTypes.EnableDisableAlarm, true);
            GoldCommandPermissions.Add(CommandTypes.DeleteAlarm, true);
            GoldCommandPermissions.Add(CommandTypes.TestNewStrategy, true);
            GoldCommandPermissions.Add(CommandTypes.TestStrategyStop, true);

            StandardCommandPermissions.Add(CommandTypes.StartTechnicalAnalysis, false);
            StandardCommandPermissions.Add(CommandTypes.LiveHistory, true);
            StandardCommandPermissions.Add(CommandTypes.RunAlarms, true);
            StandardCommandPermissions.Add(CommandTypes.RunTemplateAlarm, true);
            StandardCommandPermissions.Add(CommandTypes.ReadAlarmScript, true);
            StandardCommandPermissions.Add(CommandTypes.EvaluateAlarm, true);
            StandardCommandPermissions.Add(CommandTypes.SeenAlarm, true);
            StandardCommandPermissions.Add(CommandTypes.SeenAllAlarm, true);
            StandardCommandPermissions.Add(CommandTypes.EnableDisableAlarm, true);
            StandardCommandPermissions.Add(CommandTypes.DeleteAlarm, true);
            StandardCommandPermissions.Add(CommandTypes.TestNewStrategy, true);
            StandardCommandPermissions.Add(CommandTypes.TestStrategyStop, false);

            ViewCommandPermissions.Add(CommandTypes.StartTechnicalAnalysis, false);
            ViewCommandPermissions.Add(CommandTypes.LiveHistory, true);
            ViewCommandPermissions.Add(CommandTypes.RunAlarms, false);
            ViewCommandPermissions.Add(CommandTypes.RunTemplateAlarm, false);
            ViewCommandPermissions.Add(CommandTypes.ReadAlarmScript, false);
            ViewCommandPermissions.Add(CommandTypes.EvaluateAlarm, false);
            ViewCommandPermissions.Add(CommandTypes.SeenAlarm, false);
            ViewCommandPermissions.Add(CommandTypes.SeenAllAlarm, false);
            ViewCommandPermissions.Add(CommandTypes.EnableDisableAlarm, false);
            ViewCommandPermissions.Add(CommandTypes.DeleteAlarm, false);
            ViewCommandPermissions.Add(CommandTypes.TestNewStrategy, false);
            ViewCommandPermissions.Add(CommandTypes.TestStrategyStop, false);
        }

        private LiveHistoryService GetLiveHistoryService(string username)
        {
            LiveHistoryService result;

            if (!LiveHistories.ContainsKey(username))
            {
                try
                {
                    LiveHistoriesSemaphore.Wait();

                    if (!LiveHistories.ContainsKey(username))
                    {
                        LiveHistories.Add(username, new LiveHistoryService(SupportedSymbols, MainFormVisualizer.Alarms));
                    }
                }
                catch
                {

                }
                finally
                {
                    LiveHistoriesSemaphore.Release();
                }
            }

            result = LiveHistories[username];

            return result;
        }

        private bool StrategyTestService_StrategyTestStatusChanged(Guid sessionId, StrategyTestStatusDataModel strategyTestStatus)
        {
            var result = false;

            if (StrategyTestStatusChanged != null)
            {
                result = StrategyTestStatusChanged.Invoke(sessionId, strategyTestStatus);

                if (!result || strategyTestStatus.StrategyTestStatusType == StrategyTestStatusTypes.Error || strategyTestStatus.StrategyTestStatusType == StrategyTestStatusTypes.Finish)
                {
                    StrategyTestService strategyTest = null;

                    while (!StrategyTestServices.TryRemove(strategyTestStatus.Id, out strategyTest))
                    {
                        Thread.Sleep(0);
                    }

                    if (strategyTest != null)
                    {
                        strategyTest.StrategyTestStatusChanged -= StrategyTestService_StrategyTestStatusChanged;
                    }
                }
            }

            return result;
        }

        public bool HasPermission(UiClientTypes clientType, CommandTypes commandType)
        {
            bool result = false;

            if (clientType == UiClientTypes.Admin)
            {
                if (AdminCommandPermissions.ContainsKey(commandType))
                {
                    result = AdminCommandPermissions[commandType];
                }
            }
            else if (clientType == UiClientTypes.Gold)
            {
                if (GoldCommandPermissions.ContainsKey(commandType))
                {
                    result = GoldCommandPermissions[commandType];
                }
            }
            else if (clientType == UiClientTypes.Standard)
            {
                if (StandardCommandPermissions.ContainsKey(commandType))
                {
                    result = StandardCommandPermissions[commandType];
                }
            }
            else
            {
                if (ViewCommandPermissions.ContainsKey(commandType))
                {
                    result = ViewCommandPermissions[commandType];
                }
            }

            if (result)
            {
                if (clientType == UiClientTypes.Admin)
                {
                    if (AdminMenuItemsStatus.ContainsKey(commandType))
                    {
                        result = AdminMenuItemsStatus[commandType];
                    }
                }
                else if (clientType == UiClientTypes.Gold)
                {
                    if (GoldMenuItemsStatus.ContainsKey(commandType))
                    {
                        result = GoldMenuItemsStatus[commandType];
                    }
                }
                else if (clientType == UiClientTypes.Standard)
                {
                    if (StandardMenuItemsStatus.ContainsKey(commandType))
                    {
                        result = StandardMenuItemsStatus[commandType];
                    }
                }
                else
                {
                    if (ViewMenuItemsStatus.ContainsKey(commandType))
                    {
                        result = ViewMenuItemsStatus[commandType];
                    }
                }
            }

            return result;
        }

        public List<SymbolDataModel> GetMarketDataByUsername(string username, UiClientTypes clientType)
        {
            var result = MarketData.ToList();

            if (clientType != UiClientTypes.Admin)
            {
                var marketData = result;

                result = new List<SymbolDataModel>();

                foreach (var data in marketData)
                {
                    var cloneData = (SymbolDataModel)data.Clone();

                    var alarms = cloneData.SymbolAlarms.Where(p => p.Username == username).ToList();

                    cloneData.SymbolAlarms.Clear();

                    foreach (var alarm in alarms)
                    {
                        cloneData.SymbolAlarms.Add(alarm);
                    }

                    result.Add(cloneData);
                }
            }

            return result;
        }

        public UiClientInitializedDataObject GetClientInitializedDataByUsername(string username, UiClientTypes clientType)
        {
            var result = new UiClientInitializedDataObject();

            if (clientType == UiClientTypes.Admin)
            {
                result.MenuItemsStatus = AdminMenuItemsStatus;
            }
            else if (clientType == UiClientTypes.Gold)
            {
                result.MenuItemsStatus = GoldMenuItemsStatus;
            }
            else if (clientType == UiClientTypes.Standard)
            {
                result.MenuItemsStatus = StandardMenuItemsStatus;
            }
            else
            {
                result.MenuItemsStatus = ViewMenuItemsStatus;
            }

            result.ServerStatus = ServerStatus;
            result.Alarms = Alarms;
            result.AlarmsHistory = AlarmsHistory;

            if (clientType != UiClientTypes.Admin)
            {
                result.Alarms = result.Alarms.Where(p => p.Username == username).ToList();
                result.AlarmsHistory = result.AlarmsHistory.Where(p => p.Username == username).ToList();
            }

            result.SupportedSymbols = SupportedSymbols;
            result.MarketData = GetMarketDataByUsername(username, clientType);

            return result;
        }

        public void Start()
        {
            //
            AdminMenuItemsStatus[CommandTypes.StartTechnicalAnalysis] = false;
            GoldMenuItemsStatus[CommandTypes.StartTechnicalAnalysis] = false;
            StandardMenuItemsStatus[CommandTypes.StartTechnicalAnalysis] = false;
            ViewMenuItemsStatus[CommandTypes.StartTechnicalAnalysis] = false;

            MenuItemsStatusChanged?.Invoke(CommandTypes.StartTechnicalAnalysis);

            //
            MainFormVisualizer.PreProcessServiceWorkingNotified += (seviceWorking) =>
            {
                if (seviceWorking)
                {
                    ServerStatus.GridControlMainVisibility = Visibility.Collapsed;
                    ServerStatus.GridLoadingDataVisibility = Visibility.Visible;
                }
                else
                {
                    ServerStatus.GridControlMainVisibility = Visibility.Visible;
                    ServerStatus.GridLoadingDataVisibility = Visibility.Collapsed;

                    LiveHistories = new Dictionary<string, LiveHistoryService>();

                    AdminMenuItemsStatus[CommandTypes.Alarms] = true;
                    AdminMenuItemsStatus[CommandTypes.AlarmsHistory] = true;
                    AdminMenuItemsStatus[CommandTypes.RunAlarms] = true;
                    AdminMenuItemsStatus[CommandTypes.RunTemplateAlarm] = true;
                    AdminMenuItemsStatus[CommandTypes.LiveHistory] = true;
                    AdminMenuItemsStatus[CommandTypes.SeenAlarm] = true;
                    AdminMenuItemsStatus[CommandTypes.SeenAllAlarm] = true;
                    AdminMenuItemsStatus[CommandTypes.EnableDisableAlarm] = true;
                    AdminMenuItemsStatus[CommandTypes.ReadAlarmScript] = true;
                    AdminMenuItemsStatus[CommandTypes.EvaluateAlarm] = true;
                    AdminMenuItemsStatus[CommandTypes.DeleteAlarm] = true;
                    AdminMenuItemsStatus[CommandTypes.TestNewStrategy] = true;

                    GoldMenuItemsStatus[CommandTypes.Alarms] = true;
                    GoldMenuItemsStatus[CommandTypes.AlarmsHistory] = true;
                    GoldMenuItemsStatus[CommandTypes.RunAlarms] = true;
                    GoldMenuItemsStatus[CommandTypes.RunTemplateAlarm] = true;
                    GoldMenuItemsStatus[CommandTypes.LiveHistory] = true;
                    GoldMenuItemsStatus[CommandTypes.SeenAlarm] = true;
                    GoldMenuItemsStatus[CommandTypes.SeenAllAlarm] = true;
                    GoldMenuItemsStatus[CommandTypes.EnableDisableAlarm] = true;
                    GoldMenuItemsStatus[CommandTypes.ReadAlarmScript] = true;
                    GoldMenuItemsStatus[CommandTypes.EvaluateAlarm] = true;
                    GoldMenuItemsStatus[CommandTypes.DeleteAlarm] = true;
                    GoldMenuItemsStatus[CommandTypes.TestNewStrategy] = true;

                    StandardMenuItemsStatus[CommandTypes.Alarms] = true;
                    StandardMenuItemsStatus[CommandTypes.AlarmsHistory] = true;
                    StandardMenuItemsStatus[CommandTypes.RunAlarms] = true;
                    StandardMenuItemsStatus[CommandTypes.RunTemplateAlarm] = true;
                    StandardMenuItemsStatus[CommandTypes.LiveHistory] = true;
                    StandardMenuItemsStatus[CommandTypes.SeenAlarm] = true;
                    StandardMenuItemsStatus[CommandTypes.SeenAllAlarm] = true;
                    StandardMenuItemsStatus[CommandTypes.EnableDisableAlarm] = true;
                    StandardMenuItemsStatus[CommandTypes.ReadAlarmScript] = true;
                    StandardMenuItemsStatus[CommandTypes.EvaluateAlarm] = true;
                    StandardMenuItemsStatus[CommandTypes.DeleteAlarm] = true;
                    StandardMenuItemsStatus[CommandTypes.TestNewStrategy] = false;

                    ViewMenuItemsStatus[CommandTypes.Alarms] = true;
                    ViewMenuItemsStatus[CommandTypes.AlarmsHistory] = false;
                    ViewMenuItemsStatus[CommandTypes.RunAlarms] = false;
                    ViewMenuItemsStatus[CommandTypes.RunTemplateAlarm] = false;
                    ViewMenuItemsStatus[CommandTypes.LiveHistory] = true;
                    ViewMenuItemsStatus[CommandTypes.SeenAlarm] = false;
                    ViewMenuItemsStatus[CommandTypes.SeenAllAlarm] = false;
                    ViewMenuItemsStatus[CommandTypes.EnableDisableAlarm] = false;
                    ViewMenuItemsStatus[CommandTypes.ReadAlarmScript] = false;
                    ViewMenuItemsStatus[CommandTypes.EvaluateAlarm] = false;
                    ViewMenuItemsStatus[CommandTypes.DeleteAlarm] = false;
                    ViewMenuItemsStatus[CommandTypes.TestNewStrategy] = false;

                    MenuItemsStatusChanged?.Invoke(CommandTypes.Alarms);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.AlarmsHistory);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.RunAlarms);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.RunTemplateAlarm);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.LiveHistory);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.SeenAlarm);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.SeenAllAlarm);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.EnableDisableAlarm);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.ReadAlarmScript);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.EvaluateAlarm);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.DeleteAlarm);
                    MenuItemsStatusChanged?.Invoke(CommandTypes.TestNewStrategy);
                }
            };

            MainFormVisualizer.DetailsProgressValueChanged += (value, detail) =>
            {
                if (detail != null)
                {
                    ServerStatus.CurrentDetailSymbolType = (SymbolTypes)detail;
                }

                ServerStatus.DetailsProgressValue = value;
            };

            MainFormVisualizer.MainProgressValueChanged += (value, detail) =>
            {
                ServerStatus.MainProgressValue = value;

                MainProgressValueChanged?.Invoke(value, detail);
            };

            MainFormVisualizer.AutoSavingServiceWorkingNotified += (seviceWorking) =>
            {
                ServerStatus.SeviceWorking = seviceWorking;

                AutoSavingServiceWorkingNotified?.Invoke(seviceWorking);
            };

            MainFormVisualizer.MarketDataChanged += () =>
            {
                MarketDataChanged?.Invoke();
            };

            MainFormVisualizer.NewAlarmsReceived += (alarms) =>
            {
                InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

                try
                {
                    Alarms.Clear();
                    Alarms.AddRange(alarms);
                }
                finally
                {
                    InitializeNewSessionLock.ReleaseReaderLock();
                }

                NewAlarmsReceived?.Invoke(alarms);
            };

            MainFormVisualizer.NewAlarmsHistoryReceived += (alarms) =>
            {
                InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

                try
                {
                    foreach (var alarm in alarms)
                    {
                        AlarmsHistory.Insert(0, alarm);
                    }
                }
                finally
                {
                    InitializeNewSessionLock.ReleaseReaderLock();
                }

                NewAlarmsHistoryReceived?.Invoke(alarms);
            };

            MainFormVisualizer.StatusBarInformationReceived += (information) =>
            {
                ServerStatus.AllSymbolsSync = information.Item2;

                StatusBarInformationReceived?.Invoke(information);
            };

            MainFormVisualizer.BinanceConnectionStatus += (status) =>
            {
                ServerStatus.BinanceConnectionStatus = status;

                BinanceConnectionStatus?.Invoke(status);
            };

            ServerStatus.PropertyChanged += (sender, e) =>
            {
                ServerStatusPropertyChanged?.Invoke(sender, e);
            };

            //
            VisualizerCancellationToken = new CancellationTokenSource();

            var cancellationToken = VisualizerCancellationToken.Token;

            if (DatabaseSupport)
            {
                Task.Run(() => MainFormVisualizer.StartMarket(ServerConstantHelper.DatabaseStartSavingDataDateTime, ServerConstantHelper.MilestoneSavingDataDateTime, cancellationToken), cancellationToken);
            }
            else
            {
                Task.Run(() => MainFormVisualizer.StartMarket(null, ServerConstantHelper.MilestoneSavingDataDateTime, cancellationToken), cancellationToken);
            }
        }

        public void Stop()
        {
            VisualizerCancellationToken?.Cancel();
            VisualizerCancellationToken?.Dispose();

            MainFormVisualizer.Stop();
        }

        public bool GetMenuItemsStatus(UiClientTypes clientType, CommandTypes commandType)
        {
            var result = false;

            if (clientType == UiClientTypes.Admin)
            {
                if (AdminMenuItemsStatus.ContainsKey(commandType))
                {
                    result = AdminMenuItemsStatus[commandType];
                }
            }
            else if (clientType == UiClientTypes.Gold)
            {
                if (GoldMenuItemsStatus.ContainsKey(commandType))
                {
                    result = GoldMenuItemsStatus[commandType];
                }
            }
            else if (clientType == UiClientTypes.Standard)
            {
                if (StandardMenuItemsStatus.ContainsKey(commandType))
                {
                    result = StandardMenuItemsStatus[commandType];
                }
            }
            else
            {
                if (ViewMenuItemsStatus.ContainsKey(commandType))
                {
                    result = ViewMenuItemsStatus[commandType];
                }
            }

            return result;
        }

        public bool RunAlarm(string script, string filename, string username)
        {
            var result = false;

            var newFilename = username + "_" + DateTime.UtcNow.ToString("yyyyMMdd HHmmssff") + "_" + filename;

            var fullFilename = Path.Combine(ServerAddressHelper.AlarmDataFolder, newFilename + ".txt");
            var fullFilenameBackup = Path.Combine(ServerAddressHelper.AlarmDataBackupFolder, newFilename + ".txt");

            if (MainFormVisualizer.RunAlarm(script, fullFilename))
            {
                File.WriteAllText(fullFilename, script);
                File.WriteAllText(fullFilenameBackup, script);

                result = true;
            }

            return result;
        }

        public bool RunTemplate(string script, SymbolTypes[] symbols, string filename, string username)
        {
            var result = true;

            foreach (var symbol in symbols)
            {
                var modifiedScript = script.Replace("{s}", symbol.ToString()).Replace("{S}", symbol.ToString());

                var newFilename = username + "_" + DateTime.UtcNow.ToString("yyyyMMdd HHmmssff") + "_" + filename + "_" + symbol;

                var fullFilename = Path.Combine(ServerAddressHelper.AlarmDataFolder, newFilename + ".txt");
                var fullFilenameBackup = Path.Combine(ServerAddressHelper.AlarmDataBackupFolder, newFilename + ".txt");

                if (MainFormVisualizer.RunAlarm(modifiedScript, fullFilename))
                {
                    File.WriteAllText(fullFilename, modifiedScript);
                    File.WriteAllText(fullFilenameBackup, modifiedScript);
                }
                else
                {
                    result = false;

                    break;
                }
            }

            return result;
        }

        public string ReadAlarmScriptByUsername(string username, UiClientTypes clientType, Guid id)
        {
            var result = "";

            for (var index = MainFormVisualizer.Alarms.Count - 1; index >= 0; index--)
            {
                var alarm = MainFormVisualizer.Alarms[index].Item1;

                if (alarm.Id == id)
                {
                    if (alarm.Username == username || clientType == UiClientTypes.Admin)
                    {
                        var filename = alarm.FileName;

                        result = File.ReadAllText(filename);
                    }

                    break;
                }
            }

            return result;
        }

        public AlarmItemDataModel EvaluateAlarmByUsername(string username, UiClientTypes clientType, Guid id, DateTime datetime)
        {
            AlarmItemDataModel result = null;

            for (var index = MainFormVisualizer.Alarms.Count - 1; index >= 0; index--)
            {
                var alarm = MainFormVisualizer.Alarms[index].Item1;

                if (alarm.Id == id)
                {
                    if (alarm.Username == username || clientType == UiClientTypes.Admin)
                    {
                        var liveHistoryService = new LiveHistoryService(SupportedSymbols, null);

                        if (liveHistoryService.Update(datetime, SupportedSymbols.ToArray()))
                        {
                            string name = "";
                            var symbol = SymbolTypes.BtcUsdt;
                            var position = PositionTypes.Long;

                            result = liveHistoryService.EvaluateAlarm(AlarmHelper.ConvertStringToAlarmItem(File.ReadAllText(alarm.FileName), ref name, ref symbol, ref position));
                        }
                    }

                    break;
                }
            }

            return result;
        }

        public string GetAlarmRelatedUsername(Guid id)
        {
            var result = "";

            for (var index = 0; index < MainFormVisualizer.Alarms.Count; index++)
            {
                var alarm = MainFormVisualizer.Alarms[index].Item1;

                if (alarm.Id == id)
                {
                    result = alarm.Username;

                    break;
                }
            }

            return result;
        }

        public bool SeenAlarmByUsername(string username, UiClientTypes clientType, Guid id)
        {
            var result = false;

            for (var index = MainFormVisualizer.Alarms.Count - 1; index >= 0; index--)
            {
                var alarm = MainFormVisualizer.Alarms[index].Item1;

                if (alarm.Id == id)
                {
                    if (alarm.Username == username || clientType == UiClientTypes.Admin)
                    {
                        alarm.SeenAlarm();

                        result = true;
                    }

                    break;
                }
            }

            return result;
        }

        public List<Guid> SeenAllAlarmByUsername(string username, UiClientTypes clientType)
        {
            var result = new List<Guid>();

            for (var index = MainFormVisualizer.Alarms.Count - 1; index >= 0; index--)
            {
                var alarm = MainFormVisualizer.Alarms[index].Item1;

                if (alarm.Username == username || clientType == UiClientTypes.Admin)
                {
                    alarm.SeenAlarm();

                    result.Add(alarm.Id);
                }
            }

            return result;
        }

        public bool EnableDisableAlarmByUsername(string username, UiClientTypes clientType, Guid id)
        {
            var result = false;

            for (var index = MainFormVisualizer.Alarms.Count - 1; index >= 0; index--)
            {
                var alarm = MainFormVisualizer.Alarms[index].Item1;

                if (alarm.Id == id)
                {
                    if (alarm.Username == username || clientType == UiClientTypes.Admin)
                    {
                        alarm.SetEnabled(!alarm.Enabled);

                        result = true;
                    }

                    break;
                }
            }

            return result;
        }

        public bool DeleteAlarmByUsername(string username, UiClientTypes clientType, Guid id)
        {
            var result = MainFormVisualizer.RemoveAlarm(username, clientType, id); ;

            for (var index = Alarms.Count - 1; index >= 0; index--)
            {
                if (Alarms[index].Id == id)
                {
                    if (Alarms[index].Username == username || clientType == UiClientTypes.Admin)
                    {
                        Alarms.RemoveAt(index);
                    }

                    break;
                }
            }

            foreach (var data in MarketData)
            {
                var alarms = data.SymbolAlarms;

                for (var index = alarms.Count - 1; index >= 0; index--)
                {
                    if (alarms[index].Id == id)
                    {
                        if (alarms[index].Username == username || clientType == UiClientTypes.Admin)
                        {
                            alarms.RemoveAt(index);
                        }
                    }
                }
            }

            return result;
        }

        public List<SymbolDataModel> GetLiveHistory(string username, SymbolTypes[] symbols, DateTime dateTime)
        {
            List<SymbolDataModel> result = null;

            var liveHistory = GetLiveHistoryService(username);

            if (liveHistory.Update(dateTime, symbols))
            {
                var symbolsList = symbols.ToList();

                result = liveHistory.SymbolDataModelList.Where(p => symbolsList.Contains(p.Symbol)).ToList();
            }

            return result;
        }

        public List<SymbolDataModel> GetFirstLiveHistory(string username, SymbolTypes[] symbols)
        {
            List<SymbolDataModel> result = null;

            var liveHistory = GetLiveHistoryService(username);

            if (liveHistory.UpdateToFirstAvailableDateTime(symbols))
            {
                var symbolsList = symbols.ToList();

                result = liveHistory.SymbolDataModelList.Where(p => symbolsList.Contains(p.Symbol)).ToList();
            }

            return result;
        }

        public List<SymbolDataModel> GetLastLiveHistory(string username, SymbolTypes[] symbols)
        {
            List<SymbolDataModel> result = null;

            var liveHistory = GetLiveHistoryService(username);

            if (liveHistory.UpdateToLastAvailableDateTime(symbols))
            {
                var symbolsList = symbols.ToList();

                result = liveHistory.SymbolDataModelList.Where(p => symbolsList.Contains(p.Symbol)).ToList();
            }

            return result;
        }

        public StrategyTestService InitializeTestNewStrategy(Guid sessionId, string username, StrategyTestDataModel strategyTestData, object parameter1, object parameter2, object parameter3)
        {
            StrategyTestService result = null;

            var validation = false;

            var directoryName = DateTime.UtcNow.ToString("yyyyMMdd HHmmssff") + "-" + username;

            if (!string.IsNullOrWhiteSpace(strategyTestData.Name))
            {
                directoryName = directoryName + "-" + strategyTestData.Name;
            }

            var directoryAddress = Path.Combine(ServerAddressHelper.StrategyTestDataFolder, directoryName);

            //
            try
            {
                Directory.CreateDirectory(directoryAddress);

                if (Directory.Exists(directoryAddress))
                {
                    //
                    if (!string.IsNullOrWhiteSpace(strategyTestData.Enter.Alarm))
                    {
                        if (parameter1 != null && parameter1 is byte[] && ((byte[])parameter1).Length > 0)
                        {
                            var filename = Path.Combine(directoryAddress, "EnterAlarm.txt");

                            File.WriteAllBytes(filename, (byte[])parameter1);

                            strategyTestData.Enter.Alarm = filename;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(strategyTestData.ExitTakeProfit.Alarm))
                    {
                        if (parameter2 != null && parameter2 is byte[] && ((byte[])parameter2).Length > 0)
                        {
                            var filename = Path.Combine(directoryAddress, "ExitTakeProfitAlarm.txt");

                            File.WriteAllBytes(filename, (byte[])parameter2);

                            strategyTestData.ExitTakeProfit.Alarm = filename;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(strategyTestData.ExitStopLoss.Alarm))
                    {
                        if (parameter3 != null && parameter3 is byte[] && ((byte[])parameter3).Length > 0)
                        {
                            var filename = Path.Combine(directoryAddress, "ExitStopLossAlarm.txt");

                            File.WriteAllBytes(filename, (byte[])parameter3);

                            strategyTestData.ExitStopLoss.Alarm = filename;
                        }
                    }

                    File.WriteAllText(Path.Combine(directoryAddress, "StrategyTestData.txt"), strategyTestData.ToString());

                    validation = true;
                }
                else
                {
                    validation = false;
                }
            }
            catch
            {
                validation = false;
            }

            //
            if (validation)
            {
                result = new StrategyTestService(strategyTestData, sessionId, directoryAddress);

                result.StrategyTestStatusChanged += StrategyTestService_StrategyTestStatusChanged;

                while (!StrategyTestServices.TryAdd(strategyTestData.Id, result))
                {
                    Thread.Sleep(0);
                }
            }

            return result;
        }

        public void TestNewStrategy(StrategyTestService strategyTest)
        {
            Task.Run(() =>
            {
                if (strategyTest.Init())
                {
                    strategyTest.Start();
                }
            });
        }

        public void StopStrategyTest(Guid strategyId)
        {
            if (StrategyTestServices.ContainsKey(strategyId))
            {
                StrategyTestService strategyTest = null;

                while (!StrategyTestServices.TryRemove(strategyId, out strategyTest))
                {
                    Thread.Sleep(0);
                }

                if (strategyTest != null)
                {
                    strategyTest.StrategyTestStatusChanged -= StrategyTestService_StrategyTestStatusChanged;

                    strategyTest.Stop();
                }
            }
        }

        public event PropertyChangedEventHandler ServerStatusPropertyChanged;

        public event ProgressValueChangedHandler MainProgressValueChanged;

        public event MarketDataChangedHandler MarketDataChanged;

        public event AlarmsReceivedHandler NewAlarmsReceived;

        public event AlarmsReceivedHandler NewAlarmsHistoryReceived;

        public event ServiceWorkingNotifiedHandler AutoSavingServiceWorkingNotified;

        public event StatusBarInformationReceivedHandler StatusBarInformationReceived;

        public event MenuItemsStatusChangedHandler MenuItemsStatusChanged;

        public event BinanceConnectionStatusHandler BinanceConnectionStatus;

        public event StrategyTestStatusChangedHandler StrategyTestStatusChanged;
    }
}

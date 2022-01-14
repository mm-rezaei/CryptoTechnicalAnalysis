using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DevExpress.Data;
using DevExpress.Xpf.Bars;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;
using TechnicalAnalysisTools.Ui.Win.Delegates;
using TechnicalAnalysisTools.Ui.Win.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class LiveHistoryWindow : Window
    {
        public LiveHistoryWindow(bool viewScriptStatus, bool evaluateAlarmStatus, SymbolTypes[] selectedSymbols)
        {
            InitializeComponent();

            SelectedSymbols = selectedSymbols;

            GridControlMain.ItemsSource = SymbolDataModelList;

            MenuItemViewScript.IsEnabled = viewScriptStatus;
            MenuItemEvaluateAlarmViewerWindow.IsEnabled = evaluateAlarmStatus;
        }

        private SymbolTypes[] SelectedSymbols { get; }

        private ObservableCollection<SymbolDataModel> SymbolDataModelList { get; } = new ObservableCollection<SymbolDataModel>();

        private DateTime? CurrentDateTime { get; set; }

        private async void GoDateTime(DateTime? newDateTime)
        {
            IsEnabled = false;

            await Task.Run(() => Thread.Sleep(1));

            if (LiveHistoryRequested != null && SelectedSymbols.Length != 0)
            {
                var symbolDataModelList = await LiveHistoryRequested.Invoke(newDateTime, SelectedSymbols);

                if (symbolDataModelList == null || symbolDataModelList.Count == 0)
                {
                    if (newDateTime == null || newDateTime == DateTime.MinValue || newDateTime == DateTime.MaxValue)
                    {
                        MessageBox.Show("An error occured when data was retreiving. Please try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("The new time '" + DateTimeHelper.ConvertDateTimeToString(newDateTime.Value) + "' is not valid, or An error occured when data was retreiving. Please try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    await Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (SymbolDataModelList.Count == 0)
                        {
                            foreach (var symbolDataModel in symbolDataModelList)
                            {
                                SymbolDataModelList.Add(symbolDataModel);

                                symbolDataModel.SupportsResistances = new ObservableCollection<SymbolSupportsResistancesDataModel>();

                                SupportResistanceHelper.FillSupportsResistances(symbolDataModel);

                                GridControlSupportResistance.SortBy(GridControlSupportResistance.Columns["Price"], ColumnSortOrder.Descending);

                                GridControlSupportResistance.RefreshData();
                            }
                        }
                        else
                        {
                            foreach (var symbolDataModel in SymbolDataModelList)
                            {
                                var receivedSymbolDataModel = symbolDataModelList.FirstOrDefault(p => p.Symbol == symbolDataModel.Symbol);

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
                        }

                        if (SymbolDataModelList.Count != 0)
                        {
                            CurrentDateTime = SymbolDataModelList[0].LastMinuteCandle;
                            DateEditCurrentDateTime.DateTime = SymbolDataModelList[0].LastMinuteCandle;
                        }
                        else
                        {
                            DateEditCurrentDateTime.DateTime = DateTime.MinValue;
                        }
                    }), DispatcherPriority.Send);
                }
            }

            IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutHelper.LoadLayout(this, GridControlMain);
            LayoutHelper.LoadLayout(this, GridControlTimeFrames);

            GoDateTime(null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutHelper.SaveLayout(this, GridControlMain);
            LayoutHelper.SaveLayout(this, GridControlTimeFrames);
        }

        private async void MenuItemViewScript_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (AlarmScriptRequested != null)
            {
                if (GridControlAlarms.SelectedItem != null)
                {
                    var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                    var script = await AlarmScriptRequested.Invoke(alarm.Id);

                    var scriptViewer = new AlarmScriptViewerWindow(alarm.Name, script);

                    scriptViewer.Show();
                }
            }
        }

        private async void MenuItemEvaluateAlarmViewerWindow_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (EvaluateAlarmRequested != null)
            {
                if (GridControlAlarms.SelectedItem != null)
                {
                    //
                    var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                    var lastAlarm = alarm.LastAlarm;

                    if (CurrentDateTime.HasValue)
                    {
                        lastAlarm = CurrentDateTime.Value;
                    }

                    var evaluatedAlarmItem = await EvaluateAlarmRequested.Invoke(alarm.Id, lastAlarm);

                    //
                    var alarmEvaluationViewerWindow = new AlarmEvaluationViewerWindow(evaluatedAlarmItem, alarm.Name, alarm.Symbol, lastAlarm);

                    alarmEvaluationViewerWindow.Show();
                }
            }
        }

        private void ButtonNavigation_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentDateTime.HasValue)
            {
                //
                var buttonName = ((Button)sender).Name;

                var isNext = buttonName.Contains("Next");

                var timeFrameText = buttonName.Replace("ButtonPrevious", "").Replace("ButtonNext", "");

                var timeFrame = (TimeFrames)Enum.Parse(typeof(TimeFrames), timeFrameText);

                //
                var newDateTime = CurrentDateTime.Value.AddMinutes((isNext ? 1 : -1) * ((int)timeFrame));

                GoDateTime(newDateTime);
            }
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            var newDateTime = DateEditCurrentDateTime.DateTime;

            GoDateTime(newDateTime);
        }

        private void ButtonGoFirst_Click(object sender, RoutedEventArgs e)
        {
            GoDateTime(DateTime.MinValue);
        }

        private void ButtonGoLast_Click(object sender, RoutedEventArgs e)
        {
            GoDateTime(DateTime.MaxValue);
        }

        public event AlarmScriptRequestedHandler AlarmScriptRequested;

        public event LiveHistoryRequestedHandler LiveHistoryRequested;

        public event EvaluateAlarmRequestedHandler EvaluateAlarmRequested;
    }
}

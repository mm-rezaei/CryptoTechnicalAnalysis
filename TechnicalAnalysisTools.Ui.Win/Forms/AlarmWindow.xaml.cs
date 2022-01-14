using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.Xpf.Bars;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;
using TechnicalAnalysisTools.Ui.Win.Delegates;
using TechnicalAnalysisTools.Ui.Win.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class AlarmWindow : Window
    {
        public AlarmWindow(ObservableCollection<SymbolAlarmDataModel> alarms)
        {
            InitializeComponent();

            GridControlAlarms.ItemsSource = Alarms = alarms;

            Timer.Tick += Timer_Tick;

            Timer.Start();
        }

        private SoundPlayer AlarmPlayer { get; } = new SoundPlayer(SharedAddressHelper.AlarmWavFile);

        public ObservableCollection<SymbolAlarmDataModel> Alarms { get; }

        public DispatcherTimer Timer { get; } = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) };

        private bool EnabledSound { get; set; } = true;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Alarms != null)
            {
                if (Alarms.All(p => p.Seen))
                {
                    AlarmPlayer.Stop();
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AlarmPlayer.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutHelper.LoadLayout(this, GridControlAlarms);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LayoutHelper.SaveLayout(this, GridControlAlarms);

            AlarmPlayer.Stop();

            Hide();

            e.Cancel = true;
        }

        private void MenuItemSeenAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                IsEnabled = false;

                SeenAlarmsReceived?.Invoke(new List<SymbolAlarmDataModel>() { (SymbolAlarmDataModel)GridControlAlarms.SelectedItem });

                IsEnabled = true;
            }
        }

        private void MenuItemSeenAllAlarm_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            SeenAlarmsReceived?.Invoke(null);

            IsEnabled = true;
        }

        private void MenuItemEnableDisableAlarm_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                IsEnabled = false;

                EnableDisableAlarmsReceived?.Invoke(new List<SymbolAlarmDataModel>() { (SymbolAlarmDataModel)GridControlAlarms.SelectedItem });

                IsEnabled = true;
            }
        }

        private async void MenuItemViewScript_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (AlarmScriptRequested != null)
            {
                if (GridControlAlarms.SelectedItem != null)
                {
                    IsEnabled = false;

                    var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                    var script = await AlarmScriptRequested.Invoke(alarm.Id);

                    var scriptViewer = new AlarmScriptViewerWindow(alarm.Name, script);

                    scriptViewer.Show();

                    IsEnabled = true;
                }
            }
        }

        private async void MenuItemEvaluateAlarmViewerWindow_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (EvaluateAlarmRequested != null)
            {
                if (GridControlAlarms.SelectedItem != null)
                {
                    IsEnabled = false;

                    //
                    var alarm = (SymbolAlarmDataModel)GridControlAlarms.SelectedItem;

                    var lastAlarm = alarm.LastAlarm;

                    var evaluatedAlarmItem = await EvaluateAlarmRequested.Invoke(alarm.Id, lastAlarm);

                    //
                    var alarmEvaluationViewerWindow = new AlarmEvaluationViewerWindow(evaluatedAlarmItem, alarm.Name, alarm.Symbol, lastAlarm);

                    alarmEvaluationViewerWindow.Show();

                    IsEnabled = true;
                }
            }
        }

        private void MenuItemDeleteAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (GridControlAlarms.SelectedItem != null)
            {
                IsEnabled = false;

                RemovingAlarmsReceived?.Invoke(new List<SymbolAlarmDataModel>() { (SymbolAlarmDataModel)GridControlAlarms.SelectedItem });

                IsEnabled = true;
            }
        }

        private void ImageSound_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (EnabledSound)
            {
                EnabledSound = false;

                AlarmPlayer.Stop();

                ImageSound.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/Mute.png"));
            }
            else
            {
                EnabledSound = true;

                ImageSound.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/Unmute.png"));
            }
        }

        public void SetMenuItemStatus(CommandTypes command, bool status)
        {
            switch (command)
            {
                case CommandTypes.SeenAlarm:
                    MenuItemSeenAlarm.IsEnabled = status;
                    break;
                case CommandTypes.SeenAllAlarm:
                    MenuItemSeenAllAlarm.IsEnabled = status;
                    break;
                case CommandTypes.EnableDisableAlarm:
                    MenuItemEnableDisableAlarm.IsEnabled = status;
                    break;
                case CommandTypes.ReadAlarmScript:
                    MenuItemViewScript.IsEnabled = status;
                    break;
                case CommandTypes.EvaluateAlarm:
                    MenuItemEvaluateAlarmViewerWindow.IsEnabled = status;
                    break;
                case CommandTypes.DeleteAlarm:
                    MenuItemDeleteAlarm.IsEnabled = status;
                    break;
            }
        }

        public void ActiveAlarms()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Alarms.Count != 0)
                {
                    WindowState = WindowState.Normal;

                    AlarmPlayer.Stop();

                    Show();

                    Activate();

                    if (EnabledSound)
                    {
                        AlarmPlayer.PlayLooping();
                    }
                }
            }));
        }

        public void CloseAlarmWindow()
        {
            Closing -= Window_Closing;

            AlarmPlayer.Stop();

            Close();
        }

        public event AlarmsReceivedHandler SeenAlarmsReceived;

        public event AlarmsReceivedHandler EnableDisableAlarmsReceived;

        public event AlarmScriptRequestedHandler AlarmScriptRequested;

        public event EvaluateAlarmRequestedHandler EvaluateAlarmRequested;

        public event AlarmsReceivedHandler RemovingAlarmsReceived;
    }
}

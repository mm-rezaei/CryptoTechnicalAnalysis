using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Ui.Win.Controls
{
    internal partial class TradePositionUserControl : UserControl
    {
        public TradePositionUserControl()
        {
            InitializeComponent();
        }

        public TradeSubOrderTypes TradeSubOrderType
        {
            set
            {
                TextBlockTitle.Text = value.ToString() + " Trade Sub Order";

                switch (value)
                {
                    case TradeSubOrderTypes.Enter:
                        //
                        ComboBoxTriggerMode.ItemsSource = new TradeSubOrderTriggerModes[] { TradeSubOrderTriggerModes.Alarm };
                        ComboBoxTriggerMode.SelectedItem = TradeSubOrderTriggerModes.Alarm;

                        //
                        ComboBoxOrderMode.ItemsSource = Enum.GetValues(typeof(TradeSubOrderModes));
                        ComboBoxOrderMode.SelectedItem = TradeSubOrderModes.None;

                        //
                        ComboBoxTriggerMode.IsEnabled = false;
                        TextBoxAlarm.IsEnabled = true;
                        TextBoxPercentAmount.IsEnabled = false;
                        TextBoxFixedAmount.IsEnabled = false;
                        ComboBoxOrderMode.IsEnabled = true;
                        TextBoxTrailing.IsEnabled = false;
                        TextBoxGridPercent.IsEnabled = false;
                        TextBoxGridStepCount.IsEnabled = false;
                        break;
                    case TradeSubOrderTypes.ExitTakeProfit:
                        //
                        ComboBoxTriggerMode.ItemsSource = Enum.GetValues(typeof(TradeSubOrderTriggerModes));
                        ComboBoxTriggerMode.SelectedItem = TradeSubOrderTriggerModes.Alarm;

                        //
                        ComboBoxOrderMode.ItemsSource = Enum.GetValues(typeof(TradeSubOrderModes));
                        ComboBoxOrderMode.SelectedItem = TradeSubOrderModes.None;

                        //
                        ComboBoxTriggerMode.IsEnabled = true;
                        TextBoxAlarm.IsEnabled = true;
                        TextBoxPercentAmount.IsEnabled = false;
                        TextBoxFixedAmount.IsEnabled = false;
                        ComboBoxOrderMode.IsEnabled = true;
                        TextBoxTrailing.IsEnabled = false;
                        TextBoxGridPercent.IsEnabled = false;
                        TextBoxGridStepCount.IsEnabled = false;
                        break;
                    case TradeSubOrderTypes.ExitStopLoss:
                        //
                        ComboBoxTriggerMode.ItemsSource = Enum.GetValues(typeof(TradeSubOrderTriggerModes));
                        ComboBoxTriggerMode.SelectedItem = TradeSubOrderTriggerModes.Alarm;

                        //
                        ComboBoxOrderMode.ItemsSource = new TradeSubOrderModes[] { TradeSubOrderModes.None };
                        ComboBoxOrderMode.SelectedItem = TradeSubOrderModes.None;

                        //
                        ComboBoxTriggerMode.IsEnabled = true;
                        TextBoxAlarm.IsEnabled = true;
                        TextBoxPercentAmount.IsEnabled = false;
                        TextBoxFixedAmount.IsEnabled = false;
                        ComboBoxOrderMode.IsEnabled = false;
                        TextBoxTrailing.IsEnabled = false;
                        TextBoxGridPercent.IsEnabled = false;
                        TextBoxGridStepCount.IsEnabled = false;
                        break;
                }
            }
        }

        private void ComboBoxTriggerMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTriggerMode.SelectedIndex != -1)
            {
                var triggerMode = (TradeSubOrderTriggerModes)ComboBoxTriggerMode.SelectedItem;

                switch (triggerMode)
                {
                    case TradeSubOrderTriggerModes.Alarm:
                        {
                            TextBoxAlarm.IsEnabled = true;
                            TextBoxPercentAmount.IsEnabled = false;
                            TextBoxFixedAmount.IsEnabled = false;
                        }
                        break;
                    case TradeSubOrderTriggerModes.Percent:
                        {
                            TextBoxAlarm.IsEnabled = false;
                            TextBoxPercentAmount.IsEnabled = true;
                            TextBoxFixedAmount.IsEnabled = false;
                        }
                        break;
                    case TradeSubOrderTriggerModes.Fixed:
                        {
                            TextBoxAlarm.IsEnabled = false;
                            TextBoxPercentAmount.IsEnabled = false;
                            TextBoxFixedAmount.IsEnabled = true;
                        }
                        break;
                }
            }
        }

        private void ComboBoxOrderMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxOrderMode.SelectedIndex != -1)
            {
                var orderMode = (TradeSubOrderModes)ComboBoxOrderMode.SelectedItem;

                switch (orderMode)
                {
                    case TradeSubOrderModes.None:
                        {
                            TextBoxTrailing.IsEnabled = false;
                            TextBoxGridPercent.IsEnabled = false;
                            TextBoxGridStepCount.IsEnabled = false;
                        }
                        break;
                    case TradeSubOrderModes.TrailingOrder:
                        {
                            TextBoxTrailing.IsEnabled = true;
                            TextBoxGridPercent.IsEnabled = false;
                            TextBoxGridStepCount.IsEnabled = false;
                        }
                        break;
                    case TradeSubOrderModes.GridOrder:
                        {
                            TextBoxTrailing.IsEnabled = false;
                            TextBoxGridPercent.IsEnabled = true;
                            TextBoxGridStepCount.IsEnabled = true;
                        }
                        break;
                }
            }
        }

        private void TextBoxAlarm_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Text file (*.txt)|*.txt";
            fileDialog.CheckFileExists = true;

            var fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult.HasValue && fileDialogResult.Value)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    TextBoxAlarm.Text = fileDialog.FileName;
                }
            }
        }
    }
}

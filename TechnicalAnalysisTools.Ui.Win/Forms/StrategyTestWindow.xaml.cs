using DevExpress.Xpf.PropertyGrid;
using StockSharp.Algo.Indicators;
using StockSharp.Xaml.Charting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;
using TechnicalAnalysisTools.Ui.Win.Auxiliaries;
using TechnicalAnalysisTools.Ui.Win.Delegates;

namespace TechnicalAnalysisTools.Forms
{
    internal partial class StrategyTestWindow : Window
    {
        public StrategyTestWindow()
        {
            InitializeComponent();

            InitializeControls();
        }

        private bool _IsTestRunning;

        private bool IsTestRunning
        {
            get { return _IsTestRunning; }
            set
            {
                _IsTestRunning = value;

                if (value)
                {
                    GridSettings.IsEnabled = false;
                    GridExpert.IsEnabled = false;
                    ComboBoxMinuteCandlePriceDirection.IsEnabled = false;
                }
                else
                {
                    GridSettings.IsEnabled = true;
                    GridExpert.IsEnabled = true;
                    ComboBoxMinuteCandlePriceDirection.IsEnabled = true;
                }
            }
        }

        private StrategyTestDataModel StrategyTestData { get; set; } = new StrategyTestDataModel();

        private IndicatorSelectorItem[] IndicatorSelectorItems { get; set; }

        private FinancialChartAuxiliary FinancialChart { get; set; }

        private ObservableCollection<StrategyTestLogDataModel> StrategyTestLogs { get; set; } = new ObservableCollection<StrategyTestLogDataModel>();

        private ObservableCollection<StrategyTestOrderDataModel> StrategyTestOrders { get; set; } = new ObservableCollection<StrategyTestOrderDataModel>();

        private StrategyTestReportDataModel StrategyTestReport { get; set; } = new StrategyTestReportDataModel();

        private Button ButtonTest { get; set; }

        private ComboBox ComboBoxMinuteCandlePriceDirection { get; set; }

        private ToggleButton ToggleAutoRange { get; set; }

        private ToggleButton ToggleShowLegend { get; set; }

        private ChartArea TotalBalanceArea { get; set; }

        private ChartIndicatorElement TotalBalanceIndicatorElement { get; set; }

        private FakeIndicatorAuxiliary TotalBalanceFakeIndicator { get; set; } = new FakeIndicatorAuxiliary();

        private void InitializeControls()
        {
            //
            ComboBoxSymbol.ItemsSource = Enum.GetValues(typeof(SymbolTypes));
            ComboBoxPosition.ItemsSource = Enum.GetValues(typeof(PositionTypes));
            ComboBoxTradeAmountType.ItemsSource = Enum.GetValues(typeof(TradeAmountModes));
            ComboBoxVisualTimeFrame.ItemsSource = Enum.GetValues(typeof(TimeFrames));
            ComboBoxVisualTickFrame.ItemsSource = Enum.GetValues(typeof(TimeFrames));

            var indicators = new List<Shared.Enumerations.IndicatorType>
            {
                Shared.Enumerations.IndicatorType.BollingerBands,
                Shared.Enumerations.IndicatorType.Ichimoku,
                Shared.Enumerations.IndicatorType.Macd,
                Shared.Enumerations.IndicatorType.Rsi,
                Shared.Enumerations.IndicatorType.Stoch,
                Shared.Enumerations.IndicatorType.StochRsi,
                Shared.Enumerations.IndicatorType.Ema9,
                Shared.Enumerations.IndicatorType.Ema20,
                Shared.Enumerations.IndicatorType.Ema26,
                Shared.Enumerations.IndicatorType.Ema30,
                Shared.Enumerations.IndicatorType.Ema40,
                Shared.Enumerations.IndicatorType.Ema50,
                Shared.Enumerations.IndicatorType.Ema100,
                Shared.Enumerations.IndicatorType.Ema200,
                Shared.Enumerations.IndicatorType.Sma9,
                Shared.Enumerations.IndicatorType.Sma20,
                Shared.Enumerations.IndicatorType.Sma26,
                Shared.Enumerations.IndicatorType.Sma30,
                Shared.Enumerations.IndicatorType.Sma40,
                Shared.Enumerations.IndicatorType.Sma50,
                Shared.Enumerations.IndicatorType.Sma100,
                Shared.Enumerations.IndicatorType.Sma200
            };

            IndicatorSelectorItems = indicators.Select(p => new IndicatorSelectorItem() { Checked = false, Indicator = p }).ToArray();
            ListBoxIndicatorSelector.ItemsSource = IndicatorSelectorItems;

            //
            ChartMain.ChartTheme = ChartThemes.BrightSpark;
            ChartTotalBalanceGraph.ChartTheme = ChartThemes.BrightSpark;

            //
            var dateTimeNow = DateTime.UtcNow.AddMinutes(-1);

            dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0);

            StrategyTestData = new StrategyTestDataModel()
            {
                Id = Guid.NewGuid(),
                Name = "New Test",
                Symbol = SymbolTypes.BtcUsdt,
                FromDateTime = dateTimeNow.AddMonths(-1),
                ToDateTime = dateTimeNow,
                Position = PositionTypes.Long,
                InitialBaseCoinDeposit = 1000,
                MarketFeePercent = 0.04f,
                Leverage = 1,
                VisualMode = true,
                VisualIndicators = new Shared.Enumerations.IndicatorType[0],
                VisualSkipToDateTime = dateTimeNow.AddMonths(-1),
                VisualTimeFrame = TimeFrames.Minute15,
                VisualTickFrame = TimeFrames.Minute15,
                VisualTickPerSecond = 100,
                SaveProfitPercentOfWinPosition = 0,
                TradeAmountMode = TradeAmountModes.Percent,
                TradeAmountPercent = 100,
                TradeAmountFixedValue = 0
            };

            GridMain.DataContext = StrategyTestData;

            //
            GridControlLogs.ItemsSource = StrategyTestLogs;
            GridControlOrders.ItemsSource = StrategyTestOrders;
            PropertyGridControlReport.SelectedObject = StrategyTestReport;
        }

        private void StrategyTestData_StrategyTestStatusUpdated(StrategyTestStatusDataModel strategyTestStatus)
        {
            //
            if (strategyTestStatus.Candles != null && strategyTestStatus.Candles.Count != 0)
            {
                for (var index = 0; index < strategyTestStatus.Candles.Count; index++)
                {
                    var candle = strategyTestStatus.Candles[index];

                    //
                    var buyCount = 0;
                    var sellCount = 0;

                    if (strategyTestStatus.StrategyTestLogs != null && strategyTestStatus.StrategyTestLogs.Count != 0)
                    {
                        foreach (var log in strategyTestStatus.StrategyTestLogs)
                        {
                            var isValidCandle = log.Time >= candle.OpenDateTime;

                            if (index + 1 != strategyTestStatus.Candles.Count)
                            {
                                var nextCandle = strategyTestStatus.Candles[index + 1];

                                isValidCandle = isValidCandle && log.Time < nextCandle.OpenDateTime;
                            }

                            if (isValidCandle)
                            {
                                switch (log.SubOrderAction)
                                {
                                    case TradeSubOrderActions.Enter:
                                        {
                                            if (StrategyTestData.Position == PositionTypes.Long)
                                            {
                                                buyCount++;
                                            }
                                            else
                                            {
                                                sellCount++;
                                            }
                                        }
                                        break;
                                    case TradeSubOrderActions.Exit:
                                    case TradeSubOrderActions.TakeProfit:
                                    case TradeSubOrderActions.StopLoss:
                                    case TradeSubOrderActions.Liquid:
                                        {
                                            if (StrategyTestData.Position == PositionTypes.Long)
                                            {
                                                sellCount++;
                                            }
                                            else
                                            {
                                                buyCount++;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    FinancialChart.ProcessCandle(candle, buyCount, -1 * sellCount, TimeFrameHelper.IsThisMinuteCandleFirstTimeFrameCandle(candle.MomentaryDateTime, StrategyTestData.VisualTimeFrame));

                    // Update Total Balance
                    var data = new ChartDrawData();

                    var decimalIndicatorValue = new DecimalIndicatorValue(TotalBalanceFakeIndicator, Convert.ToDecimal(strategyTestStatus.TotalBalance));

                    decimalIndicatorValue.IsFinal = true;
                    decimalIndicatorValue.IsEmpty = false;

                    data.Group(candle.OpenDateTime).Add(TotalBalanceIndicatorElement, decimalIndicatorValue);

                    ChartTotalBalanceGraph.Draw(data);
                }
            }

            if (strategyTestStatus.StrategyTestLogs != null && strategyTestStatus.StrategyTestLogs.Count != 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var log in strategyTestStatus.StrategyTestLogs)
                    {
                        StrategyTestLogs.Add(log);
                    }
                }), DispatcherPriority.Send);
            }

            if (strategyTestStatus.StrategyTestOrders != null && strategyTestStatus.StrategyTestOrders.Count != 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var order in strategyTestStatus.StrategyTestOrders)
                    {
                        var oldOrder = StrategyTestOrders.FirstOrDefault(p => p.SubOrderId == order.SubOrderId);

                        if (oldOrder == null)
                        {
                            StrategyTestOrders.Add(order);
                        }
                        else
                        {
                            ReflectionHelper.CopyValuableProperties(order, oldOrder);
                        }
                    }
                }), DispatcherPriority.Send);
            }

            if (strategyTestStatus.StrategyTestReport != null)
            {
                ReflectionHelper.CopyValuableProperties(strategyTestStatus.StrategyTestReport, StrategyTestReport);
            }

            //
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressBarStrategyTest.Value = strategyTestStatus.Progress;
            }));

            //
            switch (strategyTestStatus.StrategyTestStatusType)
            {
                case StrategyTestStatusTypes.Finish:
                    {
                        Stop();

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("The strategy test was successfully finished.", "successful Operation", MessageBoxButton.OK, MessageBoxImage.Information);
                        }));
                    }
                    break;
                case StrategyTestStatusTypes.Error:
                    {
                        Stop();

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (!string.IsNullOrWhiteSpace(strategyTestStatus.Message))
                            {
                                MessageBox.Show(strategyTestStatus.Message, "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                MessageBox.Show("The strategy test was stopped by an error. Try to start new test.", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }));
                    }
                    break;
            }
        }

        private async Task<bool> Start()
        {
            bool result;

            //
            StrategyTestData.Id = Guid.NewGuid();

            FinancialChart = new FinancialChartAuxiliary(StrategyTestData.Symbol, ChartMain, StrategyTestData.VisualIndicators);

            FinancialChart.InitChart();

            //
            StrategyTestData.StrategyTestStatusUpdated -= StrategyTestData_StrategyTestStatusUpdated;
            StrategyTestData.StrategyTestStatusUpdated += StrategyTestData_StrategyTestStatusUpdated;

            result = await StrategyTestRequested?.Invoke(StrategyTestData);

            if (!result)
            {
                StrategyTestData.StrategyTestStatusUpdated -= StrategyTestData_StrategyTestStatusUpdated;
            }

            return result;
        }

        private void Stop()
        {
            if (IsTestRunning)
            {
                StrategyTestData.StrategyTestStatusUpdated -= StrategyTestData_StrategyTestStatusUpdated;

                StrategyTestStopRequested?.Invoke(StrategyTestData.Id);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ButtonTest.Content = "Start";
                    ProgressBarStrategyTest.Value = 0;

                    IsTestRunning = false;
                }), DispatcherPriority.Send);
            }
        }

        private void ButtonTest_Loaded(object sender, RoutedEventArgs e)
        {
            ButtonTest = ((Button)sender);
        }

        private void ToggleAutoRange_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleAutoRange = (ToggleButton)sender;
        }

        private void ToggleShowLegend_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleShowLegend = (ToggleButton)sender;
        }

        private void ComboBoxMinuteCandlePriceDirection_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBoxMinuteCandlePriceDirection = (ComboBox)sender;

            ComboBoxMinuteCandlePriceDirection.ItemsSource = Enum.GetValues(typeof(StrategyTestPriceMovementFlowModes));

            ComboBoxMinuteCandlePriceDirection.SelectedItem = StrategyTestPriceMovementFlowModes.Random;
        }

        private void ComboBoxTradeAmountType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxTradeAmountType.SelectedIndex != -1)
            {
                var tradeAmount = (TradeAmountModes)ComboBoxTradeAmountType.SelectedItem;

                if (tradeAmount == TradeAmountModes.Percent)
                {
                    TextBoxTradeAmountPercent.IsEnabled = true;
                    TextBoxTradeAmountFixed.IsEnabled = false;
                }
                else if (tradeAmount == TradeAmountModes.PercentWithMinimumFixed || tradeAmount == TradeAmountModes.PercentWithMaximumFixed)
                {
                    TextBoxTradeAmountPercent.IsEnabled = true;
                    TextBoxTradeAmountFixed.IsEnabled = true;
                }
                else
                {
                    TextBoxTradeAmountPercent.IsEnabled = false;
                    TextBoxTradeAmountFixed.IsEnabled = true;
                }
            }
        }

        private void ToggleAutoRange_Checked(object sender, RoutedEventArgs e)
        {
            ChartMain.IsAutoRange = true;
            ChartMain.IsAutoScroll = true;
        }

        private void ToggleAutoRange_Unchecked(object sender, RoutedEventArgs e)
        {
            ChartMain.IsAutoRange = false;
            ChartMain.IsAutoScroll = true;
        }

        private void ToggleShowLegend_Checked(object sender, RoutedEventArgs e)
        {
            ChartMain.ShowLegend = true;
        }

        private void ToggleShowLegend_Unchecked(object sender, RoutedEventArgs e)
        {
            ChartMain.ShowLegend = false;
        }

        private void StrategyTest_Closing(object sender, CancelEventArgs e)
        {
            if (IsTestRunning)
            {
                Stop();
            }
        }

        private void PropertyGridControlReport_ShowingEditor(object sender, ShowingPropertyGridEditorEventArgs e)
        {
            e.Cancel = true;
        }

        private async void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            //
            ((Button)sender).IsEnabled = false;

            if (IsTestRunning)
            {
                Stop();
            }
            else
            {
                // Start
                StrategyTestLogs.Clear();
                StrategyTestOrders.Clear();
                StrategyTestReport.SetDefault();

                //
                ChartTotalBalanceGraph.ClearAreas();

                TotalBalanceArea = new ChartArea();

                ChartTotalBalanceGraph.Areas.Add(TotalBalanceArea);

                TotalBalanceIndicatorElement = new ChartIndicatorElement() { Color = Colors.DarkRed };

                TotalBalanceArea.Elements.Add(TotalBalanceIndicatorElement);

                TotalBalanceArea.XAxises[0].DrawMinorGridLines = false;
                TotalBalanceArea.YAxises[0].DrawMinorGridLines = false;
                TotalBalanceArea.XAxises[0].DrawMajorGridLines = false;
                TotalBalanceArea.YAxises[0].DrawMajorGridLines = false;

                //
                StrategyTestData.StrategyTestPriceMovementFlowMode = (StrategyTestPriceMovementFlowModes)ComboBoxMinuteCandlePriceDirection.SelectedItem;

                var selectedIndicators = new Shared.Enumerations.IndicatorType[0];

                if (StrategyTestData.VisualMode)
                {
                    selectedIndicators = IndicatorSelectorItems.Where(p => p.Checked).Select(p => p.Indicator).ToArray();
                }

                StrategyTestData.VisualIndicators = selectedIndicators;

                //
                var message = StrategyTestData.Validation();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    MessageBox.Show(message, "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ButtonTest.IsEnabled = false;

                    ButtonTest.Content = "Stop";

                    IsTestRunning = true;

                    if (!await Start())
                    {
                        ButtonTest.Content = "Start";

                        IsTestRunning = false;

                        MessageBox.Show("The strategy test was failed because of an unknown error. Please try again!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    ButtonTest.IsEnabled = true;
                }
            }

            ((Button)sender).IsEnabled = true;
        }

        public event StrategyTestRequestedHandler StrategyTestRequested;

        public event StrategyTestStopRequestedHandler StrategyTestStopRequested;
    }

    internal class IndicatorSelectorItem : INotifyPropertyChanged
    {
        private bool _Checked;

        private Shared.Enumerations.IndicatorType _Indicator;

        public bool Checked
        {
            get { return _Checked; }
            set { if (_Checked != value) { _Checked = value; OnPropertyChanged(nameof(Checked)); } }
        }

        public Shared.Enumerations.IndicatorType Indicator
        {
            get { return _Indicator; }
            set { if (_Indicator != value) { _Indicator = value; OnPropertyChanged(nameof(Indicator)); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

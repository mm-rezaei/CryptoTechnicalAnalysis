using Binance.Net.Objects.Spot.MarketStream;
using Binance.Net.Objects.Spot.UserStream;
using DevExpress.Xpf.PropertyGrid;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Trading.Ui.Win.Services;
using TechnicalAnalysisTools.Trading.Ui.Win.Strategies;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Forms
{
    internal partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitializeWindow();
        }

        private bool _TradingStarted = false;

        private bool _TradingBotStarted  = false;

        private bool TradingStarted
        {
            get
            {
                return _TradingStarted;
            }
            set
            {
                if(_TradingStarted != value)
                {
                    _TradingStarted = value;


                }
            }
        }

        private bool TradingBotStarted
        {
            get
            {
                return _TradingBotStarted;
            }
            set
            {
                if (_TradingBotStarted != value)
                {
                    _TradingBotStarted = value;


                }
            }
        }

        private BinanceSpotClientService BinanceSpotClient { get; set; }

        private ObservableCollection<SymbolDataModel> MarketData { get; } = new ObservableCollection<SymbolDataModel>();

        private Thread SubscribeToOnMinuteKlineUpdatesThread { get; set; }

        private DumpStrategy CryptoDumpStrategy { get; set; }

        private SymbolTypes[] GetSymbols()
        {
            return new SymbolTypes[] { SymbolTypes.BtcUsdt };
        }

        private void InitializeWindow()
        {
            //
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Title += " " + fileVersionInfo.FileVersion.ToString();

            //
            foreach (var symbol in GetSymbols())
            {
                var candle = new SymbolDataModel()
                {
                    Symbol = symbol
                };

                MarketData.Add(candle);
            }

            GridControlMain.ItemsSource = MarketData;
        }

        private void Start()
        {
            MenuItemStart.IsEnabled = false;
            MenuItemStop.IsEnabled = false;

            //
            BinanceSpotClient = new BinanceSpotClientService("LYFdi1nYzCIdMBqUcRy0LVs9uqrliwVSAmV95Hpr620bgOtRDpSYEeAtvYNGBc9R", "8xWDzAjAAOGCnnp3cM7z0Mp0Z3fuXHKG6wy8ApdrgiL53qdpIKI8m8F2b5p86lPN");

            if (BinanceSpotClient.Init())
            {
                //
                MenuItemStart.IsEnabled = false;
                MenuItemStop.IsEnabled = true;
                MenuItemSpotBuyMarket.IsEnabled = true;
                MenuItemSpotSellMarket.IsEnabled = true;

                //
                BinanceSpotClient.BinanceStreamTickReceived += BinanceSpotClient_BinanceStreamTickReceived; ;
                BinanceSpotClient.BinanceStreamBalanceUpdateReceived += BinanceSpotClient_BinanceStreamBalanceUpdateReceived;

                SubscribeToOnMinuteKlineUpdatesThread = new Thread(() =>
                {
                    foreach (var symbol in GetSymbols())
                    {
                        BinanceSpotClient.SubscribeToBinanceMiniTickUpdates(symbol);
                    }
                });

                SubscribeToOnMinuteKlineUpdatesThread.Start();

                //
                int minutesCount = 5;
                decimal dumpPercent = 1m;
                decimal enterTrailingPercent = 0.167m;
                decimal takeProfitPercent = 0.25m;
                decimal takeProfitTrailingPercent = 0.167m;
                decimal stopLossPercent = 0.34m;
                int roundDigitCount = 5;

                CryptoDumpStrategy = new DumpStrategy(BinanceSpotClient, SymbolTypes.BtcUsdt, 50, minutesCount, dumpPercent, enterTrailingPercent, takeProfitPercent, takeProfitTrailingPercent, stopLossPercent, roundDigitCount);

                CryptoDumpStrategy.LogReceived += CryptoDumpStrategy_LogReceived;
            }
            else
            {
                MenuItemStart.IsEnabled = true;
                MenuItemStop.IsEnabled = false;
                MenuItemSpotBuyMarket.IsEnabled = false;
                MenuItemSpotSellMarket.IsEnabled = false;
            }
        }

        private void Stop()
        {
            try
            {
                SubscribeToOnMinuteKlineUpdatesThread.Abort();
            }
            catch
            {

            }

            MenuItemStart.IsEnabled = false;
            MenuItemStop.IsEnabled = false;
            MenuItemSpotBuyMarket.IsEnabled = false;
            MenuItemSpotSellMarket.IsEnabled = false;

            if (BinanceSpotClient != null)
            {
                BinanceSpotClient.Stop();

                BinanceSpotClient.BinanceStreamTickReceived -= BinanceSpotClient_BinanceStreamTickReceived;
                BinanceSpotClient.BinanceStreamBalanceUpdateReceived -= BinanceSpotClient_BinanceStreamBalanceUpdateReceived;
            }

            MenuItemStart.IsEnabled = true;
            MenuItemStop.IsEnabled = false;
        }

        private void StartBot()
        {

        }

        private void StopBot()
        {

        }

        private void CryptoDumpStrategy_LogReceived(string log)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(TextBoxLog.Text))
                {
                    TextBoxLog.AppendText(Environment.NewLine);
                }

                TextBoxLog.AppendText(log);

                if (!TextBoxLog.IsFocused)
                {
                    TextBoxLog.ScrollToEnd();
                }
            }));

            File.WriteAllLines("DumpStrategy.txt", new string[] { log });
        }

        private void MenuItemStartTrading_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void MenuItemStopTrading_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void MenuItemStartTradingBot_Click(object sender, RoutedEventArgs e)
        {
   
        }

        private void MenuItemStopTradingBot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemRefreshAssets_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage;

            var tradeResult = BinanceSpotClient.GetAccountInfo(out errorMessage);

            if (tradeResult != null)
            {
                
            }
            else
            {
                MessageBox.Show($"The trade result status is '{errorMessage}'.", "Refresh Assets Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItemSpotBuyMarket_Click(object sender, RoutedEventArgs e)
        {
            var tradeAmountWindow = new TradeAmountWindow();

            var tradeInfo = tradeAmountWindow.ShowDialogWindow();

            if (tradeInfo != null && BinanceSpotClient != null)
            {
                string errorMessage;

                var tradeResult = BinanceSpotClient.OpenMarketOrder(tradeInfo.Item1, tradeInfo.Item2, out errorMessage);

                if (tradeResult != null)
                {
                    MessageBox.Show($"The trade result status is '{tradeResult.Status}'.", "Trade Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"The trade result status is '{errorMessage}'.", "Trade Result", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemSpotSellMarket_Click(object sender, RoutedEventArgs e)
        {
            var tradeAmountWindow = new TradeAmountWindow();

            var tradeInfo = tradeAmountWindow.ShowDialogWindow();

            if (tradeInfo != null && BinanceSpotClient != null)
            {
                string errorMessage;

                var tradeResult = BinanceSpotClient.CloseMarketOrder(tradeInfo.Item1, tradeInfo.Item2, out errorMessage);

                if (tradeResult != null)
                {
                    MessageBox.Show($"The trade result status is '{tradeResult.Status}'.", "Trade Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"The trade result status is '{errorMessage}'.", "Trade Result", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void PropertyGridControlAccountInfo_ShowingEditor(object sender, ShowingPropertyGridEditorEventArgs e)
        {
            e.Cancel = true;
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

        private void WindowMain_Closing(object sender, CancelEventArgs e)
        {
            Stop();
        }

        private void BinanceSpotClient_BinanceStreamTickReceived(SymbolTypes symbol, BinanceStreamTick tick)
        {
            if (symbol == SymbolTypes.BtcUsdt)
            {
                var candle = MarketData.FirstOrDefault(p => p.Symbol == symbol);

                candle.LastMinuteCandle = tick.OpenTime;
                candle.Open = Convert.ToSingle(tick.OpenPrice);
                candle.High = Convert.ToSingle(tick.HighPrice);
                candle.Low = Convert.ToSingle(tick.LowPrice);
                candle.Close = Convert.ToSingle(tick.LastPrice);
                candle.Volume = Convert.ToSingle(tick.BaseVolume);
                candle.QuoteVolume = Convert.ToSingle(tick.QuoteVolume);
                candle.NumberOfTrades = tick.TotalTrades;

                if (CryptoDumpStrategy != null)
                {
                    var line = string.Format("{0},{1}", tick.CloseTime.ToString("yyyy/MM/dd HH:mm:ss"), tick.LastPrice);

                    File.AppendAllLines(@"BtcUsdtBinanceStreamTicks.txt", new[] { line });

                    CryptoDumpStrategy.CheckPrice(tick);
                }
            }
        }

        private void BinanceSpotClient_BinanceStreamBalanceUpdateReceived(BinanceStreamBalanceUpdate binanceStreamBalanceUpdate)
        {

        }
    }
}

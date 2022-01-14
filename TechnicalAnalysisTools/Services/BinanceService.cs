using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using Binance.Net.Objects.Spot.MarketData;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Sockets;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Services
{
    public class BinanceService
    {
        static BinanceService()
        {
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(ServerConstantHelper.BinanceKey, ServerConstantHelper.BinanceSecret)
            });

            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(ServerConstantHelper.BinanceKey, ServerConstantHelper.BinanceSecret)
            });
        }

        public BinanceService()
        {
            //
            Client = new BinanceClient();

            //
            var socketClientOption = new BinanceSocketClientOptions();

            socketClientOption.AutoReconnect = true;
            socketClientOption.ReconnectInterval = new TimeSpan(0, 0, 5);

            SocketClient = new BinanceSocketClient(socketClientOption);

            //
            UpdateSubscriptionThread = new Thread(() =>
            {
                for (; ; )
                {
                    Thread.Sleep(15 * 1000);

                    ConnectedUpdateSubscriptionsSemaphore.WaitOne();

                    try
                    {
                        try
                        {
                            foreach (var symbol in UpdateSubscriptions.Keys)
                            {
                                if (UpdateSubscriptions[symbol] == null)
                                {
                                    Subscribe(symbol);
                                }
                            }
                        }
                        catch
                        {

                        }

                        if (ConnectedUpdateSubscriptions == 0)
                        {
                            if (LastReportedStatus != BinanceConnectionStatusModes.Bad)
                            {
                                BinanceConnectionStatus?.Invoke(BinanceConnectionStatusModes.Bad);

                                LastReportedStatus = BinanceConnectionStatusModes.Bad;
                            }
                        }
                        else if (ConnectedUpdateSubscriptions == UpdateSubscriptions.Keys.Count)
                        {
                            if (LastReportedStatus != BinanceConnectionStatusModes.Good)
                            {
                                BinanceConnectionStatus?.Invoke(BinanceConnectionStatusModes.Good);

                                LastReportedStatus = BinanceConnectionStatusModes.Good;
                            }
                        }
                        else
                        {
                            if (LastReportedStatus != BinanceConnectionStatusModes.NotGood)
                            {
                                BinanceConnectionStatus?.Invoke(BinanceConnectionStatusModes.NotGood);

                                LastReportedStatus = BinanceConnectionStatusModes.NotGood;
                            }
                        }
                    }
                    finally
                    {
                        ConnectedUpdateSubscriptionsSemaphore.Release();
                    }
                }
            });

            UpdateSubscriptionThread.Start();
        }

        private Thread UpdateSubscriptionThread { get; }

        private Thread ChanelConnectionThread { get; set; }

        private Semaphore StartFunctionSemaphore { get; } = new Semaphore(1, 1);

        private Semaphore ConnectedUpdateSubscriptionsSemaphore { get; } = new Semaphore(1, 1);

        private ConcurrentDictionary<SymbolTypes, UpdateSubscription> UpdateSubscriptions { get; } = new ConcurrentDictionary<SymbolTypes, UpdateSubscription>();

        private int ConnectedUpdateSubscriptions { get; set; } = 0;

        private BinanceConnectionStatusModes LastReportedStatus { get; set; } = BinanceConnectionStatusModes.NotGood;

        private BinanceClient Client { get; }

        private BinanceSocketClient SocketClient { get; }

        private Process StartVpnConnectionCheckProcess()
        {
            var startInfo = new ProcessStartInfo(@"c:\Windows\System32\rasdial.exe", string.Format("\"{0}\" {1} {2}", ServerConstantHelper.ChanelConnectionName, ServerConstantHelper.ChanelConnectionUsername, ServerConstantHelper.ChanelConnectionPassword));

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            return Process.Start(startInfo);
        }

        private void Start()
        {
            if (ChanelConnectionThread == null)
            {
                StartFunctionSemaphore.WaitOne();

                try
                {
                    if (ChanelConnectionThread == null)
                    {
                        ChanelConnectionThread = new Thread(() =>
                        {
                            for (; ; )
                            {
                                try
                                {
                                    StartVpnConnectionCheckProcess()?.WaitForExit();

                                    Thread.Sleep(30 * 1000);
                                }
                                catch
                                {

                                }
                            }
                        });

                        ChanelConnectionThread.Start();
                    }
                }
                finally
                {
                    StartFunctionSemaphore.Release();
                }
            }
        }

        private void Subscribe(SymbolTypes symbol)
        {
            var updateSubscription = SocketClient.Spot.SubscribeToKlineUpdates(symbol.ToString().ToUpper(), KlineInterval.OneMinute, data =>
            {
                if (data.Data.Final)
                {
                    var symbols = SymbolTypeHelper.SymbolTypesList;

                    if (symbols.Any(p => p.ToString().ToLower() == data.Symbol.ToLower()))
                    {
                        var kline = new BinanceSpotKline();

                        kline.OpenTime = data.Data.OpenTime;
                        kline.CloseTime = data.Data.CloseTime;
                        kline.Open = data.Data.Open;
                        kline.High = data.Data.High;
                        kline.Low = data.Data.Low;
                        kline.Close = data.Data.Close;
                        kline.BaseVolume = data.Data.BaseVolume;
                        kline.QuoteVolume = data.Data.QuoteVolume;
                        kline.TradeCount = data.Data.TradeCount;
                        kline.TakerBuyBaseVolume = data.Data.TakerBuyBaseVolume;
                        kline.TakerBuyQuoteVolume = data.Data.TakerBuyQuoteVolume;

                        BinanceKlineUpdatesReceived?.Invoke(symbols.First(p => p.ToString().ToLower() == data.Symbol.ToLower()), kline);
                    }
                }
            }).Data;

            if (updateSubscription != null)
            {
                updateSubscription.ConnectionLost += UpdateSubscription_ConnectionLost;
                updateSubscription.ConnectionRestored += UpdateSubscription_ConnectionRestored;

                ConnectedUpdateSubscriptions++;
            }

            UpdateSubscriptions[symbol] = updateSubscription;
        }

        private void UpdateSubscription_ConnectionLost()
        {
            ConnectedUpdateSubscriptionsSemaphore.WaitOne();

            try
            {
                ConnectedUpdateSubscriptions--;
            }
            finally
            {
                ConnectedUpdateSubscriptionsSemaphore.Release();
            }
        }

        private void UpdateSubscription_ConnectionRestored(TimeSpan obj)
        {
            ConnectedUpdateSubscriptionsSemaphore.WaitOne();

            try
            {
                ConnectedUpdateSubscriptions++;
            }
            finally
            {
                ConnectedUpdateSubscriptionsSemaphore.Release();
            }
        }

        public void SubscribeToMinuteUpdates(SymbolTypes symbol)
        {
            ConnectedUpdateSubscriptionsSemaphore.WaitOne();

            try
            {
                Subscribe(symbol);
            }
            finally
            {
                ConnectedUpdateSubscriptionsSemaphore.Release();
            }
        }

        public IList<BinanceSpotKline> ReadMinuteUpdates(SymbolTypes symbol, ref BinanceSpotKline lastBinanceSpotKline)
        {
            //
            Start();

            //
            var result = new List<BinanceSpotKline>();

            var dateTimeNow = DateTime.UtcNow;

            while (lastBinanceSpotKline.CloseTime < dateTimeNow)
            {
                var list = Client.Spot.Market.GetKlines(symbol.ToString().ToUpper(), KlineInterval.OneMinute, lastBinanceSpotKline.CloseTime).Data;

                if (list != null && list.Count() != 0)
                {
                    foreach (var kline in list)
                    {
                        if (kline.CloseTime.Second == 59 && kline.BaseVolume != 0 && kline.OpenTime > lastBinanceSpotKline.CloseTime)
                        {
                            //
                            while ((kline.OpenTime - lastBinanceSpotKline.OpenTime).TotalSeconds > 60)
                            {
                                var lostKline = new BinanceSpotKline();

                                lostKline.OpenTime = lastBinanceSpotKline.OpenTime.AddMinutes(1);
                                lostKline.CloseTime = lastBinanceSpotKline.OpenTime.AddMinutes(1).AddSeconds(59);
                                lostKline.Open = lastBinanceSpotKline.Close;
                                lostKline.High = lastBinanceSpotKline.Close;
                                lostKline.Low = lastBinanceSpotKline.Close;
                                lostKline.Close = lastBinanceSpotKline.Close;
                                lostKline.BaseVolume = 0;
                                lostKline.QuoteVolume = 0;
                                lostKline.TradeCount = 0;
                                lostKline.TakerBuyBaseVolume = 0;
                                lostKline.TakerBuyQuoteVolume = 0;

                                result.Add(lostKline);

                                lastBinanceSpotKline = lostKline;
                            }

                            //
                            lastBinanceSpotKline = (BinanceSpotKline)kline;

                            result.Add(lastBinanceSpotKline);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public void UnsubscribeAll()
        {
            SocketClient.UnsubscribeAll();
        }

        public void Stop()
        {
            try
            {
                UpdateSubscriptionThread?.Abort();
            }
            catch
            {

            }

            try
            {
                ChanelConnectionThread?.Abort();
            }
            catch
            {

            }
        }

        public event BinanceKlineUpdatesReceivedHandler BinanceKlineUpdatesReceived;

        public event BinanceConnectionStatusHandler BinanceConnectionStatus;
    }
}

using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using Binance.Net.Objects.Spot.MarketStream;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Authentication;
using System;
using System.Linq;
using System.Threading;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Trading.Ui.Win.Delegates;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Services
{
    internal class BinanceSpotClientService
    {
        public BinanceSpotClientService(string binanceApiKey, string binanceApiSecret)
        {
            BinanceApiKey = binanceApiKey;
            BinanceApiSecret = binanceApiSecret;
        }

        private string BinanceApiKey { get; }

        private string BinanceApiSecret { get; }

        private BinanceClient Client { get; set; }

        private BinanceSocketClient SocketClient { get; set; }

        private Thread KeepAliveThread { get; set; }

        private string UserDataStreamKey { get; set; }

        public bool Init()
        {
            var result = false;

            if (Client == null && SocketClient == null)
            {
                Client = new BinanceClient(new BinanceClientOptions
                {
                    ApiCredentials = new ApiCredentials(BinanceApiKey, BinanceApiSecret)
                });

                var startResult = Client.Spot.UserStream.StartUserStream();

                if (startResult.Success)
                {
                    UserDataStreamKey = startResult.Data;

                    SocketClient = new BinanceSocketClient(new BinanceSocketClientOptions
                    {
                        ApiCredentials = new ApiCredentials(BinanceApiKey, BinanceApiSecret),
                        AutoReconnect = true,
                        ReconnectInterval = new TimeSpan(0, 0, 5)
                    });

                    var subscribeResponse = SocketClient.Spot.SubscribeToUserDataUpdates(UserDataStreamKey,
                        data =>
                        {

                        },
                        data =>
                        {

                        },
                        data =>
                        {

                        },
                        data =>
                        {
                            BinanceStreamBalanceUpdateReceived?.Invoke(data);
                        });

                    if (subscribeResponse.Success)
                    {
                        result = true;

                        KeepAliveThread = new Thread(() =>
                        {
                            while (true)
                            {
                                Client.Spot.UserStream.KeepAliveUserStream(UserDataStreamKey);

                                Thread.Sleep(15 * 60 * 1000);
                            }
                        });

                        KeepAliveThread.Start();
                    }
                    else
                    {
                        Client = null;
                        SocketClient = null;
                    }
                }
                else
                {
                    Client = null;
                }
            }

            return result;
        }

        public async void Stop()
        {
            try
            {
                KeepAliveThread.Abort();
            }
            catch
            {

            }

            await SocketClient.UnsubscribeAll();
        }

        public bool SubscribeToBinanceMiniTickUpdates(SymbolTypes symbol)
        {
            var result = false;

            var updateSubscription = SocketClient.Spot.SubscribeToSymbolTickerUpdates(symbol.ToString().ToUpper(), data =>
            {
                var symbols = (SymbolTypes[])Enum.GetValues(typeof(SymbolTypes));

                if (symbols.Any(p => p.ToString().ToLower() == data.Symbol.ToLower()))
                {
                    BinanceStreamTickReceived?.Invoke(symbols.First(p => p.ToString().ToLower() == data.Symbol.ToLower()), (BinanceStreamTick)data);
                }
            });

            if (updateSubscription.Success)
            {
                result = true;
            }

            return result;
        }

        public BinanceAccountInfo GetAccountInfo(out string errorMessage)
        {
            BinanceAccountInfo result = null;

            errorMessage = "";

            var webCallResult = Client.General.GetAccountInfo();

            if (webCallResult.Success)
            {
                result = webCallResult.Data;
            }
            else
            {
                errorMessage = webCallResult.Error.Message;
            }

            return result;
        }

        public BinancePlacedOrder OpenMarketOrder(SymbolTypes symbol, decimal amount, out string errorMessage)
        {
            BinancePlacedOrder result = null;

            errorMessage = "";

            var webCallResult = Client.Spot.Order.PlaceOrder(symbol.ToString().ToUpper(), OrderSide.Buy, OrderType.Market, amount);

            if (webCallResult.Success)
            {
                result = webCallResult.Data;
            }
            else
            {
                errorMessage = webCallResult.Error.Message;
            }

            return result;
        }

        public BinancePlacedOrder CloseMarketOrder(SymbolTypes symbol, decimal amount, out string errorMessage)
        {
            BinancePlacedOrder result = null;

            errorMessage = "";

            var webCallResult = Client.Spot.Order.PlaceOrder(symbol.ToString().ToUpper(), OrderSide.Sell, OrderType.Market, amount);

            if (webCallResult.Success)
            {
                result = webCallResult.Data;
            }
            else
            {
                errorMessage = webCallResult.Error.Message;
            }

            return result;
        }

        public BinancePlacedOrder CloseMarketOrder(BinancePlacedOrder binancePlacedOrder, out string errorMessage)
        {
            return CloseMarketOrder(((SymbolTypes[])Enum.GetValues(typeof(SymbolTypes))).First(p => p.ToString().ToString() == binancePlacedOrder.Symbol.ToLower()), binancePlacedOrder.QuantityFilled, out errorMessage);
        }

        public event BinanceStreamTickReceivedHandler BinanceStreamTickReceived;

        public event BinanceStreamBalanceUpdateReceivedHandler BinanceStreamBalanceUpdateReceived;
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TechnicalAnalysisTools.Shared.DataObjects;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;
using TechnicalAnalysisTools.Ui.Win.DataModels;
using TechnicalAnalysisTools.Ui.Win.Delegates;

namespace TechnicalAnalysisTools.Ui.Win.Services
{
    internal class CryptoClientService
    {
        public CryptoClientService(SessionEstablishmentDataModel sessionEstablishment)
        {
            SessionEstablishment = sessionEstablishment;
        }

        private SessionEstablishmentDataModel SessionEstablishment { get; }

        private NetworkClientService NetworkClient { get; set; }

        private Thread SocketAvailabilityThread { get; set; }

        private ConcurrentDictionary<Guid, ManualResetEvent> CommandResponseLocks { get; } = new ConcurrentDictionary<Guid, ManualResetEvent>();

        private ConcurrentDictionary<Guid, CommandDataObject> CommandResponses { get; } = new ConcurrentDictionary<Guid, CommandDataObject>();

        private void NetworkClient_Connected(object sender, EventArgs e)
        {
            Connected?.Invoke(sender, e);
        }

        private void NetworkClient_CommandDataReceived(CommandDataObject receivedCommand)
        {
            if (SessionEstablishment.IsAuthenticated == false)
            {
                NetworkClient.Stop();
            }
            else
            {
                switch (receivedCommand.Command)
                {
                    case CommandTypes.ClientInitializedData:
                    case CommandTypes.MenuItemChanged:
                    case CommandTypes.ServerStatusPropertyChanged:
                    case CommandTypes.MarketData:
                    case CommandTypes.Alarms:
                    case CommandTypes.AlarmsHistory:
                    case CommandTypes.SeenAlarmResponse:
                    case CommandTypes.TestStrategyStatus:
                        {
                            CommandDataReceived?.Invoke(receivedCommand);
                        }
                        break;
                    case CommandTypes.RunAlarmsResponse:
                    case CommandTypes.RunTemplateAlarmResponse:
                    case CommandTypes.ReadAlarmScriptResponse:
                    case CommandTypes.EvaluateAlarmResponse:
                    case CommandTypes.EnableDisableAlarmResponse:
                    case CommandTypes.DeleteAlarmResponse:
                    case CommandTypes.LiveHistoryResponse:
                    case CommandTypes.TestNewStrategyResponse:
                        {
                            if (CommandResponseLocks.ContainsKey(receivedCommand.CommandId))
                            {
                                ManualResetEvent responselock;

                                while (!CommandResponseLocks.TryRemove(receivedCommand.CommandId, out responselock))
                                {
                                    Thread.Sleep(0);
                                }

                                while (!CommandResponses.TryAdd(receivedCommand.CommandId, receivedCommand))
                                {
                                    Thread.Sleep(0);
                                }

                                responselock.Set();
                            }
                            else
                            {
                                CommandDataReceived?.Invoke(receivedCommand);
                            }
                        }
                        break;
                }
            }
        }

        private void NetworkClient_Closed(object sender, EventArgs e)
        {
            //
            try
            {
                SocketAvailabilityThread?.Abort();

                SocketAvailabilityThread = null;
            }
            catch
            {

            }

            //
            CommandResponses.Clear();

            var responseLocks = CommandResponseLocks.Values;

            CommandResponseLocks.Clear();

            foreach (var responseLock in responseLocks)
            {
                responseLock.Set();
            }

            Closed?.Invoke(this, e);
        }

        public async Task<bool> Start()
        {
            NetworkClient = new NetworkClientService(SessionEstablishment);

            NetworkClient.Connected += NetworkClient_Connected;
            NetworkClient.CommandDataReceived += NetworkClient_CommandDataReceived;
            NetworkClient.Closed += NetworkClient_Closed;

            var result = await NetworkClient.Start();

            if (result)
            {
                try
                {
                    SocketAvailabilityThread = new Thread(async () =>
                    {
                        var networkClient = NetworkClient;

                        var sendData = CommandHelper.ImAlive();

                        for (; ; )
                        {
                            try
                            {
                                await networkClient.Send(sendData);
                            }
                            catch
                            {
                                networkClient.Stop();

                                break;
                            }

                            Thread.Sleep(5000);
                        }
                    });

                    SocketAvailabilityThread.Start();
                }
                catch
                {
                    NetworkClient.Stop();
                }
            }
            else
            {
                NetworkClient = null;
            }

            return result;
        }

        public async Task<bool> Send(CommandDataObject command, bool hasResponse, CommandDataObject response)
        {
            var result = false;

            if (!hasResponse || response != null)
            {
                ManualResetEvent responselock = null;

                switch (command.Command)
                {
                    case CommandTypes.StartTechnicalAnalysis:
                    case CommandTypes.StopTechnicalAnalysis:
                    case CommandTypes.LiveHistory:
                    case CommandTypes.RunAlarms:
                    case CommandTypes.RunTemplateAlarm:
                    case CommandTypes.ReadAlarmScript:
                    case CommandTypes.EvaluateAlarm:
                    case CommandTypes.SeenAlarm:
                    case CommandTypes.SeenAllAlarm:
                    case CommandTypes.EnableDisableAlarm:
                    case CommandTypes.DeleteAlarm:
                    case CommandTypes.TestNewStrategy:
                    case CommandTypes.TestStrategyStop:
                        {
                            if (hasResponse)
                            {
                                responselock = new ManualResetEvent(false);

                                while (!CommandResponseLocks.TryAdd(command.CommandId, responselock))
                                {
                                    Thread.Sleep(0);
                                }
                            }

                            result = await NetworkClient.Send(CommandHelper.Serialize(command));
                        }
                        break;
                }

                if (result)
                {
                    if (hasResponse)
                    {
                        await Task.Run(() => { responselock.WaitOne(); });

                        if (CommandResponses.ContainsKey(command.CommandId))
                        {
                            CommandDataObject responseCommandDataObject;

                            while (!CommandResponses.TryRemove(command.CommandId, out responseCommandDataObject))
                            {
                                Thread.Sleep(0);
                            }

                            if (responseCommandDataObject != null)
                            {
                                response.CommandId = responseCommandDataObject.CommandId;
                                response.Command = responseCommandDataObject.Command;
                                response.Parameter = responseCommandDataObject.Parameter;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        public void Stop()
        {
            try
            {
                SocketAvailabilityThread?.Abort();

                SocketAvailabilityThread = null;
            }
            catch
            {

            }

            NetworkClient?.Stop();
        }

        public event EventHandler Connected;

        public event CommandDataReceivedHandler CommandDataReceived;

        public event EventHandler Closed;
    }
}

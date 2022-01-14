using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.SocketBase;
using TechnicalAnalysisTools.Auxiliaries;
using TechnicalAnalysisTools.Backends;
using TechnicalAnalysisTools.DataModels;
using TechnicalAnalysisTools.DataObjects;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Helpers;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.DataObjects;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Services
{
    public class CryptoServerService
    {
        public CryptoServerService(SessionEstablishmentDataModel sessionEstablishment)
        {
            SessionEstablishment = sessionEstablishment;
        }

        private SessionEstablishmentDataModel SessionEstablishment { get; }

        private NetworkServerService NetworkServer { get; set; }

        private SemaphoreSlim ClientsSemaphore { get; } = new SemaphoreSlim(1, 1);

        private SemaphoreSlim RequestReceivedSemaphore { get; } = new SemaphoreSlim(1, 1);

        private SemaphoreSlim DdosNewRequestLimiterLock { get; } = new SemaphoreSlim(1, 1);

        private DdosLimiterAuxiliary DdosNewRequestLimiter { get; } = new DdosLimiterAuxiliary(4, 1);

        private Dictionary<string, UiClientSessionDataObject> _UiClients { get; } = new Dictionary<string, UiClientSessionDataObject>();

        private MainBackend Backend { get; set; }

        public ReaderWriterLock InitializeNewSessionLock { get; } = new ReaderWriterLock();

        public SymbolTypes[] SupportedSymbols
        {
            get
            {
                var symbols = Backend.SupportedSymbols;

                if (symbols == null)
                {
                    return new SymbolTypes[0];
                }
                else
                {
                    return symbols.ToArray();
                }
            }
        }

        private void NetworkServer_LogReceived(LogDataObject log)
        {
            LogReceived?.Invoke(log);
        }

        private void NetworkServer_NewSessionConnected(UiClientSessionDataObject session)
        {
            //
            ClientsSemaphore.Wait();

            try
            {
                _UiClients.Add(session.Username, session);
            }
            catch
            {
                var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                logMessage += Environment.NewLine;

                if (!string.IsNullOrWhiteSpace(session.Username))
                {
                    logMessage += session.Username + ", ";
                }

                logMessage += "Ui client is already connected!";

                NetworkServer.Logger.Warn(logMessage);

                session.Close();
            }
            finally
            {
                ClientsSemaphore.Release();
            }

            //
            InitializeNewSessionConnected(session);
        }

        private void NetworkServer_SessionClosed(UiClientSessionDataObject session, CloseReason value)
        {
            ClientsSemaphore.Wait();

            try
            {
                if (!string.IsNullOrWhiteSpace(session.Username) && _UiClients.ContainsKey(session.Username))
                {
                    _UiClients.Remove(session.Username);
                }
            }
            finally
            {
                ClientsSemaphore.Release();
            }
        }

        private void ExchangeNetwork(UiClientSessionDataObject session, CommandDataObject command)
        {
            if (!session.IsEncryptionAdjusted)
            {
                var logRequiredMessage = "";
                var sessionNeedClose = false;

                if (command.Parameter != null && command.Parameter is byte[])
                {
                    var sessionKey = (byte[])command.Parameter;

                    if (sessionKey.Length != 0)
                    {
                        var networkEncryption = new NetworkEncryptionAuxiliary();

                        if (networkEncryption.InitializeBySessionKey(sessionKey))
                        {
                            session.InitializeNetworkEncryption(networkEncryption);
                        }
                        else
                        {
                            logRequiredMessage = "Session key of new command was corrupted!";
                            sessionNeedClose = true;
                        }
                    }
                    else
                    {
                        logRequiredMessage = "Session key of new command was corrupted!";
                        sessionNeedClose = true;
                    }
                }
                else
                {
                    logRequiredMessage = "Session key of new command was corrupted!";
                    sessionNeedClose = true;
                }

                if (!string.IsNullOrWhiteSpace(logRequiredMessage))
                {
                    var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                    logMessage += Environment.NewLine;

                    logMessage += logRequiredMessage;

                    NetworkServer.Logger.Warn(logMessage);
                }

                if (sessionNeedClose)
                {
                    session.Close();
                }
            }
            else
            {
                var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                logMessage += Environment.NewLine;

                if (!string.IsNullOrWhiteSpace(session.Username))
                {
                    logMessage += session.Username + ", ";
                }

                logMessage += "Session key command was received for already encryption adjusted session!";

                NetworkServer.Logger.Warn(logMessage);

                session.Close();
            }
        }

        private async void AuthenticateClient(UiClientSessionDataObject session, CommandDataObject command)
        {
            if (!session.IsAuthenticated)
            {
                if (session.IsEncryptionAdjusted)
                {
                    bool? duplicated = null;

                    string receivedLoginUsername = "";
                    string receivedFileVersion = "";

                    if (command.Parameter is string[])
                    {
                        var loginInfo = command.Parameter as string[];

                        if (loginInfo.Length == 4)
                        {
                            receivedLoginUsername = loginInfo[0];
                            receivedFileVersion = loginInfo[3];

                            ClientsSemaphore.Wait();

                            UiClientSessionDataObject oldSession = null;

                            try
                            {
                                duplicated = _UiClients.ContainsKey(loginInfo[0]);

                                if (duplicated.Value)
                                {
                                    //
                                    oldSession = _UiClients[loginInfo[0]];

                                    _UiClients.Remove(loginInfo[0]);

                                    //
                                    var duplicateMessage = string.Format("Session: {0}/{1}:{2}", oldSession.Id, oldSession.RemoteEndPoint.Address.ToString(), oldSession.RemoteEndPoint.Port);

                                    duplicateMessage += Environment.NewLine;

                                    if (!string.IsNullOrWhiteSpace(oldSession.Username))
                                    {
                                        duplicateMessage += oldSession.Username + ", ";
                                    }

                                    duplicateMessage += "Ui client is already connected. Old session was closed!";

                                    NetworkServer.Logger.Warn(duplicateMessage);

                                    //
                                    oldSession.Username = "";

                                    duplicated = false;
                                }
                            }
                            finally
                            {
                                ClientsSemaphore.Release();
                            }

                            if (oldSession != null)
                            {
                                try
                                {
                                    oldSession.Close();

                                    await Task.Run(() => Thread.Sleep(2000));
                                }
                                catch
                                {

                                }
                            }

                            if (!duplicated.Value)
                            {
                                foreach (var client in SessionEstablishment.Clients)
                                {
                                    if (client.IsEnabled && client.Username == loginInfo[0] && client.Password == loginInfo[1])
                                    {
                                        //
                                        bool isUniqueValueValid = false;

                                        var userDataFile = Path.Combine(ServerAddressHelper.UserDataFolder, client.Username + ".bin");

                                        if (File.Exists(userDataFile))
                                        {
                                            var uniqueValue = File.ReadAllText(userDataFile);

                                            if (loginInfo[2] == uniqueValue)
                                            {
                                                isUniqueValueValid = true;
                                            }
                                        }
                                        else
                                        {
                                            isUniqueValueValid = true;

                                            File.WriteAllText(userDataFile, loginInfo[2]);
                                        }

                                        //
                                        if (isUniqueValueValid || client.ClientType == UiClientTypes.Admin)
                                        {
                                            //
                                            session.Username = loginInfo[0];

                                            session.ClientType = client.ClientType;

                                            session.IsAuthenticated = true;

                                            session.IsClientEnabled = true;

                                            //
                                            var authenticate = CommandHelper.SuccessfulAuthenticate(client.ClientType);

                                            SendDataToClient(session, authenticate);
                                        }
                                        else
                                        {
                                            var message = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                                            message += Environment.NewLine;

                                            if (!string.IsNullOrWhiteSpace(loginInfo[0]))
                                            {
                                                message += loginInfo[0] + ", ";
                                            }

                                            message += $"Unique value of user '{client.Username}' in authentication was incorrect!";

                                            NetworkServer.Logger.Warn(message);
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                    logMessage += Environment.NewLine;

                    if (session.IsAuthenticated)
                    {
                        if (!string.IsNullOrWhiteSpace(session.Username))
                        {
                            logMessage += session.Username + ", ";
                        }

                        logMessage += $"Authentication by file version '{receivedFileVersion}' was successful!";

                        NetworkServer.Logger.Info(logMessage);

                        NetworkServer_NewSessionConnected(session);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(receivedLoginUsername))
                        {
                            logMessage += receivedLoginUsername + ", ";
                        }

                        if (duplicated.HasValue && duplicated.Value)
                        {
                            logMessage += "Ui client is already connected!";
                        }
                        else
                        {
                            logMessage += "Authentication was failed!";
                        }

                        NetworkServer.Logger.Warn(logMessage);

                        session.Close();
                    }
                }
                else
                {
                    var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                    logMessage += Environment.NewLine;

                    logMessage += "Session was trying to authenticating before encryption adjusted!";

                    NetworkServer.Logger.Warn(logMessage);

                    session.Close();
                }
            }
            else
            {
                var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                logMessage += Environment.NewLine;

                if (!string.IsNullOrWhiteSpace(session.Username))
                {
                    logMessage += session.Username + ", ";
                }

                logMessage += "Session was already authenticated!";

                NetworkServer.Logger.Warn(logMessage);

                session.Close();
            }
        }

        private void NetworkServer_NewRequestReceived(UiClientSessionDataObject session, RequestDataObject requestInfo)
        {
            RequestReceivedSemaphore.Wait();

            try
            {
                var decryptedResult = false;

                RequestDataObject modifiedRequestInfo = null;

                if (session.IsEncryptionAdjusted)
                {
                    if (requestInfo.Body != null && requestInfo.Body.Length != 0)
                    {
                        var decryptedData = session.Decrypt(requestInfo.Body);

                        if (decryptedData != null && requestInfo.Body.Length != 0)
                        {
                            modifiedRequestInfo = new RequestDataObject(decryptedData);

                            modifiedRequestInfo.Key = requestInfo.Key;

                            decryptedResult = true;
                        }
                    }
                }
                else
                {
                    decryptedResult = true;

                    modifiedRequestInfo = requestInfo;
                }

                if (decryptedResult)
                {
                    if (session.IsReady)
                    {
                        if (session.IsClientEnabled)
                        {
                            HandleNewRequestReceived(session, modifiedRequestInfo);
                        }
                        else
                        {
                            session.Close();
                        }
                    }
                    else
                    {
                        lock (session)
                        {
                            if (session.IsReady)
                            {
                                if (session.IsClientEnabled)
                                {
                                    HandleNewRequestReceived(session, modifiedRequestInfo);
                                }
                                else
                                {
                                    session.Close();
                                }
                            }
                            else
                            {
                                if (session.Connected)
                                {
                                    var command = CommandHelper.Deserialize(modifiedRequestInfo.Body);

                                    if (command.Command == CommandTypes.SessionKey)
                                    {
                                        ExchangeNetwork(session, command);
                                    }
                                    else if (command.Command == CommandTypes.Authenticate)
                                    {
                                        AuthenticateClient(session, command);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    session.Close();
                }
            }
            finally
            {
                RequestReceivedSemaphore.Release();
            }
        }

        private void InitializeNewSessionConnected(UiClientSessionDataObject session)
        {
            InitializeNewSessionLock.AcquireWriterLock(int.MaxValue);

            try
            {
                var initializedData = Backend.GetClientInitializedDataByUsername(session.Username, session.ClientType);

                SendDataToClient(session, new CommandDataObject { Command = CommandTypes.ClientInitializedData, Parameter = initializedData });
            }
            finally
            {
                InitializeNewSessionLock.ReleaseWriterLock();
            }
        }

        private void HandleNewRequestReceived(UiClientSessionDataObject session, RequestDataObject requestInfo)
        {
            InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

            List<UiClientSessionDataObject> broadcastSessions;

            ClientsSemaphore.Wait();

            try
            {
                broadcastSessions = _UiClients.Values.Where(p => p.ClientType == UiClientTypes.Admin).ToList();
            }
            finally
            {
                ClientsSemaphore.Release();
            }

            if (!broadcastSessions.Contains(session))
            {
                broadcastSessions.Add(session);
            }

            try
            {
                CommandDataObject command = null;

                try
                {
                    command = CommandHelper.Deserialize(requestInfo.Body);
                }
                catch
                {
                    command = null;
                }

                if (command != null)
                {
                    var isPermitted = true;

                    if (command.Command != CommandTypes.ImAlive)
                    {
                        try
                        {
                            DdosNewRequestLimiterLock.Wait();

                            isPermitted = DdosNewRequestLimiter.IsPermitted(session.Username);
                        }
                        finally
                        {
                            DdosNewRequestLimiterLock.Release();
                        }
                    }

                    if (isPermitted)
                    {
                        if (Backend.HasPermission(session.ClientType, command.Command))
                        {
                            switch (command.Command)
                            {
                                case CommandTypes.StartTechnicalAnalysis:
                                    Backend.Start();
                                    break;
                                case CommandTypes.StopTechnicalAnalysis:
                                    //Backend.Stop();
                                    break;
                                case CommandTypes.RunAlarms:
                                    {
                                        var result = false;

                                        if (command.Parameter is object[])
                                        {
                                            var parameters = command.Parameter as object[];

                                            if (parameters.Length == 2 && (parameters[0] is string) && (parameters[1] is string))
                                            {
                                                var script = parameters[0] as string;
                                                var filename = parameters[1] as string;

                                                if (!string.IsNullOrWhiteSpace(script) && !string.IsNullOrWhiteSpace(filename))
                                                {
                                                    result = Backend.RunAlarm(script, filename, session.Username);
                                                }
                                            }
                                        }

                                        SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.RunAlarmsResponse, Parameter = result });

                                        ApplyMarketDataChangedToUsers(broadcastSessions.ToArray());
                                    }
                                    break;
                                case CommandTypes.RunTemplateAlarm:
                                    {
                                        var result = false;

                                        if (command.Parameter is object[])
                                        {
                                            var parameters = command.Parameter as object[];

                                            if (parameters.Length == 3 && (parameters[0] is string) && (parameters[1] is SymbolTypes[]) && (parameters[2] is string))
                                            {
                                                var script = parameters[0] as string;
                                                var symbols = parameters[1] as SymbolTypes[];
                                                var filename = parameters[2] as string;

                                                if (!string.IsNullOrWhiteSpace(script) && symbols.Length != 0)
                                                {
                                                    result = Backend.RunTemplate(script, symbols, filename, session.Username);
                                                }
                                            }
                                        }

                                        SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.RunTemplateAlarmResponse, Parameter = result });

                                        ApplyMarketDataChangedToUsers(broadcastSessions.ToArray());
                                    }
                                    break;
                                case CommandTypes.ReadAlarmScript:
                                    {
                                        var result = "";

                                        if (command.Parameter is Guid)
                                        {
                                            var id = (Guid)command.Parameter;

                                            result = Backend.ReadAlarmScriptByUsername(session.Username, session.ClientType, id);
                                        }

                                        SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.ReadAlarmScriptResponse, Parameter = result });
                                    }
                                    break;
                                case CommandTypes.EvaluateAlarm:
                                    {
                                        AlarmItemDataModel result = null;

                                        if (command.Parameter is object[])
                                        {
                                            var parameters = command.Parameter as object[];

                                            if (parameters.Length == 2 && (parameters[0] is Guid) && (parameters[1] is DateTime))
                                            {
                                                var id = (Guid)parameters[0];
                                                var datetime = (DateTime)parameters[1];

                                                result = Backend.EvaluateAlarmByUsername(session.Username, session.ClientType, id, datetime);
                                            }
                                        }

                                        SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.EvaluateAlarmResponse, Parameter = result });
                                    }
                                    break;
                                case CommandTypes.SeenAlarm:
                                    {
                                        var result = false;

                                        if (command.Parameter is Guid)
                                        {
                                            var id = (Guid)command.Parameter;

                                            var relatedUsername = Backend.GetAlarmRelatedUsername(id);

                                            if (Backend.SeenAlarmByUsername(session.Username, session.ClientType, id))
                                            {
                                                result = true;

                                                if (relatedUsername != "")
                                                {
                                                    UiClientSessionDataObject relatedSession;

                                                    ClientsSemaphore.Wait();

                                                    try
                                                    {
                                                        relatedSession = _UiClients.Values.FirstOrDefault(p => p.Username == relatedUsername);
                                                    }
                                                    finally
                                                    {
                                                        ClientsSemaphore.Release();
                                                    }

                                                    if (relatedSession != null && !broadcastSessions.Contains(relatedSession))
                                                    {
                                                        broadcastSessions.Add(relatedSession);
                                                    }
                                                }

                                                SendDataToClients(new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.SeenAlarmResponse, Parameter = id }, broadcastSessions);
                                            }
                                        }

                                        if (!result)
                                        {
                                            SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.SeenAlarmResponse, Parameter = null });
                                        }
                                    }
                                    break;
                                case CommandTypes.SeenAllAlarm:
                                    {
                                        foreach (var id in Backend.SeenAllAlarmByUsername(session.Username, session.ClientType))
                                        {
                                            var broadcastRelatedSessions = broadcastSessions;

                                            var relatedUsername = Backend.GetAlarmRelatedUsername(id);

                                            if (relatedUsername != "")
                                            {
                                                UiClientSessionDataObject relatedSession;

                                                ClientsSemaphore.Wait();

                                                try
                                                {
                                                    relatedSession = _UiClients.Values.FirstOrDefault(p => p.Username == relatedUsername);
                                                }
                                                finally
                                                {
                                                    ClientsSemaphore.Release();
                                                }

                                                if (relatedSession != null && !broadcastRelatedSessions.Contains(relatedSession))
                                                {
                                                    broadcastRelatedSessions = broadcastSessions.ToList();

                                                    broadcastRelatedSessions.Add(relatedSession);
                                                }
                                            }

                                            SendDataToClients(new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.SeenAlarmResponse, Parameter = id }, broadcastRelatedSessions);
                                        }
                                    }
                                    break;
                                case CommandTypes.EnableDisableAlarm:
                                    {
                                        var result = false;

                                        if (command.Parameter is Guid)
                                        {
                                            var id = (Guid)command.Parameter;

                                            var relatedUsername = Backend.GetAlarmRelatedUsername(id);

                                            if (Backend.EnableDisableAlarmByUsername(session.Username, session.ClientType, id))
                                            {
                                                result = true;

                                                if (relatedUsername != "")
                                                {
                                                    UiClientSessionDataObject relatedSession;

                                                    ClientsSemaphore.Wait();

                                                    try
                                                    {
                                                        relatedSession = _UiClients.Values.FirstOrDefault(p => p.Username == relatedUsername);
                                                    }
                                                    finally
                                                    {
                                                        ClientsSemaphore.Release();
                                                    }

                                                    if (relatedSession != null && !broadcastSessions.Contains(relatedSession))
                                                    {
                                                        broadcastSessions.Add(relatedSession);
                                                    }
                                                }

                                                SendDataToClients(new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.EnableDisableAlarmResponse, Parameter = id }, broadcastSessions);
                                            }
                                        }

                                        if (!result)
                                        {
                                            SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.EnableDisableAlarmResponse, Parameter = null });
                                        }
                                    }
                                    break;
                                case CommandTypes.DeleteAlarm:
                                    {
                                        var result = false;

                                        if (command.Parameter is Guid)
                                        {
                                            var id = (Guid)command.Parameter;

                                            var relatedUsername = Backend.GetAlarmRelatedUsername(id);

                                            if (Backend.DeleteAlarmByUsername(session.Username, session.ClientType, id))
                                            {
                                                result = true;

                                                if (relatedUsername != "")
                                                {
                                                    UiClientSessionDataObject relatedSession;

                                                    ClientsSemaphore.Wait();

                                                    try
                                                    {
                                                        relatedSession = _UiClients.Values.FirstOrDefault(p => p.Username == relatedUsername);
                                                    }
                                                    finally
                                                    {
                                                        ClientsSemaphore.Release();
                                                    }

                                                    if (relatedSession != null && !broadcastSessions.Contains(relatedSession))
                                                    {
                                                        broadcastSessions.Add(relatedSession);
                                                    }
                                                }

                                                SendDataToClients(new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.DeleteAlarmResponse, Parameter = id }, broadcastSessions);
                                            }
                                        }

                                        if (!result)
                                        {
                                            SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.DeleteAlarmResponse, Parameter = null });
                                        }
                                    }
                                    break;
                                case CommandTypes.LiveHistory:
                                    {
                                        List<SymbolDataModel> result = null;

                                        if (command.Parameter is object[])
                                        {
                                            var parameters = command.Parameter as object[];

                                            if (parameters.Length == 2 && (parameters[0] is SymbolTypes[]))
                                            {
                                                var symbols = (SymbolTypes[])parameters[0];

                                                if (symbols.Length != 0)
                                                {
                                                    var datetimeParameter = parameters[1];

                                                    if (datetimeParameter == null)
                                                    {
                                                        result = Backend.GetLastLiveHistory(session.Username, symbols);
                                                    }
                                                    else if (datetimeParameter is DateTime)
                                                    {
                                                        var datetime = (DateTime)datetimeParameter;

                                                        if (datetime == DateTime.MinValue)
                                                        {
                                                            result = Backend.GetFirstLiveHistory(session.Username, symbols);
                                                        }
                                                        else if (datetime == DateTime.MaxValue)
                                                        {
                                                            result = Backend.GetLastLiveHistory(session.Username, symbols);
                                                        }
                                                        else
                                                        {
                                                            result = Backend.GetLiveHistory(session.Username, symbols, datetime);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.LiveHistoryResponse, Parameter = result });
                                    }
                                    break;
                                case CommandTypes.TestNewStrategy:
                                    {
                                        if (command.Parameter is object[])
                                        {
                                            StrategyTestService strategyTestService = null;

                                            var validation = false;

                                            var parameters = command.Parameter as object[];

                                            if (parameters.Length == 4 && parameters[0] != null && parameters[0] is StrategyTestDataModel)
                                            {
                                                var strategyTestData = parameters[0] as StrategyTestDataModel;

                                                if (strategyTestData.Enter != null && strategyTestData.ExitTakeProfit != null && strategyTestData.ExitStopLoss != null)
                                                {
                                                    //
                                                    validation = true;

                                                    if (!string.IsNullOrWhiteSpace(strategyTestData.Enter.Alarm))
                                                    {
                                                        if (parameters[1] == null || !(parameters[1] is byte[]) || ((byte[])parameters[1]).Length <= 0)
                                                        {
                                                            validation = false;
                                                        }
                                                    }

                                                    if (validation)
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(strategyTestData.ExitTakeProfit.Alarm))
                                                        {
                                                            if (parameters[2] == null || !(parameters[2] is byte[]) || ((byte[])parameters[2]).Length <= 0)
                                                            {
                                                                validation = false;
                                                            }
                                                        }
                                                    }

                                                    if (validation)
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(strategyTestData.ExitStopLoss.Alarm))
                                                        {
                                                            if (parameters[3] == null || !(parameters[3] is byte[]) || ((byte[])parameters[3]).Length <= 0)
                                                            {
                                                                validation = false;
                                                            }
                                                        }
                                                    }

                                                    //
                                                    if (validation)
                                                    {
                                                        strategyTestService = Backend.InitializeTestNewStrategy(session.Id, session.Username, strategyTestData, parameters[1], parameters[2], parameters[3]);

                                                        validation = strategyTestService != null;
                                                    }
                                                }
                                            }

                                            SendDataToClient(session, new CommandDataObject() { CommandId = command.CommandId, Command = CommandTypes.TestNewStrategyResponse, Parameter = validation });

                                            if (validation)
                                            {
                                                Backend.TestNewStrategy(strategyTestService);
                                            }
                                        }
                                    }
                                    break;
                                case CommandTypes.TestStrategyStop:
                                    {
                                        if (command.Parameter is Guid)
                                        {
                                            var id = (Guid)command.Parameter;

                                            Backend.StopStrategyTest(id);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                        logMessage += Environment.NewLine;

                        if (!string.IsNullOrWhiteSpace(session.Username))
                        {
                            logMessage += session.Username + ", ";
                        }

                        logMessage += string.Format("The request limiter close the session by command '{0}'!", command.Command);

                        NetworkServer.Logger.Warn(logMessage);

                        session.Close();
                    }
                }
            }
            finally
            {
                InitializeNewSessionLock.ReleaseReaderLock();
            }
        }

        private void Backend_ServerStatusPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

            try
            {
                SendDataToClients(CommandHelper.ServerStatusPropertyChanged(e.PropertyName, ReflectionHelper.GetPropertyValue((ServerStatusDataModel)sender, e.PropertyName)), null);
            }
            finally
            {
                InitializeNewSessionLock.ReleaseReaderLock();
            }
        }

        private void ApplyMarketDataChangedToUsers(UiClientSessionDataObject[] uiClientSessions)
        {
            InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

            try
            {
                foreach (var session in uiClientSessions)
                {
                    SendDataToClient(session, new CommandDataObject { Command = CommandTypes.MarketData, Parameter = Backend.GetMarketDataByUsername(session.Username, session.ClientType) });
                }
            }
            finally
            {
                InitializeNewSessionLock.ReleaseReaderLock();
            }
        }

        private void Backend_MarketDataChanged()
        {
            UiClientSessionDataObject[] clientSessions;

            ClientsSemaphore.Wait();

            try
            {
                clientSessions = _UiClients.Values.ToArray();
            }
            finally
            {
                ClientsSemaphore.Release();
            }

            ApplyMarketDataChangedToUsers(clientSessions);
        }

        private void Backend_NewAlarmsReceived(List<SymbolAlarmDataModel> alarms)
        {
            InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

            try
            {
                UiClientSessionDataObject[] clientSessions;

                ClientsSemaphore.Wait();

                try
                {
                    clientSessions = _UiClients.Values.ToArray();
                }
                finally
                {
                    ClientsSemaphore.Release();
                }

                foreach (var session in clientSessions)
                {
                    if (session.ClientType == UiClientTypes.Admin)
                    {
                        SendDataToClient(session, new CommandDataObject { Command = CommandTypes.Alarms, Parameter = alarms });
                    }
                    else
                    {
                        SendDataToClient(session, new CommandDataObject { Command = CommandTypes.Alarms, Parameter = alarms.Where(p => p.Username == session.Username).ToList() });
                    }
                }
            }
            finally
            {
                InitializeNewSessionLock.ReleaseReaderLock();
            }
        }

        private void Backend_NewAlarmsHistoryReceived(List<SymbolAlarmDataModel> alarms)
        {
            InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

            try
            {
                UiClientSessionDataObject[] clientSessions;

                ClientsSemaphore.Wait();

                try
                {
                    clientSessions = _UiClients.Values.ToArray();
                }
                finally
                {
                    ClientsSemaphore.Release();
                }

                foreach (var session in clientSessions)
                {
                    if (session.ClientType == UiClientTypes.Admin)
                    {
                        SendDataToClient(session, new CommandDataObject { Command = CommandTypes.AlarmsHistory, Parameter = alarms });
                    }
                    else
                    {
                        SendDataToClient(session, new CommandDataObject { Command = CommandTypes.AlarmsHistory, Parameter = alarms.Where(p => p.Username == session.Username).ToList() });
                    }
                }
            }
            finally
            {
                InitializeNewSessionLock.ReleaseReaderLock();
            }
        }

        private void Backend_MenuItemsStatusChanged(CommandTypes command)
        {
            InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

            try
            {
                UiClientSessionDataObject[] clientSessions;

                ClientsSemaphore.Wait();

                try
                {
                    clientSessions = _UiClients.Values.ToArray();
                }
                finally
                {
                    ClientsSemaphore.Release();
                }

                foreach (var clientSession in clientSessions)
                {
                    SendDataToClient(clientSession, CommandHelper.MenuItemChanged(command, Backend.GetMenuItemsStatus(clientSession.ClientType, command)));
                }
            }
            finally
            {
                InitializeNewSessionLock.ReleaseReaderLock();
            }
        }

        private bool Backend_StrategyTestStatusChanged(Guid sessionId, StrategyTestStatusDataModel strategyTestStatus)
        {
            var result = false;

            InitializeNewSessionLock.AcquireReaderLock(int.MaxValue);

            try
            {
                UiClientSessionDataObject clientSession;

                ClientsSemaphore.Wait();

                try
                {
                    clientSession = _UiClients.Values.FirstOrDefault(p => p.Id == sessionId);
                }
                finally
                {
                    ClientsSemaphore.Release();
                }

                if (clientSession != null)
                {
                    SendDataToClient(clientSession, new CommandDataObject() { Command = CommandTypes.TestStrategyStatus, Parameter = strategyTestStatus });

                    result = true;
                }
            }
            finally
            {
                InitializeNewSessionLock.ReleaseReaderLock();
            }

            return result;
        }

        private void SendDataToClient(UiClientSessionDataObject session, byte[] commandArray)
        {
            var sendDataResult = false;

            if (session.IsClientEnabled)
            {
                sendDataResult = true;

                if (commandArray != null)
                {
                    sendDataResult = false;

                    ClientsSemaphore.Wait();

                    try
                    {
                        if (session.Connected)
                        {
                            var encryptedData = session.Encrypt(commandArray);

                            if (encryptedData != null && encryptedData.Length != 0)
                            {
                                var sendData = new byte[encryptedData.Length + 4];

                                Buffer.BlockCopy(BitConverter.GetBytes(encryptedData.Length), 0, sendData, 0, 4);
                                Buffer.BlockCopy(encryptedData, 0, sendData, 4, encryptedData.Length);

                                session.Send(sendData, 0, sendData.Length);

                                sendDataResult = true;
                            }
                            else
                            {
                                var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                                logMessage += Environment.NewLine;

                                if (!string.IsNullOrWhiteSpace(session.Username))
                                {
                                    logMessage += session.Username + ", ";
                                }

                                logMessage += "Data encryption was failed!";

                                NetworkServer.Logger.Error(logMessage);
                            }
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        ClientsSemaphore.Release();
                    }
                }
            }

            if (!sendDataResult && session.Connected)
            {
                try
                {
                    session.Close();
                }
                catch
                {

                }
            }
        }

        private void SendDataToClient(UiClientSessionDataObject session, CommandDataObject command)
        {
            byte[] sendData;

            try
            {
                sendData = CommandHelper.Serialize(command);
            }
            catch
            {
                sendData = null;
            }

            SendDataToClient(session, sendData);
        }

        private void SendDataToClients(byte[] commandArray, List<UiClientSessionDataObject> uiClientSessions)
        {
            var sessionsShouldClose = new List<UiClientSessionDataObject>();

            if (commandArray != null)
            {
                ClientsSemaphore.Wait();

                try
                {
                    var sessions = uiClientSessions;

                    if (sessions == null)
                    {
                        sessions = _UiClients.Values.ToList();
                    }

                    foreach (var clientSession in sessions)
                    {
                        if (clientSession.IsClientEnabled)
                        {
                            if (clientSession.Connected)
                            {
                                var encryptedData = clientSession.Encrypt(commandArray);

                                if (encryptedData != null && encryptedData.Length != 0)
                                {
                                    var sendData = new byte[encryptedData.Length + 4];

                                    Buffer.BlockCopy(BitConverter.GetBytes(encryptedData.Length), 0, sendData, 0, 4);
                                    Buffer.BlockCopy(encryptedData, 0, sendData, 4, encryptedData.Length);

                                    try
                                    {
                                        clientSession.Send(sendData, 0, sendData.Length);
                                    }
                                    catch
                                    {
                                        sessionsShouldClose.Add(clientSession);
                                    }
                                }
                                else
                                {
                                    sessionsShouldClose.Add(clientSession);

                                    var logMessage = string.Format("Session: {0}/{1}:{2}", clientSession.Id, clientSession.RemoteEndPoint.Address.ToString(), clientSession.RemoteEndPoint.Port);

                                    logMessage += Environment.NewLine;

                                    if (!string.IsNullOrWhiteSpace(clientSession.Username))
                                    {
                                        logMessage += clientSession.Username + ", ";
                                    }

                                    logMessage += "Data encryption was failed!";

                                    NetworkServer.Logger.Error(logMessage);
                                }
                            }
                        }
                        else
                        {
                            sessionsShouldClose.Add(clientSession);
                        }
                    }
                }
                finally
                {
                    ClientsSemaphore.Release();
                }
            }

            foreach (var clientSession in sessionsShouldClose)
            {
                if (clientSession.Connected)
                {
                    try
                    {
                        clientSession.Close();
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void SendDataToClients(CommandDataObject command, List<UiClientSessionDataObject> uiClientSessions)
        {
            byte[] sendData;

            try
            {
                sendData = CommandHelper.Serialize(command);
            }
            catch
            {
                sendData = null;
            }

            SendDataToClients(sendData, uiClientSessions);
        }

        public void Start()
        {
            Backend = new MainBackend(SessionEstablishment.DatabaseSupport, InitializeNewSessionLock);

            Backend.ServerStatusPropertyChanged += Backend_ServerStatusPropertyChanged;
            Backend.MarketDataChanged += Backend_MarketDataChanged;
            Backend.NewAlarmsReceived += Backend_NewAlarmsReceived;
            Backend.NewAlarmsHistoryReceived += Backend_NewAlarmsHistoryReceived;
            Backend.MenuItemsStatusChanged += Backend_MenuItemsStatusChanged;

            Backend.MainProgressValueChanged += (value, detail) =>
            {
                MainProgressValueChanged?.Invoke(value, detail);
            };

            Backend.AutoSavingServiceWorkingNotified += (seviceWorking) =>
            {
                AutoSavingServiceWorkingNotified?.Invoke(seviceWorking);
            };

            Backend.StatusBarInformationReceived += (information) =>
            {
                StatusBarInformationReceived?.Invoke(information);
            };

            Backend.BinanceConnectionStatus += (status) =>
            {
                BinanceConnectionStatus?.Invoke(status);
            };

            Backend.StrategyTestStatusChanged += Backend_StrategyTestStatusChanged;
        }

        public void Stop()
        {
            Backend.Stop();
        }

        public bool StartListen()
        {
            NetworkServer = new NetworkServerService(SessionEstablishment);

            NetworkServer.LogReceived += NetworkServer_LogReceived;
            NetworkServer.SessionClosed += NetworkServer_SessionClosed;
            NetworkServer.NewRequestReceived += NetworkServer_NewRequestReceived;

            var result = NetworkServer.Start();

            if (!result)
            {
                NetworkServer = null;
            }

            return result;
        }

        public void StopListen()
        {
            NetworkServer?.Stop();

            try
            {
                ClientsSemaphore.Wait();

                _UiClients.Clear();
            }
            finally
            {
                ClientsSemaphore.Release();
            }
        }

        public void DisableClient(string username)
        {
            ClientsSemaphore.Wait();

            try
            {
                var session = _UiClients.Values.FirstOrDefault(p => p.Username == username);

                if (session != null)
                {
                    session.IsClientEnabled = false;

                    session.Close();
                }
            }
            catch
            {

            }
            finally
            {
                ClientsSemaphore.Release();
            }
        }

        public int GetConnectedUiClientsCount()
        {
            ClientsSemaphore.Wait();

            try
            {
                return _UiClients.Values.ToArray().Length;
            }
            finally
            {
                ClientsSemaphore.Release();
            }
        }

        public event LogReceivedHandler LogReceived;

        public event ProgressValueChangedHandler MainProgressValueChanged;

        public event ServiceWorkingNotifiedHandler AutoSavingServiceWorkingNotified;

        public event StatusBarInformationReceivedHandler StatusBarInformationReceived;

        public event BinanceConnectionStatusHandler BinanceConnectionStatus;
    }
}

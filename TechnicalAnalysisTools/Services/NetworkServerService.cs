using System;
using SuperSocket.ClientEngine;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using TechnicalAnalysisTools.Auxiliaries;
using TechnicalAnalysisTools.DataModels;
using TechnicalAnalysisTools.DataObjects;
using TechnicalAnalysisTools.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Services
{
    public class NetworkServerService
    {
        public NetworkServerService(SessionEstablishmentDataModel sessionEstablishment)
        {
            SessionEstablishment = sessionEstablishment;
        }

        private class CustomReceiveFilter : FixedHeaderReceiveFilter<RequestDataObject>
        {
            public CustomReceiveFilter() : base(4)
            {

            }

            protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
            {
                var result = BitConverter.ToInt32(header, offset);

                return result;
            }

            protected override RequestDataObject ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
            {
                if (length != 0)
                {
                    var result = new RequestDataObject(bodyBuffer.CloneRange(offset, length));

                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        private class ClientSideServer : AppServer<UiClientSessionDataObject, RequestDataObject>
        {
            public ClientSideServer() : base(new DefaultReceiveFilterFactory<CustomReceiveFilter, RequestDataObject>())
            {

            }
        }

        private SessionEstablishmentDataModel SessionEstablishment { get; }

        private ClientSideServer Server { get; set; }

        private DdosLimiterAuxiliary DdosLimiter { get; } = new DdosLimiterAuxiliary(2, 1);

        private void ServerLogFactory_LogReceived(LogDataObject log)
        {
            LogReceived?.Invoke(log);
        }

        private void Server_NewSessionConnected(UiClientSessionDataObject session)
        {
            if (DdosLimiter.IsPermitted(session.RemoteEndPoint.Address.ToString()))
            {
                NewSessionConnected?.Invoke(session);
            }
            else
            {
                var logMessage = string.Format("Session: {0}/{1}:{2}", session.Id, session.RemoteEndPoint.Address.ToString(), session.RemoteEndPoint.Port);

                logMessage += Environment.NewLine;
                logMessage += "DDoS attack detected!";

                Logger.Warn(logMessage);

                session.Close();
            }
        }

        private void Server_NewRequestReceived(UiClientSessionDataObject session, RequestDataObject requestInfo)
        {
            if (requestInfo.Body.Length != 4 || BitConverter.ToInt32(requestInfo.Body, 0) != (int)CommandTypes.ImAlive)
            {
                NewRequestReceived?.Invoke(session, requestInfo);
            }
        }

        private void Server_SessionClosed(UiClientSessionDataObject session, CloseReason value)
        {
            SessionClosed?.Invoke(session, value);
        }

        public ILog Logger
        {
            get
            {
                return Server.Logger;
            }
        }

        public bool Start()
        {
            var result = false;

            try
            {
                Server = new ClientSideServer();

                Server.NewSessionConnected += Server_NewSessionConnected;
                Server.NewRequestReceived += Server_NewRequestReceived;
                Server.SessionClosed += Server_SessionClosed;

                var rootConfig = new RootConfig();

                var serverConfig = new ServerConfig
                {
                    Ip = SessionEstablishment.Address,
                    Port = SessionEstablishment.Port,
                    Security = "None",
                    MaxConnectionNumber = 100,
                    Mode = SocketMode.Tcp,
                    MaxRequestLength = 100 * 1024,
                    KeepAliveTime = 30,
                    KeepAliveInterval = 5,
                    ListenBacklog = 100,
                    ReceiveBufferSize = 4096,
                    SendingQueueSize = 100,
                    SendTimeOut = 10000,
                    SendBufferSize = 100 * 1024,
                    LogBasicSessionActivity = true,
                    LogAllSocketException = true,
                    SessionSnapshotInterval = 5
                };

                if (Server.Setup(rootConfig, serverConfig, null, null, new ServerLogFactoryAuxiliary(ServerLogFactory_LogReceived)))
                {
                    if (Server.Start())
                    {
                        result = true;
                    }
                    else
                    {
                        Server = null;
                    }
                }
                else
                {
                    Server = null;
                }
            }
            catch
            {
                Server = null;

                result = false;
            }

            return result;
        }

        public void Stop()
        {
            try
            {
                Server?.Stop();
            }
            catch
            {

            }
        }

        public event LogReceivedHandler LogReceived;

        public event ServerSessionHandler<UiClientSessionDataObject> NewSessionConnected;

        public event ServerSessionHandler<UiClientSessionDataObject, CloseReason> SessionClosed;

        public event ServerRequestHandler<UiClientSessionDataObject, RequestDataObject> NewRequestReceived;
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;
using TechnicalAnalysisTools.Shared.DataObjects;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;
using TechnicalAnalysisTools.Ui.Win.Auxiliaries;
using TechnicalAnalysisTools.Ui.Win.DataModels;
using TechnicalAnalysisTools.Ui.Win.DataObjects;
using TechnicalAnalysisTools.Ui.Win.Delegates;

namespace TechnicalAnalysisTools.Ui.Win.Services
{
    internal class NetworkClientService
    {
        public NetworkClientService(SessionEstablishmentDataModel sessionEstablishment)
        {
            sessionEstablishment.IsAuthenticated = false;

            SessionEstablishment = sessionEstablishment;
        }

        private class CustomReceiveFilter : FixedHeaderReceiveFilter<RequestDataObject>
        {
            public CustomReceiveFilter() : base(4)
            {

            }

            protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
            {
                var result = bufferStream.ReadInt32(true);

                return result;
            }

            public override RequestDataObject ResolvePackage(IBufferStream bufferStream)
            {
                var bodySize = (int)bufferStream.Length - HeaderSize;

                var result = new RequestDataObject(new byte[bodySize]);

                if (bufferStream.Skip(HeaderSize).Read(result.Body, 0, bodySize) != bodySize)
                {
                    result = null;
                }

                return result;
            }
        }

        private static NetworkEncryptionAuxiliary NetworkEncryption { get; set; } = new NetworkEncryptionAuxiliary();

        private SessionEstablishmentDataModel SessionEstablishment { get; }

        private EasyClient<RequestDataObject> Client { get; set; }

        private CancellationTokenSource AsyncConnectionCancellationToken { get; set; }

        private Semaphore EncryptionSemaphore { get; } = new Semaphore(1, 1);

        private ManualResetEvent SignalConnectEvent { get; set; }

        private ManualResetEvent SignalAuthenticateEvent { get; set; }

        private void Client_Connected(object sender, EventArgs e)
        {
            SessionEstablishment.IsAuthenticated = false;

            SignalConnectEvent?.Set();

            Connected?.Invoke(sender, e);
        }

        private async void Client_NewPackageReceived(object sender, PackageEventArgs<RequestDataObject> e)
        {
            var decryptedResult = false;

            CommandDataObject command = null;

            try
            {
                EncryptionSemaphore.WaitOne();

                var decryptedData = NetworkEncryption.Decrypt(e.Package.Body);

                if (decryptedData != null && decryptedData.Length != 0)
                {
                    command = CommandHelper.Deserialize(decryptedData);
                }
            }
            catch
            {
                command = null;
            }
            finally
            {
                EncryptionSemaphore.Release();
            }

            if (command != null)
            {
                decryptedResult = true;

                if (SessionEstablishment.IsAuthenticated == false)
                {
                    if (command.Command == CommandTypes.SuccessfulAuthenticate)
                    {
                        if (command.Parameter is UiClientTypes)
                        {
                            var clientType = (UiClientTypes)command.Parameter;

                            if (clientType == UiClientTypes.Admin || clientType == UiClientTypes.Gold || clientType == UiClientTypes.Standard || clientType == UiClientTypes.Limited)
                            {
                                SessionEstablishment.ClientType = clientType;
                            }
                            else
                            {
                                SessionEstablishment.ClientType = UiClientTypes.Limited;
                            }
                        }
                        else
                        {
                            SessionEstablishment.ClientType = UiClientTypes.Limited;
                        }

                        SessionEstablishment.IsAuthenticated = true;

                        SignalAuthenticateEvent?.Set();
                    }
                    else
                    {
                        await Client.Close();
                    }
                }
                else
                {
                    CommandDataReceived?.Invoke(command);
                }
            }

            if (!decryptedResult)
            {
                try
                {
                    await Client.Close();
                }
                catch
                {

                }

                Client = null;
            }
        }

        private void Client_Error(object sender, ErrorEventArgs e)
        {
            if (AsyncConnectionCancellationToken != null)
            {
                AsyncConnectionCancellationToken.Cancel();

                SignalConnectEvent?.Set();
            }

            Client_Closed(sender, e);
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            var client = (EasyClient<RequestDataObject>)sender;

            try
            {
                client.Close();
            }
            catch
            {

            }

            SessionEstablishment.IsAuthenticated = false;

            SignalAuthenticateEvent?.Set();
            SignalConnectEvent?.Set();

            Closed?.Invoke(sender, e);
        }

        private string CreateUniqueKey()
        {
            var processor = new Func<string>(() =>
            {
                string result = "";

                try
                {
                    var objSearcher = new ManagementObjectSearcher("select * from Win32_Processor");
                    var objCollection = objSearcher.Get();
                    var objBaseObject = new ManagementBaseObject[objCollection.Count];

                    objCollection.CopyTo(objBaseObject, 0);

                    if (((ManagementObject)objBaseObject.GetValue(0)).Properties["ProcessorId"].Value == null)
                    {
                        result = "Null";
                    }
                    else
                    {
                        result = ((ManagementObject)objBaseObject.GetValue(0)).Properties["ProcessorId"].Value.ToString().Trim();
                    }
                }
                catch
                {
                    result = "AGPEWTMR";
                }

                result = new string(result.Trim().ToCharArray().Where(p => char.IsLetterOrDigit(p)).Where(p => p != '0').ToArray());

                while (result.Trim().Length < 8)
                {
                    result = result.Trim();

                    result += result;
                }

                result = result.Substring(result.Length - 8, 8).ToUpper();

                return result;
            });

            var phisycalSerialNumber = new Func<string>(() =>
            {
                string result = "";

                try
                {
                    var objSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                    var objCollection = objSearcher.Get();
                    var objBaseObject = new ManagementBaseObject[objCollection.Count];

                    objCollection.CopyTo(objBaseObject, 0);

                    if (((ManagementObject)objBaseObject.GetValue(0)).Properties["SerialNumber"].Value == null)
                    {
                        result = "Null";
                    }
                    else
                    {
                        result = ((ManagementObject)objBaseObject.GetValue(0)).Properties["SerialNumber"].Value.ToString().Trim();
                    }
                }
                catch
                {
                    result = "AGPEWTMR";
                }

                result = new string(result.Trim().ToCharArray().Where(p => char.IsLetterOrDigit(p)).Where(p => p != '0').ToArray());

                while (result.Trim().Length < 8)
                {
                    result = result.Trim();

                    result += result;
                }

                result = result.Substring(result.Length - 8, 8).ToUpper();

                return result;
            });

            var uniqueKey = processor() + phisycalSerialNumber();

            return HashHelper.CreateMD5(uniqueKey.PadLeft(16, '0'));
        }

        private async void ConnectClient()
        {
            Client.NoDelay = false;

            AsyncConnectionCancellationToken = new CancellationTokenSource();

            var cancellationToken = AsyncConnectionCancellationToken.Token;

            await Task.Run(() => Client.ConnectAsync(new DnsEndPoint(SessionEstablishment.Address, SessionEstablishment.Port)), cancellationToken);
        }

        public async Task<bool> Start()
        {
            var result = false;

            try
            {
                SignalConnectEvent = new ManualResetEvent(false);

                Client = new EasyClient<RequestDataObject>();

                Client.ReceiveBufferSize = 50 * 1024 * 1024;
     
                Client.Initialize(new CustomReceiveFilter());

                Client.Connected += Client_Connected;
                Client.NewPackageReceived += Client_NewPackageReceived;
                Client.Error += Client_Error;
                Client.Closed += Client_Closed;

                ConnectClient();

                SignalConnectEvent.WaitOne();

                AsyncConnectionCancellationToken = null;

                if (Client.IsConnected)
                {
                    result = true;

                    SignalAuthenticateEvent = new ManualResetEvent(false);

                    try
                    {
                        var sessionKey = NetworkEncryption.GetSessionKey();

                        if (sessionKey != null && sessionKey.Length != 0)
                        {
                            //
                            var sessionKeyInfo = CommandHelper.SessionKey(sessionKey);

                            var sessionKeyInfoForSend = new byte[sessionKeyInfo.Length + 4];

                            Buffer.BlockCopy(BitConverter.GetBytes(sessionKeyInfo.Length), 0, sessionKeyInfoForSend, 0, 4);
                            Buffer.BlockCopy(sessionKeyInfo, 0, sessionKeyInfoForSend, 4, sessionKeyInfo.Length);

                            Client.Send(sessionKeyInfoForSend);

                            //
                            var assembly = Assembly.GetExecutingAssembly();
                            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                            var parameters = new string[4];

                            parameters[0] = SessionEstablishment.Username.Trim().ToLower();
                            parameters[1] = SessionEstablishment.Password.Trim();
                            parameters[2] = HashHelper.CreateMD5(CreateUniqueKey());
                            parameters[3] = fileVersionInfo.FileVersion;

                            var loginInfo = CommandHelper.Serialize(new CommandDataObject() { Command = CommandTypes.Authenticate, Parameter = parameters });

                            result = await Send(loginInfo);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    catch
                    {
                        result = false;
                    }

                    if (Client.IsConnected)
                    {
                        SignalAuthenticateEvent.WaitOne();
                    }
                }

                if (result)
                {
                    result = Client.IsConnected;
                }

                if (!result)
                {
                    try
                    {
                        await Client.Close();
                    }
                    catch
                    {

                    }

                    Client = null;
                }
            }
            catch
            {
                try
                {
                    await Client.Close();
                }
                catch
                {

                }

                Client = null;

                result = false;
            }

            return result;
        }

        public async Task<bool> Send(byte[] data)
        {
            var result = false;

            EncryptionSemaphore.WaitOne();

            try
            {
                if (Client != null && Client.IsConnected)
                {
                    var encryptedData = NetworkEncryption.Encrypt(data);

                    var sendData = new byte[encryptedData.Length + 4];

                    Buffer.BlockCopy(BitConverter.GetBytes(encryptedData.Length), 0, sendData, 0, 4);
                    Buffer.BlockCopy(encryptedData, 0, sendData, 4, encryptedData.Length);

                    Client.Send(sendData);

                    result = true;
                }
            }
            catch
            {
                await Client.Close();
            }
            finally
            {
                EncryptionSemaphore.Release();
            }

            return result;
        }

        public void Stop()
        {
            try
            {
                Client?.Close();
            }
            catch
            {

            }
        }

        public event EventHandler Connected;

        public event CommandDataReceivedHandler CommandDataReceived;

        public event EventHandler Closed;
    }
}

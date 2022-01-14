using System;
using System.Threading;
using SuperSocket.SocketBase;
using TechnicalAnalysisTools.Auxiliaries;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.DataObjects
{
    public class UiClientSessionDataObject : AppSession<UiClientSessionDataObject, RequestDataObject>
    {
        public UiClientSessionDataObject()
        {
            Id = Guid.NewGuid();

            IsEncryptionAdjusted = false;

            IsAuthenticated = false;

            InitializedData = false;

            IsClientEnabled = false;
        }

        private Semaphore EncryptionSemaphore { get; } = new Semaphore(1, 1);

        private NetworkEncryptionAuxiliary NetworkEncryption { get; set; }

        public Guid Id { get; }

        public string Username { get; set; }

        public UiClientTypes ClientType { get; set; }

        public bool IsEncryptionAdjusted { get; private set; }

        public bool IsAuthenticated { get; set; }

        public bool IsReady => IsEncryptionAdjusted && IsAuthenticated;

        public bool InitializedData { get; set; }

        public bool IsClientEnabled { get; set; }

        public void InitializeNetworkEncryption(NetworkEncryptionAuxiliary networkEncryption)
        {
            NetworkEncryption = networkEncryption;

            IsEncryptionAdjusted = true;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] result;

            EncryptionSemaphore.WaitOne();

            try
            {
                result = NetworkEncryption.Encrypt(data);
            }
            finally
            {
                EncryptionSemaphore.Release();
            }

            return result;
        }

        public byte[] Decrypt(byte[] data)
        {
            byte[] result;

            EncryptionSemaphore.WaitOne();

            try
            {
                result = NetworkEncryption.Decrypt(data);
            }
            finally
            {
                EncryptionSemaphore.Release();
            }

            return result;
        }
    }
}

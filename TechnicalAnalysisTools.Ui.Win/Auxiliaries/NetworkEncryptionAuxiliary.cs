using System;
using System.Linq;
using System.Runtime.InteropServices;
using TechnicalAnalysisTools.Shared.Helpers;
using TechnicalAnalysisTools.Ui.Win.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Auxiliaries
{
    internal class NetworkEncryptionAuxiliary
    {
        public NetworkEncryptionAuxiliary()
        {
            Initialize(true);
        }

        ~NetworkEncryptionAuxiliary()
        {
            if (PublicKeyHandle != IntPtr.Zero)
            {
                CryptDestroyKey(PublicKeyHandle);
            }

            if (ServerContextHandle != IntPtr.Zero)
            {
                CryptReleaseContext(ServerContextHandle, 0x00000000);
            }
        }

        private static IntPtr ServerContextHandle { get; set; } = IntPtr.Zero;

        private static IntPtr PublicKeyHandle { get; set; } = IntPtr.Zero;

        private IntPtr EncryptionKeyHandle { get; set; } = IntPtr.Zero;

        private byte[] EncryptionKey { get; set; }

        private bool IsConfigured => ServerContextHandle != IntPtr.Zero && PublicKeyHandle != IntPtr.Zero && EncryptionKeyHandle != IntPtr.Zero && EncryptionKey != null;

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptAcquireContext(out IntPtr phProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptExportKey(IntPtr hKey, IntPtr hExpKey, uint dwBlobType, uint dwFlags, byte[] pbData, out uint pdwDataLen);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptImportKey(IntPtr hProv, byte[] pbData, uint dwDataLen, IntPtr hPubKey, uint dwFlags, out IntPtr phKey);

        [DllImport("AdvApi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptGenKey(IntPtr providerContext, uint algorithmId, uint flags, out IntPtr cryptKeyHandle);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptSetKeyParam(IntPtr hProv, uint dwParam, byte[] pbData, uint dwFlags);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptEncrypt(IntPtr hKey, IntPtr hHash, [MarshalAs(UnmanagedType.Bool)] bool final, uint dwFlags, byte[] pbData, ref uint pdwDataLen, uint dwBufLen);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptDecrypt(IntPtr hKey, IntPtr hHash, [MarshalAs(UnmanagedType.Bool)] bool final, uint dwFlags, byte[] pbData, ref uint pdwDataLen);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptDestroyKey(IntPtr hKey);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

        private byte[] CloneByteArray(byte[] inArray)
        {
            var result = new byte[inArray.Length];

            Buffer.BlockCopy(inArray, 0, result, 0, inArray.Length);

            return result;
        }

        private void Initialize(bool inGenerateSessionKey)
        {
            ServerContextHandle = IntPtr.Zero;
            PublicKeyHandle = IntPtr.Zero;
            EncryptionKeyHandle = IntPtr.Zero;

            try
            {
                if (AcquireContext())
                {
                    if (ImportServerPublicKey(ClientConstantHelper.ServerPublicKey))
                    {
                        if (inGenerateSessionKey)
                        {
                            GenerateSessionKey();
                        }
                    }
                }
            }
            catch
            {
                ServerContextHandle = IntPtr.Zero;
                PublicKeyHandle = IntPtr.Zero;
                EncryptionKeyHandle = IntPtr.Zero;
            }
        }

        private bool AcquireContext()
        {
            bool result;

            try
            {
                uint provRsaAes = 0x00000018;
                uint cryptVerifycontext = 0xF0000000;

                result = CryptAcquireContext(out IntPtr serverContextHandle, string.Empty, "Microsoft Enhanced RSA and AES Cryptographic Provider", provRsaAes, cryptVerifycontext);

                if (result)
                {
                    ServerContextHandle = serverContextHandle;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private bool ImportServerPublicKey(byte[] serverPublicKey)
        {
            bool result;

            try
            {
                result = CryptImportKey(ServerContextHandle, serverPublicKey, (uint)serverPublicKey.Length, IntPtr.Zero, 0, out IntPtr publicKeyHandle);

                if (result)
                {
                    PublicKeyHandle = publicKeyHandle;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private void GenerateSessionKey()
        {
            try
            {
                if (CryptGenKey(ServerContextHandle, 0x00006610, 0x00000001, out IntPtr encryptionKeyHandle))
                {
                    if (CryptSetKeyParam(encryptionKeyHandle, 0x00000001, SharedConstantHelper.SharedAesInitializationVector, 0))
                    {
                        byte[] encryptionKey;

                        if (CryptExportKey(encryptionKeyHandle, PublicKeyHandle, 0x00000001, 0, null, out uint encryptionKeyLength))
                        {
                            encryptionKey = new byte[encryptionKeyLength];

                            if (CryptExportKey(encryptionKeyHandle, PublicKeyHandle, 0x00000001, 0, encryptionKey, out encryptionKeyLength))
                            {
                                EncryptionKey = encryptionKey;
                                EncryptionKeyHandle = encryptionKeyHandle;
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

        public byte[] GetSessionKey()
        {
            if (IsConfigured)
            {
                return EncryptionKey;
            }

            return null;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] result = null;

            try
            {
                if (IsConfigured)
                {
                    result = CloneByteArray(data);

                    var length = (uint)data.Length;

                    if (!CryptEncrypt(EncryptionKeyHandle, IntPtr.Zero, true, 0x00000000, result, ref length, (uint)result.Length))
                    {
                        result = new byte[length];

                        Buffer.BlockCopy(data, 0, result, 0, data.Length);

                        length = (uint)data.Length;

                        if (!CryptEncrypt(EncryptionKeyHandle, IntPtr.Zero, true, 0x00000000, result, ref length, (uint)result.Length))
                        {
                            result = null;
                        }
                    }
                    else
                    {
                        result = result.Take((int)length).ToArray();
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        public byte[] Decrypt(byte[] data)
        {
            byte[] result = null;

            try
            {
                if (IsConfigured)
                {
                    result = CloneByteArray(data);

                    var length = (uint)data.Length;

                    if (CryptDecrypt(EncryptionKeyHandle, IntPtr.Zero, true, 0, result, ref length))
                    {
                        var array = new byte[length];

                        Buffer.BlockCopy(result, 0, array, 0, (int)length);

                        result = array;
                    }
                    else
                    {
                        result = null;
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }
    }
}

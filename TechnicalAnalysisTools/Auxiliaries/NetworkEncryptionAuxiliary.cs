using System;
using System.Linq;
using System.Runtime.InteropServices;
using TechnicalAnalysisTools.Helpers;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class NetworkEncryptionAuxiliary
    {
        ~NetworkEncryptionAuxiliary()
        {
            if (EncryptionKeyHandle != IntPtr.Zero)
            {
                CryptDestroyKey(EncryptionKeyHandle);
            }
        }

        private static IntPtr ServerContextHandle { get; set; } = IntPtr.Zero;

        private static IntPtr PublicKeyPairHandle { get; set; } = IntPtr.Zero;

        private IntPtr EncryptionKeyHandle { get; set; } = IntPtr.Zero;

        private bool IsConfigured => ServerContextHandle != IntPtr.Zero && PublicKeyPairHandle != IntPtr.Zero && EncryptionKeyHandle != IntPtr.Zero;

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptAcquireContext(out IntPtr phProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptExportKey(IntPtr hKey, IntPtr hExpKey, uint dwBlobType, uint dwFlags, byte[] pbData, out uint pdwDataLen);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptImportKey(IntPtr hProv, byte[] pbData, uint dwDataLen, IntPtr hPubKey, uint dwFlags, out IntPtr phKey);

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

        private static bool AcquireContext()
        {
            bool result;

            try
            {
                uint provRsaAes = 0x00000018;

                result = CryptAcquireContext(out IntPtr serverContextHandle, ServerConstantHelper.ServerKeyContainerName, "Microsoft Enhanced RSA and AES Cryptographic Provider", provRsaAes, 0);

                if (!result)
                {
                    uint cryptNewkeyset = 0x00000008;

                    result = CryptAcquireContext(out serverContextHandle, ServerConstantHelper.ServerKeyContainerName, "Microsoft Enhanced RSA and AES Cryptographic Provider", provRsaAes, cryptNewkeyset);
                }

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

        private static bool ImportPublicPrivateKey()
        {
            bool result;

            try
            {
                uint cryptNoSalt = 0x00000010;

                result = CryptImportKey(ServerContextHandle, ServerConstantHelper.ServerPublicPrivateKey, (uint)ServerConstantHelper.ServerPublicPrivateKey.Length, IntPtr.Zero, cryptNoSalt, out IntPtr keyPairHandle);

                if (result)
                {
                    PublicKeyPairHandle = keyPairHandle;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private static bool ExportPublicKey()
        {
            bool result;

            try
            {
                uint publickeyblob = 0x00000006;

                byte[] publicKeyBuffer = new byte[0];

                result = CryptExportKey(PublicKeyPairHandle, IntPtr.Zero, publickeyblob, 0, publicKeyBuffer, out uint publicKeyBufferLength);

                if (!result)
                {
                    publicKeyBuffer = new byte[publicKeyBufferLength];

                    result = CryptExportKey(PublicKeyPairHandle, IntPtr.Zero, publickeyblob, 0, publicKeyBuffer, out publicKeyBufferLength);
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private bool ImportEncryptionKey(byte[] inClientExportedEncryptedSessionKey, int inClientExportedEncryptedSessionKeyLength)
        {
            bool result;

            try
            {
                result = CryptImportKey(ServerContextHandle, inClientExportedEncryptedSessionKey, (uint)inClientExportedEncryptedSessionKeyLength, PublicKeyPairHandle, 0x00000000, out IntPtr cryptSessionKey);

                if (result)
                {
                    result = CryptSetKeyParam(cryptSessionKey, 1, Shared.Helpers.SharedConstantHelper.SharedAesInitializationVector, 0);

                    if (result)
                    {
                        EncryptionKeyHandle = cryptSessionKey;
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public static bool CreatePublicKey()
        {
            var result = false;

            if (AcquireContext())
            {
                if (ImportPublicPrivateKey())
                {
                    if (ExportPublicKey())
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        public static void DestroyPublicKey()
        {
            if (PublicKeyPairHandle != IntPtr.Zero)
            {
                CryptDestroyKey(PublicKeyPairHandle);
            }

            if (ServerContextHandle != IntPtr.Zero)
            {
                CryptReleaseContext(ServerContextHandle, 0x00000000);
            }
        }

        public bool InitializeBySessionKey(byte[] inClientExportedEncryptedSessionKey)
        {
            var result = ImportEncryptionKey(inClientExportedEncryptedSessionKey, inClientExportedEncryptedSessionKey.Length);

            return result;
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

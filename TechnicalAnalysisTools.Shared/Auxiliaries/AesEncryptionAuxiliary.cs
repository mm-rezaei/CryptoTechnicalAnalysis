using System;
using System.Linq;
using System.Runtime.InteropServices;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.Auxiliaries
{
    public class AesEncryptionAuxiliary
    {
        public AesEncryptionAuxiliary() : this(SharedConstantHelper.SharedAesKey)
        {

        }

        public AesEncryptionAuxiliary(byte[] inEncryptionKey)
        {
            Initialize(inEncryptionKey);
        }

        ~AesEncryptionAuxiliary()
        {
            if (EncryptionKeyHandle != IntPtr.Zero)
            {
                CryptDestroyKey(EncryptionKeyHandle);
            }

            if (ServerContextHandle != IntPtr.Zero)
            {
                CryptReleaseContext(ServerContextHandle, 0x00000000);
            }
        }

        private IntPtr ServerContextHandle { get; set; } = IntPtr.Zero;

        private IntPtr EncryptionKeyHandle { get; set; } = IntPtr.Zero;

        private bool IsConfigured => ServerContextHandle != IntPtr.Zero && EncryptionKeyHandle != IntPtr.Zero;

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptAcquireContext(out IntPtr phProv, IntPtr pszContainer, IntPtr pszProvider, uint dwProvType, uint dwFlags);

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

        private void Initialize(byte[] inEncryptionKey)
        {
            if (AcquireContext())
            {
                ImportEncryptionKey(inEncryptionKey);
            }
        }

        private byte[] CloneByteArray(byte[] inArray)
        {
            var result = new byte[inArray.Length];

            Buffer.BlockCopy(inArray, 0, result, 0, inArray.Length);

            return result;
        }

        private bool AcquireContext()
        {
            bool result;

            try
            {
                uint provRsaAes = 0x00000018;
                uint cryptVerifycontext = 0xF0000000;

                result = CryptAcquireContext(out IntPtr serverContextHandle, IntPtr.Zero, IntPtr.Zero, provRsaAes, cryptVerifycontext);

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

        private bool ImportEncryptionKey(byte[] inEncryptionKey)
        {
            bool result;

            try
            {
                //
                var aesBlob256 = new byte[44];

                byte plaintextkeyblob = 0x08;
                byte curBlobVersion = 0x02;
                uint calgAes256 = 0x00006610;
                uint keyLength = 0x00000020;

                var calgAes256Array = BitConverter.GetBytes(calgAes256);
                var keyLengthArray = BitConverter.GetBytes(keyLength);

                aesBlob256[0] = plaintextkeyblob;
                aesBlob256[1] = curBlobVersion;
                aesBlob256[2] = 0;
                aesBlob256[3] = 0;

                for (var index = 0; index < 4; index++)
                {
                    aesBlob256[index + 4] = calgAes256Array[index];
                }

                for (var index = 0; index < 4; index++)
                {
                    aesBlob256[index + 8] = keyLengthArray[index];
                }

                for (var index = 0; index < keyLength; index++)
                {
                    aesBlob256[index + 12] = inEncryptionKey[index];
                }

                //
                result = CryptImportKey(ServerContextHandle, aesBlob256, (uint)aesBlob256.Length, IntPtr.Zero, 0, out IntPtr encryptionKeyHandle);

                if (result)
                {
                    result = CryptSetKeyParam(encryptionKeyHandle, 1, SharedConstantHelper.SharedAesInitializationVector, 0);

                    if (result)
                    {
                        EncryptionKeyHandle = encryptionKeyHandle;
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public byte[] Encrypt(byte[] inData)
        {
            byte[] result = null;

            try
            {
                if (IsConfigured)
                {
                    result = CloneByteArray(inData);

                    var length = (uint)inData.Length;

                    if (!CryptEncrypt(EncryptionKeyHandle, IntPtr.Zero, true, 0, result, ref length, (uint)result.Length))
                    {
                        result = new byte[length];

                        Buffer.BlockCopy(inData, 0, result, 0, inData.Length);

                        length = (uint)inData.Length;

                        if (!CryptEncrypt(EncryptionKeyHandle, IntPtr.Zero, true, 0, result, ref length, (uint)result.Length))
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

        public byte[] Decrypt(byte[] inData)
        {
            byte[] result = null;

            try
            {
                if (IsConfigured)
                {
                    result = CloneByteArray(inData);

                    var length = (uint)inData.Length;

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

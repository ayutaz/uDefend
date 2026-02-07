using System;
using uDefend.Encryption;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.IO;
using System.Runtime.InteropServices;
#endif

namespace uDefend.KeyManagement
{
    /// <summary>
    /// Key provider using Windows DPAPI (Data Protection API) via P/Invoke.
    /// Keys are protected with the current user's credentials and stored on disk.
    /// </summary>
    public sealed class WindowsDpapiKeyProvider : IKeyStore, IDisposable
    {
        private readonly string _keyFilePath;
        private byte[] _cachedKey;
        private bool _disposed;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [StructLayout(LayoutKind.Sequential)]
        private struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptProtectData(
            ref DATA_BLOB pDataIn,
            string szDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            ref DATA_BLOB pDataOut);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptUnprotectData(
            ref DATA_BLOB pDataIn,
            IntPtr ppszDataDescr,
            IntPtr pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            ref DATA_BLOB pDataOut);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;

        private static byte[] Protect(byte[] data)
        {
            var dataIn = new DATA_BLOB();
            var dataOut = new DATA_BLOB();
            IntPtr pinned = IntPtr.Zero;

            try
            {
                pinned = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, pinned, data.Length);
                dataIn.cbData = data.Length;
                dataIn.pbData = pinned;

                if (!CryptProtectData(ref dataIn, null, IntPtr.Zero, IntPtr.Zero,
                        IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN, ref dataOut))
                {
                    throw new InvalidOperationException(
                        $"CryptProtectData failed (error code: {Marshal.GetLastWin32Error()}).");
                }

                var result = new byte[dataOut.cbData];
                Marshal.Copy(dataOut.pbData, result, 0, dataOut.cbData);
                return result;
            }
            finally
            {
                if (pinned != IntPtr.Zero) Marshal.FreeHGlobal(pinned);
                if (dataOut.pbData != IntPtr.Zero) LocalFree(dataOut.pbData);
            }
        }

        private static byte[] Unprotect(byte[] data)
        {
            var dataIn = new DATA_BLOB();
            var dataOut = new DATA_BLOB();
            IntPtr pinned = IntPtr.Zero;

            try
            {
                pinned = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, pinned, data.Length);
                dataIn.cbData = data.Length;
                dataIn.pbData = pinned;

                if (!CryptUnprotectData(ref dataIn, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                        IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN, ref dataOut))
                {
                    throw new InvalidOperationException(
                        $"CryptUnprotectData failed (error code: {Marshal.GetLastWin32Error()}).");
                }

                var result = new byte[dataOut.cbData];
                Marshal.Copy(dataOut.pbData, result, 0, dataOut.cbData);
                return result;
            }
            finally
            {
                if (pinned != IntPtr.Zero) Marshal.FreeHGlobal(pinned);
                if (dataOut.pbData != IntPtr.Zero) LocalFree(dataOut.pbData);
            }
        }
#endif

        public WindowsDpapiKeyProvider(string keyIdentifier)
        {
            if (string.IsNullOrEmpty(keyIdentifier))
                throw new ArgumentException("Key identifier must not be null or empty.", nameof(keyIdentifier));

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "uDefend", "Keys");
            Directory.CreateDirectory(appDataDir);
            _keyFilePath = Path.Combine(appDataDir, keyIdentifier + ".dpapi");
#else
            _keyFilePath = null;
#endif
        }

        public byte[] GetMasterKey()
        {
            ThrowIfDisposed();

            if (_cachedKey != null)
                return CopyKey(_cachedKey);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_keyFilePath != null && File.Exists(_keyFilePath))
            {
                byte[] protectedData = File.ReadAllBytes(_keyFilePath);
                _cachedKey = Unprotect(protectedData);
                return CopyKey(_cachedKey);
            }

            // Generate new key
            _cachedKey = CryptoUtility.GenerateRandomBytes(32);
            StoreMasterKeyInternal(_cachedKey);
            return CopyKey(_cachedKey);
#else
            throw new PlatformNotSupportedException("Windows DPAPI is only available on Windows.");
#endif
        }

        public bool HasMasterKey()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return _cachedKey != null || (_keyFilePath != null && File.Exists(_keyFilePath));
#else
            return _cachedKey != null;
#endif
        }

        public void StoreMasterKey(byte[] key)
        {
            ThrowIfDisposed();
            if (key == null || key.Length == 0)
                throw new ArgumentException("Key must not be null or empty.", nameof(key));

            _cachedKey = new byte[key.Length];
            Buffer.BlockCopy(key, 0, _cachedKey, 0, key.Length);
            StoreMasterKeyInternal(key);
        }

        public void DeleteMasterKey()
        {
            ThrowIfDisposed();

            if (_cachedKey != null)
            {
                CryptoUtility.SecureClear(_cachedKey);
                _cachedKey = null;
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_keyFilePath != null && File.Exists(_keyFilePath))
                File.Delete(_keyFilePath);
#endif
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_cachedKey != null)
            {
                CryptoUtility.SecureClear(_cachedKey);
                _cachedKey = null;
            }
        }

        private void StoreMasterKeyInternal(byte[] key)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_keyFilePath != null)
            {
                byte[] protectedData = Protect(key);
                File.WriteAllBytes(_keyFilePath, protectedData);
            }
#endif
        }

        private static byte[] CopyKey(byte[] source)
        {
            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WindowsDpapiKeyProvider));
        }
    }
}

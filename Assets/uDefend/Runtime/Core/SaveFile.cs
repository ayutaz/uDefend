using System;
using System.IO;
using System.Threading.Tasks;
using uDefend.Encryption;

namespace uDefend.Core
{
    public static class SaveFile
    {
        public static async Task WriteAsync(string path, byte[] plainData, byte[] encKey, byte[] hmacKey,
            ushort version, IEncryptionProvider provider, EncryptionType encryptionType = EncryptionType.AesCbcHmac)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (plainData == null) throw new ArgumentNullException(nameof(plainData));
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            string tmpPath = path + ".tmp";
            byte[] encryptedData = provider.Encrypt(plainData, encKey, hmacKey);

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var writer = new BinaryWriter(fs))
                {
                    ushort flags = SaveFileFormat.BuildFlags(false, encryptionType);
                    SaveFileFormat.WriteHeader(writer, version, flags);
                    writer.Write(encryptedData);
                    await fs.FlushAsync().ConfigureAwait(false);
                }

                // Atomic replace: delete target then move temp
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(tmpPath, path);
            }
            catch
            {
                // Clean up temp file on failure
                if (File.Exists(tmpPath))
                {
                    try { File.Delete(tmpPath); }
                    catch { /* best effort cleanup */ }
                }
                throw;
            }
        }

        public static async Task<(byte[] data, ushort version)> ReadAsync(string path, byte[] encKey, byte[] hmacKey,
            IEncryptionProvider provider)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            if (!File.Exists(path))
                throw new FileNotFoundException("Save file not found.", path);

            byte[] encryptedData;
            ushort version;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (var reader = new BinaryReader(fs))
            {
                (version, _) = SaveFileFormat.ReadHeader(reader);

                int remaining = (int)(fs.Length - fs.Position);
                encryptedData = reader.ReadBytes(remaining);
            }

            byte[] plainData = provider.Decrypt(encryptedData, encKey, hmacKey);
            return (plainData, version);
        }
    }
}

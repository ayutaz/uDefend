using System;
using System.IO;

namespace uDefend.Core
{
    public static class SaveFileFormat
    {
        public static readonly byte[] MagicBytes = { 0x75, 0x53, 0x41, 0x56 }; // "uSAV"
        public const int HeaderSize = 8; // Magic(4) + Version(2) + Flags(2)

        // Flags bit layout:
        // bit 0: compressed
        // bits 1-2: encryption type (00=AesCbcHmac, 01=AesGcm, 10=None)
        public static ushort BuildFlags(bool compressed, EncryptionType encryption)
        {
            ushort flags = 0;
            if (compressed)
                flags |= 1;
            flags |= (ushort)((int)encryption << 1);
            return flags;
        }

        public static (bool compressed, EncryptionType encryption) ParseFlags(ushort flags)
        {
            bool compressed = (flags & 1) != 0;
            var encryption = (EncryptionType)((flags >> 1) & 0x03);
            return (compressed, encryption);
        }

        public static void WriteHeader(BinaryWriter writer, ushort version, ushort flags)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.Write(MagicBytes);
            writer.Write(version);
            writer.Write(flags);
        }

        public static (ushort version, ushort flags) ReadHeader(BinaryReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (!ValidateMagic(reader))
                throw new InvalidDataException("Invalid save file: magic number mismatch.");

            ushort version = reader.ReadUInt16();
            ushort flags = reader.ReadUInt16();
            return (version, flags);
        }

        public static bool ValidateMagic(BinaryReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            byte[] magic = reader.ReadBytes(MagicBytes.Length);
            if (magic.Length != MagicBytes.Length)
                return false;

            for (int i = 0; i < MagicBytes.Length; i++)
            {
                if (magic[i] != MagicBytes[i])
                    return false;
            }
            return true;
        }
    }
}

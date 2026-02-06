using System;
using uDefend.Encryption;

namespace uDefend.KeyManagement
{
    public sealed class EnvelopeEncryption
    {
        private const int DekSize = 32;

        private readonly IKeyProvider _keyProvider;
        private readonly IEncryptionProvider _encryptionProvider;

        public EnvelopeEncryption(IKeyProvider keyProvider)
            : this(keyProvider, new AesCbcHmacProvider()) { }

        public EnvelopeEncryption(IKeyProvider keyProvider, IEncryptionProvider encryptionProvider)
        {
            _keyProvider = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));
            _encryptionProvider = encryptionProvider ?? throw new ArgumentNullException(nameof(encryptionProvider));
        }

        public EncryptedEnvelope Encrypt(byte[] plaintext)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));

            byte[] masterKey = _keyProvider.GetMasterKey();
            KeyDerivation.DeriveKeys(masterKey, out byte[] masterEncKey, out byte[] masterHmacKey);
            CryptoUtility.SecureClear(masterKey);

            // Generate a random DEK
            byte[] dek = CryptoUtility.GenerateRandomBytes(DekSize);
            KeyDerivation.DeriveKeys(dek, out byte[] dataEncKey, out byte[] dataHmacKey);

            // Encrypt data with DEK-derived keys
            byte[] encryptedData = _encryptionProvider.Encrypt(plaintext, dataEncKey, dataHmacKey);

            // Encrypt DEK with master-derived keys
            byte[] encryptedDek = _encryptionProvider.Encrypt(dek, masterEncKey, masterHmacKey);

            // Clear sensitive material
            CryptoUtility.SecureClear(dek);
            CryptoUtility.SecureClear(dataEncKey);
            CryptoUtility.SecureClear(dataHmacKey);
            CryptoUtility.SecureClear(masterEncKey);
            CryptoUtility.SecureClear(masterHmacKey);

            return new EncryptedEnvelope(encryptedDek, encryptedData);
        }

        public byte[] Decrypt(EncryptedEnvelope envelope)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            byte[] masterKey = _keyProvider.GetMasterKey();
            KeyDerivation.DeriveKeys(masterKey, out byte[] masterEncKey, out byte[] masterHmacKey);
            CryptoUtility.SecureClear(masterKey);

            // Decrypt DEK
            byte[] dek;
            try
            {
                dek = _encryptionProvider.Decrypt(envelope.EncryptedDek, masterEncKey, masterHmacKey);
            }
            finally
            {
                CryptoUtility.SecureClear(masterEncKey);
                CryptoUtility.SecureClear(masterHmacKey);
            }

            // Derive data keys from DEK
            KeyDerivation.DeriveKeys(dek, out byte[] dataEncKey, out byte[] dataHmacKey);
            CryptoUtility.SecureClear(dek);

            // Decrypt data
            byte[] plaintext;
            try
            {
                plaintext = _encryptionProvider.Decrypt(envelope.EncryptedData, dataEncKey, dataHmacKey);
            }
            finally
            {
                CryptoUtility.SecureClear(dataEncKey);
                CryptoUtility.SecureClear(dataHmacKey);
            }

            return plaintext;
        }

        public EncryptedEnvelope RotateKey(EncryptedEnvelope envelope, IKeyProvider newKeyProvider)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (newKeyProvider == null) throw new ArgumentNullException(nameof(newKeyProvider));

            // Decrypt DEK with old master key
            byte[] oldMasterKey = _keyProvider.GetMasterKey();
            KeyDerivation.DeriveKeys(oldMasterKey, out byte[] oldMasterEncKey, out byte[] oldMasterHmacKey);
            CryptoUtility.SecureClear(oldMasterKey);

            byte[] dek;
            try
            {
                dek = _encryptionProvider.Decrypt(envelope.EncryptedDek, oldMasterEncKey, oldMasterHmacKey);
            }
            finally
            {
                CryptoUtility.SecureClear(oldMasterEncKey);
                CryptoUtility.SecureClear(oldMasterHmacKey);
            }

            // Re-encrypt DEK with new master key
            byte[] newMasterKey = newKeyProvider.GetMasterKey();
            KeyDerivation.DeriveKeys(newMasterKey, out byte[] newMasterEncKey, out byte[] newMasterHmacKey);
            CryptoUtility.SecureClear(newMasterKey);

            byte[] newEncryptedDek;
            try
            {
                newEncryptedDek = _encryptionProvider.Encrypt(dek, newMasterEncKey, newMasterHmacKey);
            }
            finally
            {
                CryptoUtility.SecureClear(dek);
                CryptoUtility.SecureClear(newMasterEncKey);
                CryptoUtility.SecureClear(newMasterHmacKey);
            }

            // Data stays the same - only the DEK wrapper changes
            return new EncryptedEnvelope(newEncryptedDek, envelope.EncryptedData);
        }
    }

    public sealed class EncryptedEnvelope
    {
        public byte[] EncryptedDek { get; }
        public byte[] EncryptedData { get; }

        public EncryptedEnvelope(byte[] encryptedDek, byte[] encryptedData)
        {
            EncryptedDek = encryptedDek ?? throw new ArgumentNullException(nameof(encryptedDek));
            EncryptedData = encryptedData ?? throw new ArgumentNullException(nameof(encryptedData));
        }
    }
}

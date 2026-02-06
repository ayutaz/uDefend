namespace uDefend.Encryption
{
    public interface IEncryptionProvider
    {
        byte[] Encrypt(byte[] plaintext, byte[] encryptionKey, byte[] hmacKey);
        byte[] Decrypt(byte[] ciphertext, byte[] encryptionKey, byte[] hmacKey);
    }
}

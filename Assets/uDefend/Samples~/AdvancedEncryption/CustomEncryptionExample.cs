using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using uDefend.Core;
using uDefend.Encryption;
using uDefend.KeyManagement;

/// <summary>
/// Advanced encryption usage example.
/// Demonstrates low-level encryption API and custom key provider usage.
/// </summary>
public class CustomEncryptionExample : MonoBehaviour
{
    private void Start()
    {
        DemoStringEncryption();
        DemoLowLevelEncryption();
        DemoPbkdf2KeyProvider();
    }

    /// <summary>
    /// Simple string encryption using the StringEncryption utility.
    /// </summary>
    private void DemoStringEncryption()
    {
        Debug.Log("=== String Encryption Demo ===");

        string password = "MySecretPassword123";
        string original = "This is sensitive data.";

        string encrypted = StringEncryption.Encrypt(original, password);
        Debug.Log($"Encrypted: {encrypted}");

        string decrypted = StringEncryption.Decrypt(encrypted, password);
        Debug.Log($"Decrypted: {decrypted}");
        Debug.Log($"Match: {original == decrypted}");
    }

    /// <summary>
    /// Low-level encryption using IEncryptionProvider directly.
    /// </summary>
    private void DemoLowLevelEncryption()
    {
        Debug.Log("=== Low-Level Encryption Demo ===");

        IEncryptionProvider provider = new AesCbcHmacProvider();

        // Generate keys securely
        byte[] encKey = CryptoUtility.GenerateRandomBytes(32);  // AES-256 key
        byte[] hmacKey = CryptoUtility.GenerateRandomBytes(32); // HMAC-SHA256 key

        try
        {
            byte[] plaintext = Encoding.UTF8.GetBytes("Hello, encrypted world!");

            // Encrypt — IV is generated automatically, HMAC appended
            byte[] ciphertext = provider.Encrypt(plaintext, encKey, hmacKey);
            Debug.Log($"Ciphertext length: {ciphertext.Length} bytes");

            // Decrypt — HMAC verified first, then decrypted
            byte[] decrypted = provider.Decrypt(ciphertext, encKey, hmacKey);
            string result = Encoding.UTF8.GetString(decrypted);
            Debug.Log($"Decrypted: {result}");

            // Clean up sensitive data
            CryptoUtility.SecureClear(decrypted);
        }
        finally
        {
            CryptoUtility.SecureClear(encKey);
            CryptoUtility.SecureClear(hmacKey);
        }
    }

    /// <summary>
    /// Using PBKDF2 key provider with a user-supplied passphrase.
    /// </summary>
    private void DemoPbkdf2KeyProvider()
    {
        Debug.Log("=== PBKDF2 Key Provider Demo ===");

        byte[] passphrase = Encoding.UTF8.GetBytes("user-password-here");

        using (var keyProvider = new Pbkdf2KeyProvider(passphrase))
        {
            // Derive master key (SHA-256, 600,000 iterations)
            byte[] key = keyProvider.GetMasterKey();
            Debug.Log($"Derived key length: {key.Length} bytes (256-bit)");

            // Save the salt for later use (needed to re-derive the same key)
            byte[] salt = keyProvider.GetSalt();
            Debug.Log($"Salt: {Convert.ToBase64String(salt)}");

            CryptoUtility.SecureClear(key);
        }

        // Clear the passphrase buffer
        CryptoUtility.SecureClear(passphrase);
    }
}

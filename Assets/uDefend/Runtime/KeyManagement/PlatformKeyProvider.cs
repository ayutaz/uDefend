using System;
using System.Text;
using UnityEngine;

namespace uDefend.KeyManagement
{
    /// <summary>
    /// Factory that creates the appropriate IKeyProvider for the current platform.
    /// - Windows: DPAPI (user-scoped, hardware-independent)
    /// - Android: Android Keystore (hardware-backed TEE/StrongBox when available)
    /// - iOS: Keychain Services (Secure Enclave when available)
    /// - Other: PBKDF2 fallback (identifier-derived key)
    /// </summary>
    public static class PlatformKeyProvider
    {
        /// <summary>
        /// Creates a platform-appropriate key provider.
        /// On supported platforms, keys are stored in hardware-backed secure storage.
        /// On unsupported platforms, falls back to PBKDF2 key derivation.
        /// </summary>
        /// <param name="identifier">
        /// Application-unique identifier used as key alias/service name.
        /// Use the same identifier consistently to retrieve the same key.
        /// </param>
        public static IKeyProvider Create(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException("Identifier must not be null or empty.", nameof(identifier));

#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidKeystoreKeyProvider(identifier);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosKeychainKeyProvider(identifier);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new WindowsDpapiKeyProvider(identifier);
#else
            // Fallback: derive a key from the identifier using PBKDF2
            byte[] passphrase = Encoding.UTF8.GetBytes(identifier);
            return new Pbkdf2KeyProvider(passphrase);
#endif
        }

        /// <summary>
        /// Creates a platform-appropriate key store (supports StoreMasterKey/DeleteMasterKey).
        /// Throws PlatformNotSupportedException on platforms without native key storage.
        /// </summary>
        public static IKeyStore CreateStore(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException("Identifier must not be null or empty.", nameof(identifier));

#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidKeystoreKeyProvider(identifier);
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosKeychainKeyProvider(identifier);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return new WindowsDpapiKeyProvider(identifier);
#else
            throw new PlatformNotSupportedException(
                "Native key storage is not available on this platform. " +
                "Use PlatformKeyProvider.Create() for PBKDF2 fallback.");
#endif
        }
    }
}

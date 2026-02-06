using System;
using System.Text;

namespace uDefend.KeyManagement
{
    public static class PlatformKeyProvider
    {
        public static IKeyProvider Create(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException("Identifier must not be null or empty.", nameof(identifier));

            // For now, all platforms use PBKDF2 fallback.
            // Future milestones will add:
            //   - Android Keystore (RuntimePlatform.Android)
            //   - iOS Keychain (RuntimePlatform.IPhonePlayer)
            //   - Windows DPAPI (RuntimePlatform.WindowsPlayer / WindowsEditor)

            byte[] passphrase = Encoding.UTF8.GetBytes(identifier);
            return new Pbkdf2KeyProvider(passphrase);
        }
    }
}

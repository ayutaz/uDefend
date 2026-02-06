namespace uDefend.KeyManagement
{
    /// <summary>
    /// Provides master key retrieval for envelope encryption.
    /// Callers own the returned byte[] and must clear it when done.
    /// </summary>
    public interface IKeyProvider
    {
        /// <summary>Returns a copy of the master key. Caller must clear the returned buffer.</summary>
        byte[] GetMasterKey();

        /// <summary>Returns true if a master key is available.</summary>
        bool HasMasterKey();
    }

    /// <summary>
    /// Extended key provider that supports key storage and deletion.
    /// Use for platform key providers (Android Keystore, iOS Keychain, DPAPI).
    /// </summary>
    public interface IKeyStore : IKeyProvider
    {
        void StoreMasterKey(byte[] key);
        void DeleteMasterKey();
    }
}

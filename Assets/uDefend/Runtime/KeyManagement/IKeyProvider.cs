namespace uDefend.KeyManagement
{
    public interface IKeyProvider
    {
        byte[] GetMasterKey();
        void StoreMasterKey(byte[] key);
        bool HasMasterKey();
    }
}

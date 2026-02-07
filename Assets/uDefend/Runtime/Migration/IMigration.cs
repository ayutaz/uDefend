namespace uDefend.Migration
{
    public interface IMigration
    {
        ushort FromVersion { get; }
        ushort ToVersion { get; }
        byte[] Migrate(byte[] data);
    }
}

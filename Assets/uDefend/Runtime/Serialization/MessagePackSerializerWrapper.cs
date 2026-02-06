using System;

namespace uDefend.Serialization
{
    /// <summary>
    /// ISerializer implementation wrapping MessagePack v3.
    /// Requires the UDEFEND_MESSAGEPACK scripting define and the MessagePack v3 package to be installed.
    /// </summary>
    public sealed class MessagePackSerializerWrapper : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
#if UDEFEND_MESSAGEPACK
            return MessagePack.MessagePackSerializer.Serialize(obj);
#else
            throw new NotSupportedException(
                "MessagePack v3 is not installed. Add the MessagePack v3 package and define UDEFEND_MESSAGEPACK.");
#endif
        }

        public T Deserialize<T>(byte[] data)
        {
#if UDEFEND_MESSAGEPACK
            return MessagePack.MessagePackSerializer.Deserialize<T>(data);
#else
            throw new NotSupportedException(
                "MessagePack v3 is not installed. Add the MessagePack v3 package and define UDEFEND_MESSAGEPACK.");
#endif
        }
    }
}

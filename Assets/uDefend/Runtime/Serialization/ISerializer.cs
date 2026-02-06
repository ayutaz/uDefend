using System;

namespace uDefend.Serialization
{
    /// <summary>
    /// Abstraction for object serialization/deserialization.
    /// </summary>
    /// <remarks>
    /// Implementations must throw <see cref="ArgumentNullException"/> when
    /// <c>obj</c> (for Serialize) or <c>data</c> (for Deserialize) is null.
    /// Deserialize must also throw <see cref="ArgumentException"/> when <c>data</c> is empty.
    /// </remarks>
    public interface ISerializer
    {
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null.</exception>
        byte[] Serialize<T>(T obj);

        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="data"/> is empty.</exception>
        T Deserialize<T>(byte[] data);
    }
}

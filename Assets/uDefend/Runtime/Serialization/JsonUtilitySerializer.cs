using System;
using System.Text;
using UnityEngine;

namespace uDefend.Serialization
{
    /// <summary>
    /// ISerializer implementation using UnityEngine.JsonUtility.
    /// Note: JsonUtility does not support Dictionary, properties, polymorphism, or null collections.
    /// Fields must be public or marked with [SerializeField]. Types must be marked with [Serializable].
    /// </summary>
    public sealed class JsonUtilitySerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var json = JsonUtility.ToJson(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Data cannot be empty.", nameof(data));

            var json = Encoding.UTF8.GetString(data);
            return JsonUtility.FromJson<T>(json);
        }
    }
}

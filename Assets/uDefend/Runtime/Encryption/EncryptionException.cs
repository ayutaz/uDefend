using System;

namespace uDefend.Encryption
{
    public class EncryptionException : Exception
    {
        public EncryptionException()
            : base("An encryption operation failed.") { }

        public EncryptionException(string message)
            : base(message) { }

        public EncryptionException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class TamperDetectedException : EncryptionException
    {
        public TamperDetectedException()
            : base("Data integrity verification failed.") { }

        public TamperDetectedException(string message)
            : base(message) { }

        public TamperDetectedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

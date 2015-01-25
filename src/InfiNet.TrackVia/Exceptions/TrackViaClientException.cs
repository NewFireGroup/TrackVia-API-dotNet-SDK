using System;

namespace InfiNet.TrackVia.Exceptions
{
    [Serializable]
    public class TrackViaClientException : Exception
    {
        public TrackViaClientException() { }
        public TrackViaClientException(string message) : base(message) { }
        public TrackViaClientException(string message, Exception inner) : base(message, inner) { }
        protected TrackViaClientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace InfiNet.TrackVia.Exceptions
{
    [SerializableAttribute] 
    public class InvalidXmlException : Exception
    {
        public InvalidXmlException() { }
        public InvalidXmlException(string message) : base(message) { }
        public InvalidXmlException(string message, Exception inner) : base(message, inner) { }

        public InvalidXmlException(string message, Exception inner, string httpRequest, string httpResponse)
            : base(message, inner)
        {
            HttpRequest = httpRequest;
            HttpResponse = httpResponse;
        }

        protected InvalidXmlException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public string HttpRequest { get; private set; }

        public string HttpResponse { get; private set; }

        #region Implement Serializable

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("HttpRequest", HttpRequest);
            info.AddValue("HttpResponse", HttpResponse);
        }

        #endregion
    }
}

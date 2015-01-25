using System;
using System.Net;
using System.Xml.Linq;

namespace InfiNet.TrackVia.HttpClient
{
    [Serializable()]
    public class HttpClientResponse
    {
        /// <summary>
        /// The original HttpRequest
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string HttpRequest { get; set; }
        public string Content { get; set; }
        public HttpClientResponseTypes ContentType { get; set; }
        public string MediaType { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public byte[] FileContent { get; set; }

        public XElement ContentAsXElement()
        {
            return XElement.Parse(Content);
        }

    }
}

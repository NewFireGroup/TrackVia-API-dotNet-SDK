using InfiNet.TrackVia.HttpClient;

namespace InfiNet.TrackVia.Model
{
    public class OAuth2HttpClientResponse : HttpClientResponse
    {
        public OAuth2Token Token { get; set; }
    }
}

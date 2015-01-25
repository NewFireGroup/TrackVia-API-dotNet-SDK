using System;

namespace InfiNet.TrackVia.HttpClient
{
    [Serializable()]
    public enum HttpClientResponseTypes
    {
        unknown = 0,
        none = 1,
        text = 2,
        xml = 3,
        json = 4,
        file = 5
    }
}

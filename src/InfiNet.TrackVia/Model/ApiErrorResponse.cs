using System.Collections.Generic;

namespace InfiNet.TrackVia.Model
{
    /// <summary>
    /// Captures the two ways the Trackvia Service passes back error information, as name/value pairs:
    /// 1. error + error_description (optional)
    /// 2. errors + message + name + code + stackTrace (optional)
    /// </summary>
    public class ApiErrorResponse
    {
        // Type 1 error response
        public List<string> Errors { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string stackTrace { get; set; }

        // Type 2 error response
        public string Error { get; set; }
        public string Error_Description { get; set; }
    }
}


namespace InfiNet.TrackVia.Model
{

    public class ApiError
    {
        public static readonly ApiError invalid_grant;
        public static readonly ApiError bad_credentials;
        public static readonly ApiError invalid_token;
        public static readonly ApiError unhandled_error;

        static ApiError()
        {
            invalid_grant = new ApiError("invalid_grant", "invalid authorization grant");
            bad_credentials = new ApiError("invalid_grant", "Bad credentials");
            invalid_token = new ApiError("invalid_token", "invalid access or refresh token");
            unhandled_error = new ApiError("unhandled_error", "unknown issue occurred when making api request");
        }


        public ApiError(string key, string description)
        {
            this.code = key;
            this.description = description;
        }

        public string code { get; set; }
        public string description { get; set; }

    }
}

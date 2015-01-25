using InfiNet.TrackVia.Model;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace InfiNet.TrackVia.Exceptions
{
    [Serializable]
    public class TrackViaApiException : Exception
    {
        #region Constructors

        public TrackViaApiException() : this(null) { }
        public TrackViaApiException(ApiErrorResponse apiErrorResponse) 
        {
            this.ApiErrorResponse = apiErrorResponse ?? new ApiErrorResponse();
        }
        #endregion

        /// <summary>
        /// Retrieves a formatted message of the TrackVia API erorr without having to always
        /// include null reference checking or common formatting code in client programs.
        /// </summary>
        /// <returns></returns>
        public string GetApiErrorMessage()
        {
            if (ApiErrorResponse != null)
                return string.Format("TrackVia API Error: '{0}', Description '{1}'", ApiErrorResponse.Error, 
                    ApiErrorResponse.Error_Description);
            else
                return string.Format("TrackVia API '{0}': {1}", this.GetType().ToString(), Message);
        }

        public ApiErrorResponse ApiErrorResponse { get; private set; }

        #region Implement Serializable

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ApiErrorResponse", ApiErrorResponse);
        }

        #endregion
    }
}
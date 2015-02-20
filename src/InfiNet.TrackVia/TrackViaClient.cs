using InfiNet.TrackVia.Exceptions;
using InfiNet.TrackVia.HttpClient;
using InfiNet.TrackVia.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace InfiNet.TrackVia
{
    public class TrackViaClient : IDisposable
    {
        protected const string DEFAULT_BASE_URI_PATH = "";
        protected const Scheme DEFAULT_SCHEME = Scheme.https;
        protected const int DEFAULT_PORT = 443;

        protected const string ACCESS_TOKEN_QUERY_PARAM = "access_token";
        protected const string REFRESH_TOKEN_QUERY_PARAM_AND_VALID_GRANT_TYPE = "refresh_token";
        protected const string CLIENT_ID_QUERY_PARAM = "client_id";
        protected const string USER_KEY_QUERY_PARAM = "user_key";
        protected const string GRANT_TYPE_QUERY_PARAM = "grant_type";
        
        private string _hostName;
        private string _baseUriPath;
        private Scheme _scheme;
        private int _port;
        private string _apiKey;

        private OAuth2Token _lastGoodToken;

        private readonly IAsyncHttpClientHelper _httpClient;

        #region ctors

        /// <summary>
        /// Creates a client, with which to access the Trackvia API.
        /// </summary>
        /// <param name="httpClient">async HttpClient helper</param>
        /// <param name="hostName">host of the service api endpoint</param>
        /// <param name="username">name of an account user with access to targeted views and forms</param>
        /// <param name="password">password of the account user</param>
        /// <param name="apiUserKey">3Scale user key, granted when registering using the Trackvia Developer Portal</param>
        public TrackViaClient(IAsyncHttpClientHelper httpClient, string hostName, string username, string password, string apiUserKey) :
            this(httpClient, DEFAULT_BASE_URI_PATH, DEFAULT_SCHEME, hostName, DEFAULT_PORT, username, password, apiUserKey) { }

        /// <summary>
        /// Creates a client, with which to access the Trackvia API.
        /// </summary>
        /// <param name="hostName">host of the service api endpoint</param>
        /// <param name="username">name of an account user with access to targeted views and forms</param>
        /// <param name="password">password of the account user</param>
        /// <param name="apiUserKey">3Scale user key, granted when registering using the Trackvia Developer Portal</param>
        public TrackViaClient(string hostName, string username, string password, string apiUserKey) :
            this(null, DEFAULT_BASE_URI_PATH, DEFAULT_SCHEME, hostName, DEFAULT_PORT, username, password, apiUserKey) { }

        /// <summary>
        /// Creates a client, with which to access the Trackvia API.
        /// </summary>
        /// <param name="httpClient">async HttpClient helper</param>
        /// <param name="baseUriPath">prefixed to every HTTP request, before API-specific path segments (e.g., /openapi)</param>
        /// <param name="scheme">one of the supported protocol schemes (http or https)</param>
        /// <param name="hostName">host of the service api endpoint</param>
        /// <param name="port">port of the service endpoint (default: 443)</param>
        /// <param name="username">name of an account user with access to targeted views and forms</param>
        /// <param name="password">password of the account user</param>
        /// <param name="apiUserKey">3Scale user key, granted when registering using the Trackvia Developer Portal</param>
        public TrackViaClient(IAsyncHttpClientHelper httpClient, string baseUriPath, Model.Scheme scheme, string hostName, int port, string username, string password, string apiKey)
        {
            this._baseUriPath = baseUriPath;
            this._scheme = scheme;
            this._hostName = hostName;
            this._port = port;
            this._apiKey = apiKey;

            _httpClient = httpClient ?? new AsyncHttpClientHelper();

            // Call Authorization
            Authorize(username, password);
        }

        /// <summary>
        /// Facilitates use of mocking frameworks for testing.
        /// </summary>
        /// <param name="httpClient">async HttpClient helper</param>
        /// <param name="hostName">host of the service api endpoint</param>
        /// <param name="apiUserKey">3Scale user key, granted when registering using the Trackvia Developer Portal</param>
        /// <returns>TrackViaClient</returns>
        /// <remarks>
        /// Helpful when using TrackViaClient in asynchronous environments such as ASP.Net where you want
        /// to call the AuthorizeAsync(...) instead of Authorize(...)
        /// </remarks>
        public TrackViaClient(IAsyncHttpClientHelper httpClient, string hostName, string apiUserKey)
        {
            this._baseUriPath = DEFAULT_BASE_URI_PATH;
            this._scheme = DEFAULT_SCHEME;
            this._hostName = hostName;
            this._port = DEFAULT_PORT;
            this._apiKey = apiUserKey;

            _httpClient = httpClient;
        }

        /// <summary>
        /// Instantiates a TrackVia client without performing the performing authorization
        /// </summary>
        /// <param name="hostName">host of the service api endpoint</param>
        /// <param name="apiUserKey">3Scale user key, granted when registering using the Trackvia Developer Portal</param>
        /// <returns></returns>
        public TrackViaClient(string hostName, string apiUserKey)
        {
            this._baseUriPath = DEFAULT_BASE_URI_PATH;
            this._scheme = DEFAULT_SCHEME;
            this._hostName = hostName;
            this._port = DEFAULT_PORT;
            this._apiKey = apiUserKey;

            _httpClient = new AsyncHttpClientHelper();
        }

        #endregion

        #region internal methods

        public OAuth2Token LastGoodToken
        {
            get { return this._lastGoodToken; }
        }

        private string GetRefreshToken()
        {
            return (this._lastGoodToken != null && this._lastGoodToken.RefreshToken != null) ?
                            (this._lastGoodToken.RefreshToken.Value) : (null);
        }

        private string GetAccessToken()
        {
            return (this._lastGoodToken != null) ? (this._lastGoodToken.Value) : (null);
        }

        private string GetApiUserKey()
        {
            return this._apiKey;
        }

        private static void CheckTrackViaApiResponseForErrors(HttpClientResponse Response)
        {
            ApiErrorResponse errorResponse = null;
            switch (Response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created: // Used during create
                case System.Net.HttpStatusCode.NoContent:
                    // proceed with code
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(Response.Content);
                    throw new TrackViaApiException(errorResponse);
                case System.Net.HttpStatusCode.BadRequest:
                    errorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(Response.Content);
                    throw new TrackViaApiException(errorResponse);
                //throw new TrackViaApiException(new ApiErrorResponse()
                //{
                //    Code = Response.StatusCode.ToString(),
                //    Message = Response.Content,
                //    Error = ApiError.invalid_grant.code,
                //    ErrorDescription = ApiError.invalid_grant.description
                //});
                default:
                    throw new TrackViaApiException(new ApiErrorResponse()
                    {
                        Code = Response.StatusCode.ToString(),
                        Message = Response.Content,
                        Error = ApiError.unhandled_error.code,
                        Error_Description = ApiError.unhandled_error.description
                    });
            }
        }

        #endregion 

        #region Unit Test Support Methods

        /// <summary>
        /// Used primarily for testing purposes to we can keep LastGoodToken private
        /// </summary>
        /// <param name="reference">an OAuth2Token with which we compare</param>
        /// <returns></returns>
        public bool ValidateLastGoodTokenIsEqual(OAuth2Token reference)
        {
            return (_lastGoodToken != null && _lastGoodToken.Equals(reference));
        }

        public bool ValidateLastGoodTokenHasNotExpired(DateTime referencePointInTime)
        {
            return (this._lastGoodToken != null && this._lastGoodToken.Expiration >= referencePointInTime);
        }

        public bool ValidateAccessTokenIsPresent()
        {
            return (GetAccessToken() != null);
        }

        #endregion

        #region Public Authentication Methods

        /// <summary>
        /// Authorizes the client for access to views and forms of a given account user.
        /// </summary>
        /// <param name="username">name of the account user</param>
        /// <param name="password">password of the account user</param>
        /// <remarks>
        /// Side effect of a successful authentication try is the client saves the resulting
        /// access and refresh token, caching it for future client calls.
        /// </remarks>
        public void Authorize(string username, string password)
        {
            string url = AuthorizeBuildUrl(username, password);

            HttpClientResponse response = _httpClient.SendGetRequestAsync(url).Result;

            AuthorizeHandleResponse(response);
        }

        /// <summary>
        /// Authorizes the client for access to views and forms of a given account user.
        /// </summary>
        /// <param name="username">name of the account user</param>
        /// <param name="password">password of the account user</param>
        /// <remarks>
        /// Side effect of a successful authentication try is the client saves the resulting
        /// access and refresh token, caching it for future client calls.
        /// </remarks>
        public async Task AuthorizeAsync(string username, string password)
        {
            string url = AuthorizeBuildUrl(username, password);

            HttpClientResponse response = await _httpClient.SendGetRequestAsync(url);

            AuthorizeHandleResponse(response);
        }

        private void AuthorizeHandleResponse(HttpClientResponse response)
        {
            CheckTrackViaApiResponseForErrors(response);

            OAuth2Token token = JsonConvert.DeserializeObject<OAuth2Token>(response.Content);

            this._lastGoodToken = token;
        }

        private string AuthorizeBuildUrl(string username, string password)
        {
            string path = String.Format("{0}/oauth/token", this._baseUriPath);
            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter("username", username)
                    .SetParameter("password", password)
                    .SetParameter(CLIENT_ID_QUERY_PARAM, "TrackViaAPI")
                    .SetParameter(GRANT_TYPE_QUERY_PARAM, "password")
                    .Build()
            };

            string url = uriBuilder.ToString();
            return url;
        }

        /// <summary>
        /// Force refresh of the last known good token, using the refresh token provided
        /// by the service for that token.
        ///
        /// The client will automatically try to refresh the access token if it encounters
        /// an ApiError.InvalidToken error on any service call.  Should this fail, that error
        /// will be rethrown.  Catching it will provide an empty to handle authentication outside
        /// of the client.  The advantage of refreshAccessToken is it is faster than calling
        /// authorize() and you don't have to pass user credentials.
        /// </summary>
        public void RefreshAccessToken()
        {
            string url = RefreshAccessTokenBuildUrl(GetRefreshToken());

            HttpClientResponse Response = _httpClient.SendGetRequestAsync(url).Result;

            RefreshAccessTokenCheckResponseAndUpdateLastGoodToken(Response);
        }

        /// <summary>
        /// Force refresh of the last known good token, using the refresh token provided
        /// by the service for that token.
        ///
        /// The client will automatically try to refresh the access token if it encounters
        /// an ApiError.InvalidToken error on any service call.  Should this fail, that error
        /// will be rethrown.  Catching it will provide an empty to handle authentication outside
        /// of the client.  The advantage of refreshAccessToken is it is faster than calling
        /// authorize() and you don't have to pass user credentials.
        /// </summary>
        public async void RefreshAccessTokenAsync()
        {
            string url = RefreshAccessTokenBuildUrl(GetRefreshToken());

            HttpClientResponse Response = await _httpClient.SendGetRequestAsync(url);

            RefreshAccessTokenCheckResponseAndUpdateLastGoodToken(Response);
        }

        /// <summary>
        /// Force refresh of the last known good token using the refresh token provided
        /// by the supplied refresh token token.
        /// 
        /// This is used when trying to instantiate a new TrackVia client using a refresh token
        /// from another instance (ex: web site without persisting TrackVia client between  requests).
        /// </summary>
        /// <param name="validRefreshToken"></param>
        public async Task RefreshAccessTokenAsync(string validRefreshToken)
        {
            string url = RefreshAccessTokenBuildUrl(validRefreshToken);
            
            HttpClientResponse Response = await _httpClient.SendGetRequestAsync(url);

            RefreshAccessTokenCheckResponseAndUpdateLastGoodToken(Response);
        }

        private void RefreshAccessTokenCheckResponseAndUpdateLastGoodToken(HttpClientResponse Response)
        {
            CheckTrackViaApiResponseForErrors(Response);

            OAuth2Token token = JsonConvert.DeserializeObject<OAuth2Token>(Response.Content);

            this._lastGoodToken = token;
        }

        private string RefreshAccessTokenBuildUrl(string validRefreshToken)
        {
            string path = String.Format("{0}/oauth/token", this._baseUriPath);
            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(GRANT_TYPE_QUERY_PARAM, REFRESH_TOKEN_QUERY_PARAM_AND_VALID_GRANT_TYPE)
                    .SetParameter(CLIENT_ID_QUERY_PARAM, "TrackViaAPI")
                    .SetParameter(REFRESH_TOKEN_QUERY_PARAM_AND_VALID_GRANT_TYPE, validRefreshToken)
                    .Build()
            };

            string url = uriBuilder.ToString();
            return url;
        }

        #endregion

        #region Public Application/View Administration Methods

        /// <summary>
        /// Gets the applications available to the authenticated user.
        /// </summary>
        /// <returns>list of applications, which may be empty if none are available</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public List<App> getApps()
        {
            string path = String.Format("{0}/openapi/apps", this._baseUriPath);

            HttpClientResponse Response = getCommonSharedCode(path);

            List<App> apps = JsonConvert.DeserializeObject<List<App>>(Response.Content);

            return apps;
        }

        /// <summary>
        /// Gets views available to the authenticated user.
        /// </summary>
        /// <returns>a list of views, which may be empty if none are available</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public List<View> getViews()
        {
            return getViews(null);
        }

        /// <summary>
        /// Gets views matching optionalName parameter available to the authenticated user.
        /// </summary>
        /// <param name="optionalName">Optional case sensitive view name to filter results</param>
        /// <returns>a list of views, which may be empty if none are available</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public List<View> getViews(string optionalName)
        {
            string path = String.Format("{0}/openapi/views", this._baseUriPath);

            UriHelper queryStringHelper = new UriHelper()
                     .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                     .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey());
                     
            if(!string.IsNullOrWhiteSpace(optionalName))
            {
                queryStringHelper.SetParameter("name", optionalName);
            }

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = queryStringHelper.Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendGetRequestAsync(url);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            List<View> views = JsonConvert.DeserializeObject<List<View>>(Response.Content);

            return views;
        }

        /// <summary>
        /// Gets a view by its name, if available to the authenticated user.
        /// </summary>
        /// <param name="name">name the case-sensitive view name to get</param>
        /// <returns>the view or null if not found</returns>
        public View getFirstMatchingView(string name)
        {
            List<View> views = getViews(name);

            return (views == null || views.Count == 0) ? (null) : (views[0]);
        }

        #endregion

        #region Public GetRecord(s) Methods

        /// <summary>
        /// Gets records available to the authenticated user in the given view.
        /// 
        /// Use with small tables, when all records can be reasonably transferred in a single call.
        /// </summary>
        /// <param name="viewId">view identifier in which to get records</param>
        /// <returns>both field metadata and record data, as a record set</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public RecordSet getRecords(long viewId)
        {
            string path = String.Format("{0}/openapi/views/{1}", this._baseUriPath, viewId);

            HttpClientResponse Response = getCommonSharedCode(path);

            RecordSet recordSet = JsonConvert.DeserializeObject<RecordSet>(Response.Content);

            return recordSet;
        }

        /// <summary>
        /// Gets records available to the authenticated user in the given view.
        /// 
        /// Use with small tables, when all records can be reasonably transferred in a single call.
        /// </summary>
        /// <typeparam name="T">return instances of this type (instead of a raw record Map<String, Object>)</typeparam>
        /// <param name="viewId">viewId view identifier in which to get records</param>
        /// <returns>both field metadata and record data, as a record set</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public DomainRecordSet<T> getRecords<T>(long viewId)
        {
            string path = String.Format("{0}/openapi/views/{1}", this._baseUriPath, viewId);

            HttpClientResponse Response = getCommonSharedCode(path);

            DomainRecordSet<T> recordSet = JsonConvert.DeserializeObject<DomainRecordSet<T>>(Response.Content);

            return recordSet;
        }

        /// <summary>
        /// Gets a record.  The record must be available to the authenticated user in the given view.
        /// </summary>
        /// <param name="viewId">viewId view identifier in which to get records</param>
        /// <param name="recordId">recordId unique record identifier</param>
        /// <returns></returns>
        public Record getRecord(long viewId, long recordId)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/{2}", this._baseUriPath, viewId, recordId);

            HttpClientResponse Response = getCommonSharedCode(path);

            Record record = JsonConvert.DeserializeObject<Record>(Response.Content);

            return record;
        }

        /// <summary>
        /// Gets a record.  The record must be available to the authenticated user in the given view.
        /// </summary>
        /// <param name="viewId">viewId view identifier in which to get records</param>
        /// <param name="recordId">recordId unique record identifier</param>
        /// <returns></returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public DomainRecord<T> getRecord<T>(long viewId, long recordId)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/{2}", this._baseUriPath, viewId, recordId);

            HttpClientResponse Response = getCommonSharedCode(path);

            DomainRecord<T> record = JsonConvert.DeserializeObject<DomainRecord<T>>(Response.Content);

            return record;
        }

        /// <summary>
        /// Common getRecord(s) code between RecordSet and DomainRecordSet methods
        /// </summary>
        /// <param name="path">URL path for the Get Request</param>
        /// <returns></returns>
        private HttpClientResponse getCommonSharedCode(string path)
        {
            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendGetRequestAsync(url);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            return Response;
        }
        #endregion

        #region Public FindRecord Methods & Supporting Private Methods

        /// <summary>
        /// Finds record matching given search criteria, returning native records.
        /// </summary>
        /// <param name="viewId">view identifier in which to search for records</param>
        /// <param name="searchCriteria">query substring used for a substring match against all of the user-defined fields</param>
        /// <param name="startIndex">the index (0 based) of the first user record, useful for paging</param>
        /// <param name="maxRecords">retrieve no more than this many user records, must be 0 < max < 101</param>
        /// <returns>a list of application objects matching the search criteria, which may be empty</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public RecordSet findRecords(long viewId, string searchCriteria, int startIndex, int maxRecords)
        {
            HttpClientResponse Response = findRecordsShared(viewId, searchCriteria, startIndex, maxRecords);

            RecordSet recordSet = JsonConvert.DeserializeObject<RecordSet>(Response.Content);

            return recordSet;
        }

        /// <summary>
        /// Finds record matching given search criteria, returning native records.
        /// </summary>
        /// <param name="viewId">view identifier in which to search for records</param>
        /// <param name="searchCriteria">query substring used for a substring match against all of the user-defined fields</param>
        /// <param name="startIndex">the index (0 based) of the first user record, useful for paging</param>
        /// <param name="maxRecords">retrieve no more than this many user records, must be 0 < max < 101</param>
        /// <returns>a list of application objects matching the search criteria, which may be empty</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public DomainRecordSet<T> findRecords<T>(long viewId, string searchCriteria, int startIndex, int maxRecords)
        {
            HttpClientResponse Response = findRecordsShared(viewId, searchCriteria, startIndex, maxRecords);

            DomainRecordSet<T> recordSet = JsonConvert.DeserializeObject<DomainRecordSet<T>>(Response.Content);

            return recordSet;
        }

        /// <summary>
        /// Common find code between RecordSet and DomainRecordSet methods
        /// </summary>
        private HttpClientResponse findRecordsShared(long viewId, string searchCriteria, int startIndex, int maxRecords)
        {
            string path = String.Format("{0}/openapi/views/{1}/find", this._baseUriPath, viewId);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                     .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                     .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                     .SetParameter("q", searchCriteria)
                     .SetParameter("start", startIndex < 0 ? "0" : startIndex.ToString())
                     .SetParameter("max", maxRecords < 1 ? "50" : maxRecords > 100 ? "100" : maxRecords.ToString())
                     .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendGetRequestAsync(url);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);
            return Response;
        }

        #endregion

        #region Public Create Methods

        /// <summary>
        /// Creates a batch of records in a view accessible to the authenticated user.
        /// Record id field will be set to a newly assigned value.
        /// </summary>
        /// <param name="viewId">view identifier in which to create the record batch</param>
        /// <param name="batch">batch one or more records for creation</param>
        /// <returns>both field metadata and record data</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public RecordSet createRecords(long viewId, RecordDataBatch batch)
        {
            string path = String.Format("{0}/openapi/views/{1}/records", this._baseUriPath, viewId);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            string jsonSerializedData = JsonConvert.SerializeObject(batch);

            Task<HttpClientResponse> Request = _httpClient.SendPostJsonRequestAsync(url, jsonSerializedData);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            RecordSet recordSet = JsonConvert.DeserializeObject<RecordSet>(Response.Content);

            return recordSet;
        }

        /// <summary>
        /// Creates a batch of records in a view accessible to the authenticated user.
        /// Record id field will be set to a newly assigned value.
        /// </summary>
        /// <param name="viewId">view identifier in which to create the record batch</param>
        /// <param name="batch">batch one or more records for creation</param>
        /// <returns>both field metadata and record data, as a record set of <T> objects</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public DomainRecordSet<T> createRecords<T>(long viewId, DomainRecordDataBatch<T> batch)
        {

            string path = String.Format("{0}/openapi/views/{1}/records", this._baseUriPath, viewId);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            string jsonSerializedData = JsonConvert.SerializeObject(batch);

            Task<HttpClientResponse> Request = _httpClient.SendPostJsonRequestAsync(url, jsonSerializedData);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            DomainRecordSet<T> recordSet = JsonConvert.DeserializeObject<DomainRecordSet<T>>(Response.Content);

            return recordSet;
        }

        #endregion

        #region Public Update Methods

        /// <summary>
        /// Updates a record in a view accessible to the authenticated user.
        /// </summary>
        /// <param name="viewId">view identifier in which to update the record</param>
        /// <param name="recordId">unique record identifier</param>
        /// <param name="data">data instance of RecordData</param>
        /// <returns>Update Record</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public Record updateRecord(long viewId, long recordId, RecordData data)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/{2}", this._baseUriPath, viewId, recordId);

            RecordDataBatch batch = new RecordDataBatch(new RecordData[] { data });

            string jsonSerializedData = JsonConvert.SerializeObject(batch);

            HttpClientResponse Response = postCommonSharedCode(path, jsonSerializedData);

            RecordSet rsResponse = JsonConvert.DeserializeObject<RecordSet>(Response.Content);

            Record record = (rsResponse != null && rsResponse.Data != null && rsResponse.Data.Count == 1) ?
                new Record(rsResponse.Structure, rsResponse.Data[0])
                : null;

            return record;
        }

         
        /*
         * Talking with John from TrackVia this should work in theory, but generating
         * errors. Need to work with TrackVia to see if this is possible. It would
         * GREATLY speed up the update process
         * 
         */
        /*
        /// <summary>
        /// Updates a record in a view accessible to the authenticated user.
        /// </summary>
        /// <param name="viewId">view identifier in which to update the record</param>
        /// <param name="data">enumerable data instance of RecordData</param>
        /// <returns>RecordSet of updated records</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public RecordSet updateRecords(long viewId, IEnumerable<RecordData> data)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/0", this._baseUriPath, viewId);

            RecordDataBatch batch = new RecordDataBatch(data);

            string jsonSerializedData = JsonConvert.SerializeObject(batch);

            HttpClientResponse Response = postCommonSharedCode(path, jsonSerializedData);

            RecordSet rsResponse = JsonConvert.DeserializeObject<RecordSet>(Response.Content);

            return rsResponse;
        }
        
         */

        /// <summary>
        /// Updates a record in a view accessible to the authenticated user using the typed object.
        /// </summary>
        /// <param name="viewId">view identifier in which to update the record</param>
        /// <param name="recordId">unique record identifier</param>
        /// <param name="data">instance of an application-defined class, representing the record data</param>
        /// <returns></returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public DomainRecord<T> updateRecord<T>(long viewId, long recordId, T data)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/{2}", this._baseUriPath, viewId, recordId);

            DomainRecordDataBatch<T> batch = new DomainRecordDataBatch<T>(new T[] { data });
            string jsonSerializedData = JsonConvert.SerializeObject(batch); 
            
            HttpClientResponse Response = postCommonSharedCode(path, jsonSerializedData);

            DomainRecordSet<T> recordSet = JsonConvert.DeserializeObject<DomainRecordSet<T>>(Response.Content);

            DomainRecord<T> record = (recordSet != null && recordSet.Data != null && recordSet.Data.Count == 1) ?
                new DomainRecord<T>(recordSet.Structure, recordSet.Data[0])
                : null;

            return record;
        }

        private HttpClientResponse postCommonSharedCode(string path, string jsonSerializedData)
        {
            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendPutJsonRequestAsync(url, jsonSerializedData);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);
            return Response;
        }

        #endregion

        #region Public Delete Method

        /// <summary>
        /// Deletes a record in the view of the authenticated user.
        /// </summary>
        /// <param name="viewId">view identifier in which to create the record batch</param>
        /// <param name="recordId">unique record identifier</param>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public void deleteRecord(long viewId, long recordId)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/{2}", this._baseUriPath, viewId, recordId);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendDeleteRequestAsync(url);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);
        }

        #endregion

        #region Public File Methods

        /// <summary>
        /// Adds a file to a record in the view of the authenticated user.
        /// </summary>
        /// <param name="viewId">view identifier in which to modify the record</param>
        /// <param name="recordId">unique record identifier</param>
        /// <param name="fileName">name of the file (named like the corresponding Trackvia "column")</param>
        /// <param name="localFilePath">locally accessible path to the file</param>
        /// <returns>record for the field being updated</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public Record addFile(long viewId, long recordId, string fileName, string localFilePath)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/{2}/files/{3}", this._baseUriPath, viewId, recordId, fileName);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendPostFileRequestAsync(url, System.IO.Path.GetFileName(localFilePath), localFilePath);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            Record record = JsonConvert.DeserializeObject<Record>(Response.Content);

            return record;
        }

        /// <summary>
        /// Gets file contents from a record in a view of the authenticated user.
        /// </summary>
        /// <param name="viewId">view identifier in which to modify the record</param>
        /// <param name="recordId">unique record identifier</param>
        /// <param name="fileName">name of the file (named like the corresponding Trackvia "column")</param>
        /// <param name="localFilePath">locally accessible path to the file</param>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public void getFile(long viewId, long recordId, string fileName, string localFilePath)
        {
            // Overwrites not allowed
            if (System.IO.File.Exists(localFilePath))
            {
                throw new TrackViaClientException(String.Format("Will not overwrite the file {0}; aborting", localFilePath));
            }

            string path = String.Format("{0}/openapi/views/{1}/records/{2}/files/{3}", this._baseUriPath, viewId, recordId, fileName);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendGetFileRequestAsync(url);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            System.IO.File.WriteAllBytes(localFilePath, Response.FileContent);
        }

        /// <summary>
        /// Deletes a file in a view of the authenticated user, if permissible.
        /// </summary>
        /// <param name="viewId">view identifier in which to modify the record</param>
        /// <param name="recordId">unique record identifier</param>
        /// <param name="fileName">name of the file (named like the corresponding Trackvia "column")</param>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public void deleteFile(long viewId, long recordId, string fileName)
        {
            string path = String.Format("{0}/openapi/views/{1}/records/{2}/files/{3}", this._baseUriPath, viewId, recordId, fileName);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendDeleteRequestAsync(url);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);
        }

        #endregion

        #region User Methods

        /// <summary>
        /// Gets account users available to the authenticated user.
        /// </summary>
        /// <param name="start">the index (0 based) of the first user record, useful for paging</param>
        /// <param name="max">retrieve no more than this many user records</param>
        /// <returns>
        /// a list of available users, observing the start and max constraints
        /// </returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public List<User> getUsers(int start, int max)
        {
            string path = String.Format("{0}/openapi/users", this._baseUriPath);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendGetRequestAsync(url);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            UserRecordSet recordSet = JsonConvert.DeserializeObject<UserRecordSet>(Response.Content);

            return recordSet.Data;
        }

        /// <summary>
        /// Creates a new account user.  The user's initial state starts with email confirmation.
        /// </summary>
        /// <param name="email">email address of the user</param>
        /// <param name="firstName">first name of the user</param>
        /// <param name="lastName">last name of the user</param>
        /// <param name="timeZone">abbreviated time zone where the user observes time</param>
        /// <returns>the new user</returns>
        /// <exception cref="TrackViaApiException">if the service fails to process this request</exception>
        /// <exception cref="TrackviaClientException">if an error occurs outside the service, failing the request</exception>
        public User createUser(string email, string firstName, string lastName, string timeZone)
        {
            string path = String.Format("{0}/openapi/users", this._baseUriPath);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = this._scheme.ToString(),
                Host = this._hostName,
                Port = this._port,
                Path = path,
                Query = new UriHelper()
                    .SetParameter(ACCESS_TOKEN_QUERY_PARAM, GetAccessToken())
                    .SetParameter(USER_KEY_QUERY_PARAM, GetApiUserKey())
                    .SetParameter("email", email)
                    .SetParameter("firstname", firstName)
                    .SetParameter("lastname", lastName)
                    .SetParameter("timeZone", timeZone)
                    .Build()
            };

            string url = uriBuilder.ToString();

            Task<HttpClientResponse> Request = _httpClient.SendPostJsonRequestAsync(url, string.Empty);
            Request.Wait();

            HttpClientResponse Response = Request.Result;
            CheckTrackViaApiResponseForErrors(Response);

            UserRecord userRecord = JsonConvert.DeserializeObject<UserRecord>(Response.Content);

            return (userRecord != null) ? (userRecord.Data) : (null);
        }

        #endregion

        #region Implements IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_httpClient != null)
                    _httpClient.Dispose();
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Helps with creating parameters for our HttpClient
        /// </summary>
        private class UriHelper
        {
            private readonly List<KeyValuePair<string, string>> _parameters = new List<KeyValuePair<string, string>>();

            public string Build()
            {
                HttpContent encodedContent = new FormUrlEncodedContent(_parameters);

                string stringResult = encodedContent.ReadAsStringAsync().Result;

                return stringResult;
            }

            public UriHelper SetParameter(string key, string value)
            {
                _parameters.Add(new KeyValuePair<string, string>(key, value));
                return this;
            }
        }

        #endregion
    }
    
}

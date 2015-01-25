using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InfiNet.TrackVia.HttpClient
{

    public interface IAsyncHttpClientHelper : IDisposable
    {
        System.Threading.Tasks.Task<HttpClientResponse> SendGetRequestAsync(string endPoint);
        System.Threading.Tasks.Task<HttpClientResponse> SendPostRequestAsync(string endPoint, ICollection<KeyValuePair<string, string>> parameters);
        System.Threading.Tasks.Task<HttpClientResponse> SendPostFileRequestAsync(string endPoint, string fileName, string filePath);
        System.Threading.Tasks.Task<HttpClientResponse> SendPostJsonRequestAsync(string endPoint, string jsonContent);
        System.Threading.Tasks.Task<HttpClientResponse> SendPutJsonRequestAsync(string endPoint, string jsonContent);
        System.Threading.Tasks.Task<HttpClientResponse> SendDeleteRequestAsync(string endPoint);
        System.Threading.Tasks.Task<HttpClientResponse> SendGetFileRequestAsync(string endPoint);
    }

    public class AsyncHttpClientHelper : IDisposable, IAsyncHttpClientHelper
    {
        private const string APPLICATION_XML_CONTENT_TYPE = "application/xml";
        private const string APPLICATION_JSON_CONTENT_TYPE = "application/json";
        private const string APPLICATION_FILE_CONTENT_TYPE = "application/octet-stream";

        private System.Net.Http.HttpClient _httpClient = null;

        #region Ctors

        public AsyncHttpClientHelper()
            : this(null)
        { }

        public AsyncHttpClientHelper(System.Net.Http.HttpClient httpClient)
        {
            this._httpClient = httpClient ?? new System.Net.Http.HttpClient();
        }

        #endregion

        #region Implements IAsyncClientHelper
        public async Task<HttpClientResponse> SendGetRequestAsync(string endPoint)
        {
            HttpClientResponse sendGetRequestAsyncResult = new HttpClientResponse() { ContentType = HttpClientResponseTypes.unknown };

            HttpResponseMessage response = await _httpClient.GetAsync(endPoint);

            sendGetRequestAsyncResult.Content = await response.Content.ReadAsStringAsync();

            ParseAndUpdateHttpClientResponse(sendGetRequestAsyncResult, endPoint.ToString(), response);

            return sendGetRequestAsyncResult;
        }

        public async Task<HttpClientResponse> SendGetFileRequestAsync(string endPoint)
        {
            HttpClientResponse sendGetRequestAsyncResult = new HttpClientResponse() { ContentType = HttpClientResponseTypes.unknown };

            HttpResponseMessage response = await _httpClient.GetAsync(endPoint);

            sendGetRequestAsyncResult.Content = await response.Content.ReadAsStringAsync();

            sendGetRequestAsyncResult.StatusCode = response.StatusCode;
            sendGetRequestAsyncResult.ContentType = HttpClientResponseTypes.file;
            sendGetRequestAsyncResult.FileContent = response.Content.ReadAsByteArrayAsync().Result;

            return sendGetRequestAsyncResult;
        }

        public async Task<HttpClientResponse> SendPostRequestAsync(string endPoint, ICollection<KeyValuePair<string, string>> formParameters)
        {
            HttpClientResponse sendPostRequestAsyncResult = new HttpClientResponse() { ContentType = HttpClientResponseTypes.unknown };

            FormUrlEncodedContent form = new FormUrlEncodedContent(formParameters);
            string formContentUsedForErrorHandling = await form.ReadAsStringAsync();

            // DDB (2014/12/16): May be necessary if we need to force XML as a result
            //HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, endPoint);
            //requestMessage.Content = new StringContent(string.Empty);
            //requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(APPLICATION_XML_CONTENT_TYPE);
            //HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

            HttpResponseMessage response = await _httpClient.PostAsync(endPoint, form);

            sendPostRequestAsyncResult.Content = await response.Content.ReadAsStringAsync();
            ParseAndUpdateHttpClientResponse(sendPostRequestAsyncResult, formContentUsedForErrorHandling, response);

            return sendPostRequestAsyncResult;
        }

        public async Task<HttpClientResponse> SendPostFileRequestAsync(string endPoint, string fileName, string filePath)
        {
            HttpClientResponse sendPostRequestAsyncResult = new HttpClientResponse() { ContentType = HttpClientResponseTypes.unknown };

            using (System.IO.FileStream readStream = System.IO.File.OpenRead(filePath))
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(CreateFileContent(readStream, fileName, GetContentType(filePath)));

                    HttpResponseMessage response = await _httpClient.PostAsync(endPoint, content);

                    sendPostRequestAsyncResult.Content = await response.Content.ReadAsStringAsync();

                    ParseAndUpdateHttpClientResponse(sendPostRequestAsyncResult, content.ToString(), response);
                }
            }

            return sendPostRequestAsyncResult;
        }

        public async Task<HttpClientResponse> SendPostJsonRequestAsync(string endPoint, string jsonContent)
        {
            HttpClientResponse sendPostRequestAsyncResult = new HttpClientResponse() { ContentType = HttpClientResponseTypes.unknown };
            HttpContent contentPost = new StringContent(jsonContent, Encoding.UTF8, APPLICATION_JSON_CONTENT_TYPE);

            HttpResponseMessage response = await _httpClient.PostAsync(endPoint, contentPost);

            sendPostRequestAsyncResult.Content = await response.Content.ReadAsStringAsync();
            ParseAndUpdateHttpClientResponse(sendPostRequestAsyncResult, jsonContent, response);

            return sendPostRequestAsyncResult;
        }

        public async Task<HttpClientResponse> SendPutJsonRequestAsync(string endPoint, string jsonContent)
        {
            HttpClientResponse sendPostRequestAsyncResult = new HttpClientResponse() { ContentType = HttpClientResponseTypes.unknown };
            HttpContent contentPost = new StringContent(jsonContent, Encoding.UTF8, APPLICATION_JSON_CONTENT_TYPE);

            HttpResponseMessage response = await _httpClient.PutAsync(endPoint, contentPost);

            sendPostRequestAsyncResult.Content = await response.Content.ReadAsStringAsync();
            ParseAndUpdateHttpClientResponse(sendPostRequestAsyncResult, jsonContent, response);

            return sendPostRequestAsyncResult;
        }

        public async Task<HttpClientResponse> SendDeleteRequestAsync(string endPoint)
        {
            HttpClientResponse sendGetRequestAsyncResult = new HttpClientResponse() { ContentType = HttpClientResponseTypes.unknown };

            HttpResponseMessage response = await _httpClient.DeleteAsync(endPoint);

            sendGetRequestAsyncResult.Content = await response.Content.ReadAsStringAsync();

            ParseAndUpdateHttpClientResponse(sendGetRequestAsyncResult, endPoint.ToString(), response);

            return sendGetRequestAsyncResult;
        }
        #endregion
        
        #region Priviate Methods

        /// <summary>
        /// Updates the httpClientResponse parameter by parsing content and media type. If 
        /// necessary, will update the XElement property if the response is Xml.
        /// </summary>
        /// <param name="httpClientResponse"></param>
        /// <param name="formContentUsedForErrorHandling">Used when raising InvalidXmlException</param>
        /// <param name="response">HttpClient response</param>
        private static void ParseAndUpdateHttpClientResponse(HttpClientResponse httpClientResponse, string formContentUsedForErrorHandling, HttpResponseMessage response)
        {
            httpClientResponse.StatusCode = response.StatusCode;
            httpClientResponse.ContentType = HttpClientResponseTypes.unknown;

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                httpClientResponse.ContentType = HttpClientResponseTypes.none;

                // Nothing more to do here
                return;
            }

            httpClientResponse.MediaType = response.Content.Headers.ContentType.MediaType;

            if (string.Equals(response.Content.Headers.ContentType.MediaType, APPLICATION_XML_CONTENT_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                XElement rootNode = null;
                try
                {
                    rootNode = XElement.Parse(httpClientResponse.Content);
                }
                catch (Exception ex)
                {
                    if (rootNode == null)
                    {
                        string invalidContent;
                        try
                        {
                            invalidContent = httpClientResponse.Content;
                        }
                        catch (Exception contentException)
                        {
                            invalidContent = "Unable to read HttpResponse: " + contentException.Message;
                        }

                        throw new InfiNet.TrackVia.Exceptions.InvalidXmlException(ex.Message, ex, formContentUsedForErrorHandling, invalidContent);
                    }
                }

                httpClientResponse.Content = rootNode.ToString();
                httpClientResponse.ContentType = HttpClientResponseTypes.xml;
            }
            else if (string.Equals(response.Content.Headers.ContentType.MediaType, APPLICATION_JSON_CONTENT_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                httpClientResponse.ContentType = HttpClientResponseTypes.json;
            }
        }

        private StreamContent CreateFileContent(System.IO.Stream stream, string fileName, string contentType)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = "\"" + fileName + "\""
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        private string GetContentType(string fileName)
        {

            string contentType = "application/octetstream";

            string ext = System.IO.Path.GetExtension(fileName).ToLower();

            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

            if (registryKey != null && registryKey.GetValue("Content Type") != null)

                contentType = registryKey.GetValue("Content Type").ToString();

            return contentType;

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
    }
}

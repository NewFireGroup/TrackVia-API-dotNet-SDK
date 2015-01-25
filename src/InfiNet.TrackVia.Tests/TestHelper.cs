using InfiNet.TrackVia.HttpClient;
using InfiNet.TrackVia.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InfiNet.TrackVia.Tests
{
    internal static class TestHelper
    {
        public static readonly string HostName_Fake = "go.api.trackvia.com";
        public static readonly string ApiKey_Fake = "myapikey";
        public static readonly string Username_Fake = "myuser";
        public static readonly string Password_Fake = "mypassword";

        public static void EnsureProductionValuesBeforeRunningIntegrationTests()
        {
            EnsureProductionValuesBeforeRunningIntegrationTests(false);
        }

        public static void EnsureProductionValuesBeforeRunningIntegrationTests(bool additionalTestThatMustFailToRunTest)
        {
            if (IntegrationTestConfig.TRACKVIA_API_KEY == string.Empty || IntegrationTestConfig.TRACKVIA_USERNAME == string.Empty 
                || IntegrationTestConfig.TRACKVIA_PASSWORD == string.Empty || additionalTestThatMustFailToRunTest)
                Assert.Inconclusive("set api key, username, and password to run integration tests");
        }

        public static Mock<IAsyncHttpClientHelper> CreateMockHttpAuthorization(OAuth2Token token)
        {
            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(token),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = HttpStatusCode.OK
            });

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();
            httpClient.Setup(x => x
                .SendGetRequestAsync(It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);

            return httpClient;
        }
        
        public static OAuth2Token GetTestAuthToken()
        {
            OAuth2Token token = new OAuth2Token()
            {
                Value = "123",
                Access_Token = "123",
                AccessToken = "123",
                Expiration = DateTime.Now.AddSeconds(60),
                Expires_In = 60,
                ExpiresIn = 60,
                Refresh_Token = "456",
                RefreshToken = new RefreshToken("456", DateTime.Now.AddSeconds(60)),
                Scope = new string[] { "read", "write" },
                TokenType = OAuth2Token.Type.bearer
            };
            return token;
        }

        public static OAuth2Token GetTestRefreshToken()
        {
            OAuth2Token token = new OAuth2Token()
            {
                Value = "789",
                Access_Token = "756",
                AccessToken = "789",
                Expiration = DateTime.Now.AddSeconds(120),
                Expires_In = 60,
                ExpiresIn = 60,
                Refresh_Token = "012",
                RefreshToken = new RefreshToken("012", DateTime.Now.AddSeconds(120)),
                Scope = new string[] { "read", "write" },
                TokenType = OAuth2Token.Type.bearer
            };
            return token;
        }

        public static string GetOAuthJsonResponse()
        {
            DateTime expirationDate = DateTime.Now;
            DateTime refreshExpirationDate = DateTime.Now.AddYears(1);

            return TestData.GetOAuthJsonResponse(refreshExpirationDate, expirationDate);
        }

        public static void HttpClient_SetupGetRequest(HttpStatusCode statusCode, object content, Mock<IAsyncHttpClientHelper> httpClientMock)
        {
            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(content),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = statusCode
            });

            httpClientMock.Setup(x => x
                .SendGetRequestAsync(It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);
        }

        public static void HttpClient_SetupDeleteRequest(HttpStatusCode statusCode, Mock<IAsyncHttpClientHelper> httpClientMock)
        {
            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                StatusCode = statusCode
            });

            httpClientMock.Setup(x => x
                .SendDeleteRequestAsync(It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);
        }

        public static void HttpClient_SetupPostRequest(HttpStatusCode statusCode, object content, Mock<IAsyncHttpClientHelper> httpClientMock)
        {
            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(content),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = statusCode
            });

            httpClientMock.Setup(x => x
                .SendPostRequestAsync(It.IsAny<string>(), It.IsAny<ICollection<KeyValuePair<string, string>>>()))
                .Returns(asyncTaskResult.Task);
        }

        public static void HttpClient_SetupPostJsonRequest(HttpStatusCode statusCode, object content, Mock<IAsyncHttpClientHelper> httpClientMock)
        {
            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(content),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = statusCode
            });

            httpClientMock.Setup(x => x
                .SendPostJsonRequestAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);
        }

        public static void HttpClient_SetupPutJsonRequest(HttpStatusCode statusCode, object content, Mock<IAsyncHttpClientHelper> httpClientMock)
        {
            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(content),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = statusCode
            });

            httpClientMock.Setup(x => x
                .SendPutJsonRequestAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);
        }
    }
}

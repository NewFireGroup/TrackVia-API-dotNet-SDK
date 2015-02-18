using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InfiNet.Testing.Extensions;
using InfiNet.TrackVia.Model;
using InfiNet.TrackVia.HttpClient;
using Moq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using InfiNet.TrackVia.Exceptions;
using System.Collections.Generic;

namespace InfiNet.TrackVia.Tests
{
    [TestClass]
    public partial class TrackViaClient_UnitTests
    {
        [TestMethod]
        public void JsonSanity()
        {
            string JsonText = "{\"Data\":{\"Locations\":[\"CA\"]}}";
            Record foo = JsonConvert.DeserializeObject<Record>(JsonText, new NestedArrayConverter());
            foo.ShouldNotBeNull();
        }

        [TestMethod]
        public void InstantiateClient_AysncConstructor_ShouldNotBeNullAndNotMakeAnyWebRequests()
        {
            //Assemble
            // Act
            TrackViaClient client = new TrackViaClient(TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Assert
            client.ShouldNotBeNull();
        }

        [TestMethod]
        public void InstantiateClient_SimplifiedParameters_ShouldNotBeNullAndShouldAuthorize()
        {
            //Assemble
            OAuth2Token token = TestHelper.GetTestAuthToken();
            Mock<IAsyncHttpClientHelper> httpClient = TestHelper.CreateMockHttpAuthorization(token);

            // Act
            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake,
                TestHelper.Username_Fake, TestHelper.Password_Fake, TestHelper.ApiKey_Fake);

            // Assert
            client.ShouldNotBeNull();
            client.ValidateLastGoodTokenIsEqual(token).ShouldBeTrue("last good token does not match expected result");
        }

        [TestMethod]
        public void InstantiateClient_ExpandedParameters_ShouldNotBeNull()
        {
            //Assemble
            string baseUriPath = "/openapi";
            Scheme scheme = Scheme.https;
            int port = 80;
            OAuth2Token token = TestHelper.GetTestAuthToken();
            Mock<IAsyncHttpClientHelper> httpClient = TestHelper.CreateMockHttpAuthorization(token);

            // Act
            TrackViaClient client = new TrackViaClient(httpClient.Object, baseUriPath, scheme, TestHelper.HostName_Fake, port,
                TestHelper.Username_Fake, TestHelper.Password_Fake, TestHelper.ApiKey_Fake);

            // Assert
            client.ShouldNotBeNull();
        }

        #region Authorization Tests
        
        [TestMethod]
        public void OAuth2Token_JsonResultShouldDeserialize()
        {
            // Assemble
            DateTime expirationDate = DateTime.Now;
            DateTime refreshExpirationDate = DateTime.Now.AddYears(1);

            string jsonResult = TestData.GetOAuthJsonResponse(refreshExpirationDate, expirationDate);

            // Act
            OAuth2Token x = JsonConvert.DeserializeObject<OAuth2Token>(jsonResult);

            // Assert
            x.ShouldNotBeNull();
            x.TokenType.ShouldEqual<OAuth2Token.Type>(OAuth2Token.Type.bearer);
            x.Value.ShouldEqual("abcedefg");
            x.Refresh_Token.ShouldEqual("abcedefghijklpmno");
            x.RefreshToken.ShouldNotBeNull();
            x.RefreshToken.Value.ShouldEqual(x.Refresh_Token);
            x.RefreshToken.Expiration.ToString().ShouldEqual(refreshExpirationDate.ToString());
            x.ExpiresIn.ShouldEqual(900);
            x.Expires_In.ShouldEqual(900);
            x.Expiration.ToString().ShouldEqual(expirationDate.ToString());
            x.Scope.ShouldNotBeNull().Length.ShouldEqual(3);
            x.Access_Token.ShouldEqual("sdfghjklsdfghjkldfghjk");
            x.Access_Token.ShouldEqual(x.AccessToken);
        }

        [TestMethod]
        public void TrackViaClient_Authorize_ShouldDeserializeJsonAndReturnToken()
        {
            // Assemble
            OAuth2Token token = TestHelper.GetTestAuthToken();

            Mock<IAsyncHttpClientHelper> httpClient = TestHelper.CreateMockHttpAuthorization(token);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake,
                TestHelper.Username_Fake, TestHelper.Password_Fake, TestHelper.ApiKey_Fake);

            // Act
            client.Authorize(TestHelper.Username_Fake, TestHelper.Password_Fake);

            // Assert
            client.ValidateLastGoodTokenIsEqual(token).ShouldBeTrue("last good token does not match expected result");
        }

        [TestMethod]
        public async void TrackViaClient_AuthorizeAsync_ShouldDeserializeJsonAndReturnToken()
        {
            // Assemble
            OAuth2Token token = TestHelper.GetTestAuthToken();

            Mock<IAsyncHttpClientHelper> httpClient = TestHelper.CreateMockHttpAuthorization(token);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake,
                TestHelper.Username_Fake, TestHelper.Password_Fake, TestHelper.ApiKey_Fake);

            // Act
            await client.AuthorizeAsync(TestHelper.Username_Fake, TestHelper.Password_Fake);

            // Assert
            client.ValidateLastGoodTokenIsEqual(token).ShouldBeTrue("last good token does not match expected result");
        }

        [TestMethod]
        public void TrackViaClient_Authorized_TestUnauthorized()
        {
            // Assemble

            ApiErrorResponse errorResponse = new ApiErrorResponse();
            errorResponse.Error = ApiError.invalid_grant.code;
            errorResponse.Error_Description = ApiError.invalid_grant.description;

            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(errorResponse),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = HttpStatusCode.Unauthorized
            });

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();
            httpClient.Setup(x => x
                .SendGetRequestAsync(It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            try
            {
                client.Authorize("dontcare", "dontcare");
                Assert.Fail("authorization shouldn't have succeeded");
            }
            catch (TrackViaApiException ex)
            {
                // Assert
                ex.ShouldNotBeNull();
                ex.ApiErrorResponse.ShouldNotBeNull();
                ex.ApiErrorResponse.Error.ShouldEqual(errorResponse.Error);
                ex.ApiErrorResponse.Error_Description.ShouldEqual(errorResponse.Error_Description);
            }


        }

        [TestMethod]
        public void TrackViaClient_RefreshAccessToken_ShouldReturnNewToken()
        {
            // Assemble
            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            // Authorization Token
            OAuth2Token token = TestHelper.GetTestAuthToken();
            TaskCompletionSource<HttpClientResponse> asyncAuthorizeTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncAuthorizeTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(token),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = HttpStatusCode.OK
            });
            httpClient.Setup(x => x
                .SendGetRequestAsync(It.Is<string>(s => s.Contains("grant_type=password"))))
                .Returns(asyncAuthorizeTaskResult.Task);

            // Refresh Token
            OAuth2Token refreshToken = TestHelper.GetTestRefreshToken();
            TaskCompletionSource<HttpClientResponse> asyncRefreshTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncRefreshTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(refreshToken),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = HttpStatusCode.OK
            });
            httpClient.Setup(x => x
                .SendGetRequestAsync(It.Is<string>(s => s.Contains("grant_type=refresh_token"))))
                .Returns(asyncRefreshTaskResult.Task);

            // Act
            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake,
                TestHelper.Username_Fake, TestHelper.Password_Fake, TestHelper.ApiKey_Fake);
            client.Authorize(TestHelper.Username_Fake, TestHelper.Password_Fake);
            client.RefreshAccessToken();

            // Assert
            client.ValidateLastGoodTokenIsEqual(refreshToken).ShouldBeTrue("last good token does not match expected result");
        }

        #endregion

        #region Public Application/View Administration Tests

        [TestMethod]
        public void TrackViaClient_GetApps_ShouldReturnListOfApps()
        {
            // Assemble
            List<App> apps = new List<App>()
            {
                new App("1", "Contact Management - Construction"),
                new App("2", "Contact Management - IT")
            };

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, apps, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            List<App> appsResponse = client.getApps();

            // Assert
            appsResponse.ShouldNotBeNull()
                .Count.ShouldEqual(apps.Count);
            for (int i = 0; i < apps.Count; i++)
            {
                appsResponse[i].ShouldNotBeNull().ShouldEqual(apps[i]);
            }

        }

        [TestMethod]
        public void TrackViaClient_GetViews_ShouldReturnListOfViews()
        {
            // Assemble
            List<View> views = new List<View>()
            {
                new View("1", "Default Contacts View", "Contact Management - IT"),
                new View("2", "Default Activities View", "Contact Management - IT")
            };

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, views, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            List<View> viewsResponse = client.getViews();

            // Assert
            viewsResponse.ShouldNotBeNull()
                .Count.ShouldEqual(views.Count);
            for (int i = 0; i < views.Count; i++)
            {
                viewsResponse[i].ShouldNotBeNull().ShouldEqual(views[i]);
            }

        }

        [TestMethod]
        public void TrackViaClient_GetViewByName_ShouldReturnListOfMatchingView()
        {
            // Assemble
            List<View> views = new List<View>()
            {
                new View("1", "Default Contacts View", "Contact Management - IT"),
                new View("2", "Default Activities View", "Contact Management - IT")
            };

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, views, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            View viewResponse = client.getFirstMatchingView("Default Contacts View");

            // Assert
            viewResponse.ShouldNotBeNull().ShouldEqual(views[0]);
        }

        #endregion

        #region Get Record Tests

        [TestMethod]
        public void TrackViaClient_GetRecords_ShouldReturnListOfRecords()
        {
            // Assemble
            RecordSet rs = TestData.getUnitTestRecordSet1();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, rs, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            RecordSet rsResponse = client.getRecords(1L);

            // Assert
            rsResponse
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);
            rsResponse.Data
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);

            for (int i = 0; i < rsResponse.Count; i++)
            {
                RecordData rd1 = rs.Data[i];
                RecordData rd2 = rsResponse.Data[i];

                rd2.ShouldNotBeNull().ShouldEqual(rd1);
            }

        }

        [TestMethod]
        public void TrackViaClient_GetRecordsAsDomainClass_ShouldReturnListOfRecordsAsType()
        {
            // Assemble
            RecordSet rs = TestData.getUnitTestRecordSet3();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, rs, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            DomainRecordSet<TestData.Contact> rsResponse = client.getRecords<TestData.Contact>(1L);

            // Assert
            rsResponse
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);
            rsResponse.Data
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);
        }

        [TestMethod]
        public void TrackViaClient_GetRecord_ShouldReturnRecord()
        {
            // Assemble
            Record record = TestData.getUnitTestRecord1();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, record, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            Record recordResponse = client.getRecord(1, 1);

            // Assert
            recordResponse.ShouldNotBeNull();
            recordResponse.Data.ShouldNotBeNull();
            recordResponse.Data.Id.ShouldEqual(1L);
        }

        [TestMethod]
        public void TrackViaClient_GetRecordAsDomainClass_ShouldReturnRecordAsType()
        {
            // Assemble
            Record record = TestData.getUnitTestRecord1();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, record, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            DomainRecord<TestData.Contact> contactRecord = client.getRecord<TestData.Contact>(1, 1);

            // Assert
            contactRecord.ShouldNotBeNull();
            contactRecord.Data.ShouldNotBeNull();
            contactRecord.Data.Id.ShouldEqual(record.Data.Id);
        }

        #endregion

        #region Find Record Tests

        [TestMethod]
        public void TrackViaClient_FindRecords_ShouldReturnListOfRecords()
        {
            // Assemble
            string searchCriteria = "dontcare";
            int startIndex = 0;
            int maxRecords = 25;

            RecordSet rs = TestData.getUnitTestRecordSet1();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();
            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(rs),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = HttpStatusCode.OK
            });
            httpClient.Setup(x => x
                .SendGetRequestAsync(It.Is<string>(s => s.Contains("q=" + searchCriteria) && s.Contains("start=" + startIndex) 
                    && s.Contains("max=" + maxRecords))))
                .Returns(asyncTaskResult.Task);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            RecordSet rsResponse = client.findRecords(1L, searchCriteria, startIndex, maxRecords);

            // Assert
            rsResponse
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);
            rsResponse.Data
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);

            for (int i = 0; i < rsResponse.Count; i++)
            {
                RecordData rd1 = rs.Data[i];
                RecordData rd2 = rsResponse.Data[i];

                rd2.ShouldNotBeNull().ShouldEqual(rd1);
            }

        }

        #endregion

        #region Create Record Tests

        [TestMethod]
        public void TrackViaClient_CreateRecordBatch_ShouldCreatedRecords()
        {
            // Assemble
            RecordSet rs = TestData.getUnitTestRecordSet3();
            RecordDataBatch batch = new RecordDataBatch(rs.Data);

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupPostJsonRequest(HttpStatusCode.Created, rs, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            RecordSet rsResponse = client.createRecords(1L, batch);

            // Assert
            rsResponse
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);
            rsResponse.Data
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);

            for (int i = 0; i < rsResponse.Count; i++)
            {
                RecordData rd1 = rs.Data[i];
                RecordData rd2 = rsResponse.Data[i];

                rd2.Id.ShouldEqual(rd1.Id);
            }
        }

        [TestMethod]
        public void TrackViaClient_CreateRecordBatchAsDomainClass_ShouldCreatedRecords()
        {
            // Assemble
            Record rawRecord = TestData.getUnitTestRecord1();
            TestData.Contact contact = TestData.getUnitTestContact1();
            List<TestData.Contact> contacts = new List<TestData.Contact>(new TestData.Contact[] { contact });
            DomainRecordSet<TestData.Contact> rs = new DomainRecordSet<TestData.Contact>(rawRecord.Structure, contacts);

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupPostJsonRequest(HttpStatusCode.Created, rs, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            DomainRecordDataBatch<TestData.Contact> batch = new DomainRecordDataBatch<TestData.Contact>(contacts);

            // Act
            DomainRecordSet<TestData.Contact> rsResponse = client.createRecords<TestData.Contact>(1L, batch);

            // Assert
            rsResponse
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);
            rsResponse.Data
                .ShouldNotBeNull()
                .Count.ShouldEqual(rs.Count);

        }

        #endregion

        #region Update Record Tests

        [TestMethod]
        public void TrackViaClient_UpdateRecord_ShouldUpdateRecordAndReturn()
        {
            // Assemble
            RecordSet rs = TestData.getUnitTestRecordSet2();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupPutJsonRequest(HttpStatusCode.OK, rs, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            Record updatedRecord = client.updateRecord(1L, 1L, rs.Data[0]);

            // Assert
            updatedRecord.ShouldNotBeNull();
            updatedRecord.Data.ShouldNotBeNull();
            updatedRecord.Data.ShouldEqual(rs.Data[0]);
        }

        [TestMethod]
        public void TrackViaClient_UpdateRecordAsDomainClass_ShouldUpdateRecordAndReturn()
        {
            // Assemble
            RecordSet rs = TestData.getUnitTestRecordSet3();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupPutJsonRequest(HttpStatusCode.OK, rs, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            TestData.Contact contact = TestData.getUnitTestContact1();

            // Act
            DomainRecord<TestData.Contact> responseRecord = client.updateRecord<TestData.Contact>(1L, contact.Id, contact);

            // Assert
            responseRecord.ShouldNotBeNull();
            responseRecord.Data.ShouldNotBeNull();
            responseRecord.Data.Id.ShouldEqual(contact.Id);
        }

        #endregion

        #region Delete Record Test

        [TestMethod]
        public void TrackViaClient_DeleteRecord_ShouldNotBlowUp()
        {
            // Assemble
            RecordSet rs = TestData.getUnitTestRecordSet2();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupDeleteRequest(HttpStatusCode.NoContent, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            client.deleteRecord(1L, 1L);
        }

        #endregion

        #region File Tests

        [TestMethod]
        public void TrackViaClient_AddFile_ShouldReturnUpdatedRecord()
        {
            // Assemble
            string tempFilePath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFilePath, "This is only a test");

            Record record = TestData.getUnitTestRecord1();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = JsonConvert.SerializeObject(record),
                ContentType = HttpClientResponseTypes.json,
                StatusCode = HttpStatusCode.OK
            });

            httpClient.Setup(x => x
                .SendPostFileRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            Record updatedRecord = client.addFile(1L, 1L, "Test File", tempFilePath);

            // Assert
            updatedRecord.ShouldNotBeNull();
            updatedRecord.Data.ShouldNotBeNull();
            updatedRecord.Data[RecordData.INTERNAL_ID_FIELD_NAME].ShouldEqual(record.Data[RecordData.INTERNAL_ID_FIELD_NAME]);
        }

        [TestMethod]
        public void TrackViaClient_GetFile_ShouldReturnFileBytes()
        {
            // Assemble
            string tempFilePath = System.IO.Path.GetTempFileName();
            System.IO.File.Delete(tempFilePath);

            string fileContent = "This is only a test";
            byte[] fileData = System.Text.Encoding.UTF8.GetBytes(fileContent);
            
            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = Convert.ToBase64String(fileData),
                FileContent = fileData,
                ContentType = HttpClientResponseTypes.file,
                StatusCode = HttpStatusCode.OK
            });

            httpClient.Setup(x => x
                .SendGetFileRequestAsync(It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            client.getFile(1L, 1L, "Test File", tempFilePath);

            // Assert
            System.IO.File.Exists(tempFilePath).ShouldBeTrue();
            System.IO.File.ReadAllText(tempFilePath).ShouldEqual(fileContent);
            System.IO.File.Delete(tempFilePath);
        }

        [TestMethod]
        public void TrackViaClient_DeleteFile_ShouldReturnSuccess()
        {
            // Assemble
            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TaskCompletionSource<HttpClientResponse> asyncTaskResult = new TaskCompletionSource<HttpClientResponse>();
            asyncTaskResult.SetResult(new HttpClientResponse()
            {
                Content = null,
                ContentType = HttpClientResponseTypes.none,
                StatusCode = HttpStatusCode.NoContent
            });

            httpClient.Setup(x => x
                .SendDeleteRequestAsync(It.IsAny<string>()))
                .Returns(asyncTaskResult.Task);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            client.deleteFile(1L, 1L, "Test File");

            // Assert
        }

        #endregion

        #region User Methods

        [TestMethod]
        public void TrackViaClient_GetUsers_ShouldReturnListOfUsers()
        {
            // Assemble
            UserRecordSet userRecordSet = TestData.getUnitTestUserRecordSet1();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupGetRequest(HttpStatusCode.OK, userRecordSet, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            List<User> usersResponse = client.getUsers(0, 25);

            // Assert
            usersResponse
                .ShouldNotBeNull()
                .Count.ShouldEqual(userRecordSet.Count);
            usersResponse[0]
                .ShouldEqual(userRecordSet.Data[0]);
        }


        [TestMethod]
        public void TrackViaClient_CreateUser_ShouldReturnUser()
        {
            // Assemble
            UserRecord record = TestData.getUnitTestUserRecord1();

            Mock<IAsyncHttpClientHelper> httpClient = new Mock<IAsyncHttpClientHelper>();

            TestHelper.HttpClient_SetupPostJsonRequest(HttpStatusCode.OK, record, httpClient);

            TrackViaClient client = new TrackViaClient(httpClient.Object, TestHelper.HostName_Fake, TestHelper.ApiKey_Fake);

            // Act
            User userReponse = client.createUser(record.Data.Email, record.Data.FirstName, record.Data.LastName,
                record.Data.TimeZone);

            // Assert
            userReponse
                .ShouldNotBeNull()
                .ShouldEqual(record.Data);
        }

        #endregion
    }
}

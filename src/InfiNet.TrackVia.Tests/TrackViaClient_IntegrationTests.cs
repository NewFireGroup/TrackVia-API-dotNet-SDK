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
    public partial class TrackViaClient_IntegrationTests
    {
        #region Authorization Tests

        [TestMethod]
        public void IntegrationTest_TrackViaClient_CompletePasswordAuthorization()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests();

            // Assemble

            // Act
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME, 
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Assert 
            client.ShouldNotBeNull();
            client.ValidateLastGoodTokenHasNotExpired(DateTime.Now).ShouldBeTrue("last good token should still be valid");
            client.ValidateAccessTokenIsPresent().ShouldBeTrue("access token is not present");
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_CompleteRefreshToken()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests();

            // Assemble

            // Act
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            client.RefreshAccessToken();

            // Assert 
            client.ShouldNotBeNull();
            client.ValidateLastGoodTokenHasNotExpired(DateTime.Now).ShouldBeTrue("last good token should still be valid");
            client.ValidateAccessTokenIsPresent().ShouldBeTrue("access token is not present");
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_ShouldFailWithUnauthorized()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests();

            // Assemble

            // Act
            try
            {
                TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, "dontcare", "dontcare", IntegrationTestConfig.TRACKVIA_API_KEY);
                Assert.Fail("authorization shouldn't have succeeded");
            }
            catch (TrackViaApiException ex)
            {
                // Assert
                ex.ShouldNotBeNull();
                ex.ApiErrorResponse.ShouldNotBeNull();
                ex.ApiErrorResponse.Error.ShouldEqual(ApiError.bad_credentials.code);
                ex.ApiErrorResponse.Error_Description.ShouldEqual(ApiError.bad_credentials.description);
            }
        }

        #endregion

        #region Public Application/View Administration Tests

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetAppList()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_WITHATLEASTTWORECORDS <= 0);

            // Assemble

            // Act
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME, 
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);
            RecordSet rsResult = client.getRecords(IntegrationTestConfig.TRACKVIA_VIEWID_WITHATLEASTTWORECORDS);

            // Assert 
            rsResult
                .ShouldNotBeNull()
                .Count.ShouldBeGreaterThan(1);
            rsResult.Data
                .ShouldNotBeNull()
                .Count.ShouldEqual(rsResult.Count);

            for (int i = 0; i < rsResult.Count; i++)
            {
                RecordData rd2 = rsResult.Data[i];

                rd2.ShouldNotBeNull();
            }
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetViewList()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests();

            // Assemble

            // Act
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME, 
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);
            var viewsResult = client.getViews();

            // Assert 
            viewsResult.ShouldNotBeNull().ShouldNotBeEmpty();
            viewsResult.Count.ShouldBeGreaterThan(1);
            for (int i = 0; i < viewsResult.Count; i++)
            {
                var viewUnderTest = viewsResult[i];
                viewUnderTest.ShouldNotBeNull();
                viewUnderTest.ApplicationName.ShouldNotBeEmpty();
                viewUnderTest.Id.ShouldNotBeEmpty();
                viewUnderTest.Name.ShouldNotBeEmpty();
            }
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetViewListMatchingName()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(string.IsNullOrWhiteSpace(
                IntegrationTestConfig.TRACKVIA_VIEWNAME_THATMATCHES_ATLEASTTWOVIEWS));

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            var viewsResult = client.getViews("My Accounts");

            // Assert 
            viewsResult.ShouldNotBeNull().ShouldNotBeEmpty();
            viewsResult.Count.ShouldBeGreaterThan(1);
            for (int i = 0; i < viewsResult.Count; i++)
            {
                var viewUnderTest = viewsResult[i];
                viewUnderTest.ShouldNotBeNull();
                viewUnderTest.ApplicationName.ShouldNotBeEmpty();
                viewUnderTest.Id.ShouldNotBeEmpty();
                viewUnderTest.Name.ShouldNotBeEmpty();
                Assert.IsTrue(viewUnderTest.Name.Contains("My Accounts"), "returned view does not contain passed string");
            }
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetSingleView()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(string.IsNullOrWhiteSpace(
               IntegrationTestConfig.TRACKVIA_VIEWNAME_THATMATCHES_ATLEASTONEVIEW));

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            View viewResult = client.getFirstMatchingView("My Accounts");

            // Assert 
            viewResult.ShouldNotBeNull();
                viewResult.ApplicationName.ShouldNotBeEmpty();
                viewResult.Id.ShouldNotBeEmpty();
                viewResult.Name.ShouldEqual("My Accounts");
        }

        #endregion

        #region Get Record Tests

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetRecords()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME, 
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            RecordSet rsResult = client.getRecords(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW);

            // Assert 
            rsResult.ShouldNotBeNull();
            rsResult.Data.ShouldNotBeNull()
                .ShouldNotBeEmpty();
            rsResult.Count.ShouldBeGreaterThan(1);
            for (int i = 0; i < rsResult.Count; i++)
            {
                var recordUnderTest = rsResult.Data[i];
                recordUnderTest.ShouldNotBeNull();
                recordUnderTest.Id.ShouldBeGreaterThan(0);
            }
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetDomainRecords_SimpleCrmAccounts()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME, 
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            DomainRecordSet<TestData.SimpleCrmContact> domainRecordSet = client.getRecords<TestData.SimpleCrmContact>(
                IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW);

            // Assert 
            domainRecordSet.ShouldNotBeNull()
                .Data.ShouldNotBeNull().ShouldNotBeEmpty();
            domainRecordSet.Count.ShouldBeGreaterThan(1);
            domainRecordSet.Data.Count.ShouldEqual(domainRecordSet.Count);

            for (int i = 0; i < domainRecordSet.Count; i++)
            {
                TestData.SimpleCrmContact Account = domainRecordSet.Data[i];
                Account.ShouldNotBeNull();
                Account.Id.ShouldBeGreaterThan(0);
                Account.AccountName.ShouldNotBeNull().ShouldNotBeEmpty();
                Account.PrimaryContact.ShouldNotBeNull().ShouldNotBeEmpty();
            }
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetRecord_SimpleCrmAccount()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0
                || IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW_VALIDACCOUNTRECORDID <= 0);

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME, 
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            Record record = client.getRecord(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW, 
                IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW_VALIDACCOUNTRECORDID);

            // Assert 
            record.ShouldNotBeNull();
            record.Data.ShouldNotBeNull();
            record.Data.Id.ShouldEqual(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW_VALIDACCOUNTRECORDID);
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetDomainRecord_SimpleCrmAccount()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0
                || IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW_VALIDACCOUNTRECORDID <= 0);

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            DomainRecord<TestData.SimpleCrmContact> record = client.getRecord<TestData.SimpleCrmContact>(
                IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW,
                IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW_VALIDACCOUNTRECORDID);

            // Assert 
            record.ShouldNotBeNull();
            record.Data.ShouldNotBeNull();
            record.Data.Id.ShouldEqual(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW_VALIDACCOUNTRECORDID);
        }

        #endregion

        #region Find Record Tests

        [TestMethod]
        public void IntegrationTest_TrackViaClient_FindRecords_ShouldReturnListOfRecords()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            // Assemble
            string searchCriteria = "A";
            int startIndex = 0;
            int maxRecords = 2;

            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            RecordSet rsResult = client.findRecords(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW,
                searchCriteria, startIndex, maxRecords);

            // Assert 
            rsResult.ShouldNotBeNull();
            rsResult.Data.ShouldNotBeNull()
                .ShouldNotBeEmpty();
            rsResult.Count.ShouldEqual(2);
            for (int i = 0; i < rsResult.Count; i++)
            {
                var recordUnderTest = rsResult.Data[i];
                recordUnderTest.ShouldNotBeNull();
                recordUnderTest.Id.ShouldBeGreaterThan(0);
            }

        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_FindDomainRecords_ShouldReturnListOfRecords()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            // Assemble
            string searchCriteria = "A";
            int startIndex = 0;
            int maxRecords = 2;

            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            DomainRecordSet<TestData.SimpleCrmContact> domainRecordSet = client.findRecords<TestData.SimpleCrmContact>(
                IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW,
                searchCriteria, startIndex, maxRecords);

            // Assert 
            domainRecordSet.ShouldNotBeNull();
            domainRecordSet.Data.ShouldNotBeNull()
                .ShouldNotBeEmpty();
            domainRecordSet.Count.ShouldEqual(2);
            // Assert 

            for (int i = 0; i < domainRecordSet.Count; i++)
            {
                TestData.SimpleCrmContact Account = domainRecordSet.Data[i];
                Account.ShouldNotBeNull();
                Account.Id.ShouldBeGreaterThan(0);
                Account.AccountName.ShouldNotBeNull().ShouldNotBeEmpty();
                Account.PrimaryContact.ShouldNotBeNull().ShouldNotBeEmpty();
            }

        }

        #endregion

        #region File Tests

        [TestMethod]
        public void IntegrationTest_TrackViaClient_AddFile_ShouldReturnUpdatedRecord()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            // Assemble
            string tempFilePath = IntegrationTestConfig.IMAGE_FILE_FULL_PATH_FOR_UPLOAD; // System.IO.Path.GetTempFileName();

            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            Record updatedRecord = client.addFile(IntegrationTestConfig.TRACKVIA_VIEWID_INCLUDESFILEFIELD,
                IntegrationTestConfig.TRACKVIA_VIEWID_INCLUDESFILEFIELD_RECORDID,
                IntegrationTestConfig.TRACKVIA_VIEWID_INCLUDESFILEFIELD_FIELDNAME, tempFilePath);

            // Assert
            updatedRecord.ShouldNotBeNull();
            updatedRecord.Data.ShouldNotBeNull();
            updatedRecord.Data[IntegrationTestConfig.TRACKVIA_VIEWID_INCLUDESFILEFIELD_FIELDNAME].ShouldNotBeNull();
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetFile_ShouldReturnUpdatedRecord()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            // Assemble
            string tempFilePath = System.IO.Path.GetTempFileName();
            System.IO.File.Delete(tempFilePath);

            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            client.getFile(IntegrationTestConfig.TRACKVIA_VIEWID_INCLUDESFILEFIELD,
                IntegrationTestConfig.TRACKVIA_VIEWID_INCLUDESFILEFIELD_RECORDID,
                IntegrationTestConfig.TRACKVIA_VIEWID_INCLUDESFILEFIELD_FIELDNAME, tempFilePath);

            // Assert
            System.IO.File.Exists(tempFilePath).ShouldBeTrue();
            System.IO.File.ReadAllBytes(tempFilePath).Length.ShouldBeGreaterThan(0);
            System.IO.File.Delete(tempFilePath);
        }

        #endregion

        #region User Tests

        [TestMethod]
        public void IntegrationTest_TrackViaClient_GetUsers()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests();

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            List<User> users = client.getUsers(0, 25);

            // Assert 
            users.ShouldNotBeNull();
            users.Count.ShouldBeGreaterThan(1);
        }

        #endregion

        #region Integration Tests (Manual)

        [TestMethod]
        public void IntegrationTest_TrackViaClient_CreateRecords_SimpleCRMAccount()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests();

            // Assemble
            RecordData recordData = TestData.IntegrationTest_SimpleCrmContact_GetCreateRecordData();
            RecordDataBatch rsBatch = new RecordDataBatch(new RecordData[] { recordData });

            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME, 
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Act
            RecordSet rsResponse = client.createRecords(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW, rsBatch);

            // Assert 
            rsResponse.ShouldNotBeNull();
            rsResponse.Data.ShouldNotBeNull();
            rsResponse.Count.ShouldEqual(1);
            rsResponse.Data[0].ShouldNotBeNull();
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_CreateDomainRecords_SimpleCrmAccounts()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            // Assemble
            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            TestData.SimpleCrmContact contact = TestData.IntegrationTest_SimpleCrmContact_GetCreate();

            DomainRecordDataBatch<TestData.SimpleCrmContact> batch = new DomainRecordDataBatch<TestData.SimpleCrmContact>(new TestData.SimpleCrmContact[] {
                contact
            });

            // Act
            DomainRecordSet<TestData.SimpleCrmContact> domainRecordSet = client.createRecords<TestData.SimpleCrmContact>(
                IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW, batch);

            // Assert 
            domainRecordSet.ShouldNotBeNull()
                .Data.ShouldNotBeNull().ShouldNotBeEmpty();
            domainRecordSet.Count.ShouldEqual(1);
            domainRecordSet.Data.Count.ShouldEqual(domainRecordSet.Count);

            for (int i = 0; i < domainRecordSet.Count; i++)
            {
                TestData.SimpleCrmContact Account = domainRecordSet.Data[i];
                Account.ShouldNotBeNull();
                Account.Id.ShouldBeGreaterThan(0);
                Account.AccountName.ShouldNotBeNull().ShouldEqual(contact.AccountName);
                Account.PrimaryContact.ShouldNotBeNull().ShouldEqual(contact.PrimaryContact);
                Account.ContactPhone.ShouldNotBeNull().ShouldEqual(contact.ContactPhone);
                Account.ContactEmail.ShouldNotBeNull().ShouldEqual(contact.ContactEmail);
                Account.Address.ShouldNotBeNull().ShouldEqual(contact.Address);
                Account.City.ShouldNotBeNull().ShouldEqual(contact.City);
                Account.State.ShouldNotBeNull().ShouldEqual(contact.State);
                Account.ZipCode.ShouldNotBeNull().ShouldEqual(contact.ZipCode);
            }
        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_Scenario_CreateAndUpdateRecord_SimpleCRMAccount()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);
           
            // Create a record we can update
            Record record = Integration_CreateRecordStep(client);

            // Lets leave one field unchanged, update one field and add a new field value
            RecordData updatedData = new RecordData();
            updatedData.Add("Primary Contact", "Updated Primary Contact");
            updatedData.Add("Contact Phone", "555-555-5555");

            // Act
            Record updatedResult = client.updateRecord(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW,
                record.Data.Id, updatedData);

            // Assert
            updatedResult.ShouldNotBeNull();
            updatedResult.Data.ShouldNotBeNull();
            updatedResult.Data.Id.ShouldEqual(record.Data.Id);
            updatedResult.Data["Account Name"].ShouldEqual(record.Data["Account Name"]);
            updatedResult.Data["Primary Contact"].ShouldEqual(updatedData["Primary Contact"]);
            updatedResult.Data["Contact Phone"].ShouldEqual(updatedData["Contact Phone"]);

        }

        [TestMethod]
        public void IntegrationTest_TrackViaClient_Scenario_CreateAndUpdateDeleteRecord_SimpleCRMAccount()
        {
            TestHelper.EnsureProductionValuesBeforeRunningIntegrationTests(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW <= 0);

            TrackViaClient client = new TrackViaClient(IntegrationTestConfig.TRACKVIA_HOSTNAME, IntegrationTestConfig.TRACKVIA_USERNAME,
                IntegrationTestConfig.TRACKVIA_PASSWORD, IntegrationTestConfig.TRACKVIA_API_KEY);

            // Create a record we can update
            Record record = Integration_CreateRecordStep(client);

            // Lets leave one field unchanged, update one field and add a new field value
            Record updatedRecord = Integration_UpdateRecordStep(client, record);

            client.deleteRecord(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW,
                updatedRecord.Data.Id);

        }

        #endregion

        #region Scenario Testing Shared Steps

        private static Record Integration_UpdateRecordStep(TrackViaClient client, Record record)
        {
            RecordData updateData = new RecordData();
            updateData.Add("Primary Contact", "Updated Primary Contact");
            updateData.Add("Contact Phone", "555-555-5555");

            // Act
            Record updatedRecord = client.updateRecord(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW,
                record.Data.Id, updateData);

            // Assert
            updatedRecord.ShouldNotBeNull();
            updatedRecord.Data.ShouldNotBeNull();
            updatedRecord.Data.Id.ShouldEqual(record.Data.Id);
            updatedRecord.Data["Account Name"].ShouldEqual(record.Data["Account Name"]);
            updatedRecord.Data["Primary Contact"].ShouldEqual(updateData["Primary Contact"]);
            updatedRecord.Data["Contact Phone"].ShouldEqual(updateData["Contact Phone"]);


            return updatedRecord;
        }

        private static Record Integration_CreateRecordStep(TrackViaClient client)
        {
            RecordData recordData = TestData.IntegrationTest_SimpleCrmContact_GetCreateRecordData();
            RecordDataBatch rsBatch = new RecordDataBatch(new RecordData[] { recordData });


            // Act
            RecordSet rsResponse = client.createRecords(IntegrationTestConfig.TRACKVIA_VIEWID_DEMOSIMPLECRM_ACCOUNTSDEFAULTVIEW, rsBatch);

            // Assert 
            rsResponse.ShouldNotBeNull();
            rsResponse.Count.ShouldEqual(1);
            rsResponse.Data.ShouldNotBeNull();
            rsResponse.Data[0].Id.ShouldBeGreaterThan(0);
            rsResponse.Data[0].ShouldNotBeNull();

            return new Record(rsResponse.Structure, rsResponse.Data[0]);
        }

        #endregion
    }
}

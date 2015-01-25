using InfiNet.TrackVia.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfiNet.TrackVia.Tests
{
    public static class TestData
    {
        public static string GetOAuthJsonResponse(DateTime refreshExpiration, DateTime expirationDate)
        {
            string Result = @"
                    {
                      ""value"": ""abcedefg"",
                      ""tokenType"": ""bearer"",
                      ""refreshToken"": {
                        ""value"": ""abcedefghijklpmno"",
                        ""expiration"": """ + refreshExpiration.ToString() + @"""
                      },
                      ""expires_in"": 900,
                      ""expiresIn"": 900,
                      ""expiration"": """ + expirationDate.ToString() + @""",
                      ""scope"": [
                        ""read"",
                        ""trust"",
                        ""write""
                      ],
                      ""pusher_application_key"": ""qwertyuio123456789"",
                      ""access_token"": ""sdfghjklsdfghjkldfghjk"",
                      ""accessToken"": ""sdfghjklsdfghjkldfghjk"",
                      ""refresh_token"": ""abcedefghijklpmno""
                    }
                ";

            return Result;
        }

        public static RecordSet getUnitTestRecordSet1()
        {
            List<FieldMetadata> structure = new List<FieldMetadata>(new FieldMetadata[] {
                    new FieldMetadata(RecordData.INTERNAL_ID_FIELD_NAME, TrackViaDataType.Number, true, false, new string[] { }),
                    new FieldMetadata("ContactName", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("CompanyName", TrackViaDataType.ShortAnswer, true, false, new string[] { })
            });
            List<RecordData> data = new List<RecordData>(new RecordData[]{new RecordData(), new RecordData()});

            data[0].Add(RecordData.INTERNAL_ID_FIELD_NAME, 1L);
            data[0].Add("ContactName", "James Randall");
            data[0].Add("CompanyName", "Cryogenic Futures");

            data[1].Add(RecordData.INTERNAL_ID_FIELD_NAME, 2L);
            data[1].Add("ContactName", "Simon Black");
            data[1].Add("CompanyName", "Sunshine Industries");

            return new RecordSet(structure, data);

        }

        internal static RecordSet getUnitTestRecordSet2()
        {
            List<FieldMetadata> structure = new List<FieldMetadata>(new FieldMetadata[] {
                    new FieldMetadata(RecordData.INTERNAL_ID_FIELD_NAME, TrackViaDataType.Number, true, false, new string[] { }),
                    new FieldMetadata("ContactName", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("CompanyName", TrackViaDataType.ShortAnswer, true, false, new string[] { })
            });
            List<RecordData> data = new List<RecordData>(new RecordData[] { new RecordData()});


            data[0].Add(RecordData.INTERNAL_ID_FIELD_NAME, 1L);
            data[0].Add("ContactName", "James Randall");
            data[0].Add("CompanyName", "Cryogenic Futures");

            return new RecordSet(structure, data);
        }

        public static RecordSet getUnitTestRecordSet3()
        {
            Record record = getUnitTestRecord1();
            List<RecordData> data = new List<RecordData>();
            data.Add(record.Data);

            RecordSet rs = new RecordSet(record.Structure, data);

            return rs;
        }

        public static Record getUnitTestRecord1()
        {
            List<FieldMetadata> structure = new List<FieldMetadata>(new FieldMetadata[] {
                new FieldMetadata(RecordData.INTERNAL_ID_FIELD_NAME, TrackViaDataType.Number, true, false, new string[] { }),
                    new FieldMetadata("ContactName", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("CompanyName", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("Locations", TrackViaDataType.CheckBox, true, false, new string[]{"CO", "CA"}),
                    new FieldMetadata("IsCustomer", TrackViaDataType.Number, true, false, new string[] { }),
                    new FieldMetadata("Revenue", TrackViaDataType.Currency, true, false, new string[] { }),
                    new FieldMetadata("RevenueCaptured", TrackViaDataType.Percentage, true, false, new string[] { }),
                    new FieldMetadata("TestFile", TrackViaDataType.Document, true, false, new string[] { }),
                    new FieldMetadata("LastContactDate", TrackViaDataType.Date, true, false, new string[] { })
            });

            RecordData data = new RecordData();
            data.Add(RecordData.INTERNAL_ID_FIELD_NAME, 1L);
            data.Add("ContactName", "James Randall");
            data.Add("CompanyName", "Cryogenic Futures");
            data.Add("Locations", new String[] { "CA" });
            data.Add("IsCustomer", true);
            data.Add("Revenue", 100000.0);
            data.Add("RevenueCaptured", 0.35);
            data.Add("TestFile", 222L);
            data.Add("LastContactDate", DateTime.Now.ToString());

            return new Record(structure, data);
        }

        public static Contact getUnitTestContact1()
        {
            return new Contact()
            {
                CompanyName = "Cryogenic Futures",
                ContactName = "James Randall",
                Id = 1L,
                IsCustomer = true,
                LastContactDate = DateTime.Now,
                Locations = new List<string>(new string[] { "CA" }),
                Revenue = 100000.0d,
                RevenueCaptured = 10000.0d,
                TestFile = 222L
            };
        }

        public static SimpleCrmContact IntegrationTest_SimpleCrmContact_GetCreate()
        {
            return new SimpleCrmContact()
            {
                AccountName = "Integration Test",
                PrimaryContact = "John Smith",
                ContactPhone = "555-555-5555",
                ContactEmail = "jsmith@integrationtest.com",
                Address = "1234 S Baker ST",
                City = "Fort Collins",
                State = "Colorado",
                ZipCode = "80521"
            };
        }

        public static RecordData IntegrationTest_SimpleCrmContact_GetCreateRecordData()
        {
            RecordData result = new RecordData();
            result.Add("Account Name", "Integration Test");
            result.Add("Primary Contact", "Test User");

            return result;
        }

        #region User Methods

        public static UserRecordSet getUnitTestUserRecordSet1()
        {
            List<FieldMetadata> structure = getUnitTestUserFieldMetaData1();
            User u1 = getUnitTestUser1();
            List<User> data = new List<User>(new User[] { u1 });

            return new UserRecordSet(structure, data);
        }

        public static UserRecord getUnitTestUserRecord1()
        {
            return new UserRecord(getUnitTestUserFieldMetaData1(), getUnitTestUser1());
        }


        public static List<FieldMetadata> getUnitTestUserFieldMetaData1()
        {
            return new List<FieldMetadata>(new FieldMetadata[]{
                    new FieldMetadata(RecordData.INTERNAL_ID_FIELD_NAME, TrackViaDataType.Number, true, false, new string[] { }),
                    new FieldMetadata("FirstName", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("LastName", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("Status", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("Email", TrackViaDataType.Email, true, false, new string[] { }),
                    new FieldMetadata("Timezone", TrackViaDataType.ShortAnswer, true, false, new string[] { }),
                    new FieldMetadata("Created", TrackViaDataType.DateTime, true, false, new string[] { }),
            });
        }

        public static User getUnitTestUser1()
        {
            return new User("Joe", "Black", "ACTIVE", "joe@gmail.com", "MST", DateTime.Now);
        }

        #endregion

        #region nested classes

        public class Contact : IIdentifiable
        {
            public Contact()
            {
                this.Locations = new List<string>();
            }

            public long Id { get; set; }
            public string ContactName { get; set; }
            public string CompanyName { get; set; }
            public bool IsCustomer { get; set; }
            public double Revenue { get; set; }
            public double RevenueCaptured { get; set; }
            public long TestFile { get; set; }
            public DateTime LastContactDate { get; set; }
            public List<string> Locations { get; set; }

        }

        public class SimpleCrmContact : IIdentifiable
        {
            public long Id { get; set; }

            [JsonProperty("Account Name")]
            public string AccountName { get; set; }
            
            [JsonProperty("Primary Contact")]
            public string PrimaryContact { get; set; }
            
            [JsonProperty("Contact Phone")]
            public string ContactPhone { get; set; }
            
            [JsonProperty("Contact Email")]
            public string ContactEmail { get; set; }
            
            public string Address { get; set; }
            
            public string City { get; set; }
            
            public string State { get; set; }

            [JsonProperty("Zip Code")]
            public string ZipCode { get; set; }
        }

        #endregion
    }
}

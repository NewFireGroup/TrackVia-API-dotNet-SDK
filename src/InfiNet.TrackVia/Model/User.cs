using Newtonsoft.Json;
using System;

namespace InfiNet.TrackVia.Model
{
    public class User
    {
        public User()
        { }

        public User(string firstName, string lastName, string status, string email,
            string timeZone, DateTime created)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Status = status;
            this.Email = email;
            this.TimeZone = timeZone;
            this.Created = created;
        }

        public long Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        public string Status { get; set; }
        [JsonProperty("Time Zone")]
        public string TimeZone { get; set; }
        public DateTime Created { get; set; }



        #region override Equals

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is User)) return false;

            User otherUser = (User)obj;

            if (!(otherUser.Id == Id))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + (int) Id;
            return result;
        }

        #endregion
    }
}

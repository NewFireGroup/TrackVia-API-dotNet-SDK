using System;

namespace InfiNet.TrackVia.Model
{
    public class RefreshToken
    {
        public RefreshToken() { }
        public RefreshToken(string value, DateTime expiration)
        {
            this.Value = value;
            this.Expiration = expiration;
        }

        public string Value { get; set; }
        public DateTime Expiration { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is RefreshToken)) return false;

            RefreshToken otherToken = (RefreshToken)obj;

            if (otherToken.Value == null || Value == null) return false;
            if (!otherToken.Value.Equals(this.Value)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + (int)((this.Value != null) ? (this.Value.GetHashCode()) : (0));
            return result;
        }
    }
}

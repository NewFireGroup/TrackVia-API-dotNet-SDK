using System;

namespace InfiNet.TrackVia.Model
{
    public class OAuth2Token
    {

        public string Value { get; set; }
        public Type TokenType { get; set; }
        public RefreshToken RefreshToken { get; set; }
        public long ExpiresIn { get; set; }
        public long Expires_In { get; set; }
        public DateTime Expiration { get; set; }
        public string[] Scope { get; set; }
        public string Access_Token { get; set; }
        public string AccessToken { get; set; }
        public string Refresh_Token { get; set; }

        #region override Equals

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is OAuth2Token)) return false;

            OAuth2Token otherToken = (OAuth2Token)obj;

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

        #endregion

        #region Nested Types

        public enum Type { bearer = 1 }
        
        #endregion
    }
}

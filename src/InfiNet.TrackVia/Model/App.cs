
namespace InfiNet.TrackVia.Model
{
    public class App
    {

        public App(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }

        #region override Equals

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is App)) return false;

            App otherApp = (App)obj;

            if (otherApp.Id == null || Id == null) return false;

            return otherApp.Id.Equals(this.Id);
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 1;
            result = prime * result + (int)((this.Id != null) ? (this.Id.GetHashCode()) : (0));
            return result;
        }

        #endregion

    }
}

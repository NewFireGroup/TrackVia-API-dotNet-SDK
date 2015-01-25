
namespace InfiNet.TrackVia.Model
{
    public class View
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ApplicationName { get; set; }

        public View() { }

        public View(string id, string name, string applicationName)
        {
            this.Id = id;
            this.Name = name;
            this.ApplicationName = applicationName;
        }

        #region override Equals

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is View)) return false;

            View otherView = (View)obj;

            if (otherView.Id == null || Id == null) return false;
         
            return otherView.Id.Equals(this.Id);
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

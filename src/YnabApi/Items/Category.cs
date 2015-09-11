using System;
using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Category : IHaveCategoryId, IEquatable<Category>
    {
        private readonly JObject _category;

        internal Category(JObject category)
        {
            this._category = category;

            this.Id = category.Value<string>("entityId");
            this.Name = category.Value<string>("name");
            this.IsTombstone = category.Value<bool>("isTombstone");
            this.IsSystemCategory = false;
        }

        internal Category(string id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.IsTombstone = false;
            this.IsSystemCategory = true;
        }

        public string Id { get; }
        public string Name { get; }

        public bool IsTombstone { get; }

        public bool IsSystemCategory { get; }

        string IHaveCategoryId.Id => this.Id;

        internal JObject GetJson() => (JObject)this._category.DeepClone();

        public override string ToString()
        {
            return this.Name;
        }

        public bool Equals(Category other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(this.Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != this.GetType())
                return false;

            return this.Equals((Category)obj);
        }

        public override int GetHashCode()
        {
            return this.Id?.GetHashCode() ?? 0;
        }
    }
}
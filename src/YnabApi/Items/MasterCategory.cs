using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YnabApi.Items
{
    public class MasterCategory : IEquatable<MasterCategory>
    {
        private readonly JObject _masterCategory;

        public MasterCategory(JObject masterCategory)
        {
            this._masterCategory = masterCategory;

            this.Id = masterCategory.Value<string>("entityId");
            this.Name = masterCategory.Value<string>("name");
            this.IsTombstone = masterCategory.Value<bool>("isTombstone");
            this.SubCategories = masterCategory
                .Value<JArray>("subCategories")
                .Values<JObject>()
                .Select(f => new Category(f))
                .ToList();
        }

        public string Id { get; }
        public string Name { get; }
        public bool IsTombstone { get; }
        public IList<Category> SubCategories { get; }

        internal JObject GetJson() => (JObject)this._masterCategory.DeepClone();

        public override string ToString()
        {
            return this.Name;
        }

        public bool Equals(MasterCategory other)
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

            return this.Equals((MasterCategory)obj);
        }

        public override int GetHashCode()
        {
            return this.Id?.GetHashCode() ?? 0;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YnabApi.Items
{
    public class MasterCategory : IEquatable<MasterCategory>
    {
        private readonly JObject _masterCategory;
        
        internal MasterCategory(JObject masterCategory)
        {
            this._masterCategory = masterCategory;

            this.Id = masterCategory.Value<string>("entityId");
            this.Name = masterCategory.Value<string>("name");
            this.IsTombstone = masterCategory.Value<bool>("isTombstone");
            this.IsSystemCategory = false;
            this.SubCategories = masterCategory
                .Value<JArray>("subCategories")
                ?.Values<JObject>()
                ?.Select(f => new Category(f))
                ?.ToList() ?? new List<Category>();

            this.EnsureSubCategoriesAreFilledForSystemCategories();
        }

        internal MasterCategory(string categoryId, string categoryName)
        {
            this.Id = categoryId;
            this.Name = categoryName;
            this.IsTombstone = false;
            this.IsSystemCategory = true;
            this.SubCategories = new List<Category>();

            this.EnsureSubCategoriesAreFilledForSystemCategories();
        }
        
        public string Id { get; }
        public string Name { get; }
        public bool IsTombstone { get; }
        public bool IsSystemCategory { get; private set; }
        public IList<Category> SubCategories { get; }

        internal JObject GetJson() => (JObject)this._masterCategory.DeepClone();
        
        private void EnsureSubCategoriesAreFilledForSystemCategories()
        {
            if (this.Id == Constants.MasterCategory.HiddenId)
            {
                this.IsSystemCategory = true;
            }

            if (this.Id == Constants.MasterCategory.IncomeId)
            {
                this.IsSystemCategory = true;

                this.SubCategories.Add(new Category(Constants.Category.DeferredIncomeId, Constants.Category.DeferredIncome));
                this.SubCategories.Add(new Category(Constants.Category.ImmediateIncomeId, Constants.Category.ImmediateIncome));
            }

            if (this.Id == Constants.MasterCategory.InternalId)
            {
                this.IsSystemCategory = true;

                this.SubCategories.Add(new Category(Constants.Category.SplitId, Constants.Category.Split));
            }
        }

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
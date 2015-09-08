using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YnabApi.Items
{
    public class MonthlyCategoryBudget : IEquatable<MonthlyCategoryBudget>
    {
        private readonly JObject _monthlyCategoryBudget;

        public MonthlyCategoryBudget(JObject monthlyCategoryBudget, IList<Category> allCategories)
        {
            this._monthlyCategoryBudget = monthlyCategoryBudget;

            this.Id = monthlyCategoryBudget.Value<string>("entityId");
            this.Category = allCategories.FirstOrDefault(f => f.Id == monthlyCategoryBudget.Value<string>("categoryId"));
            this.Budgeted = monthlyCategoryBudget.Value<decimal>("budgeted");
        }

        public string Id { get; }
        public Category Category { get; }
        public decimal Budgeted { get; }
        
        internal JObject GetJson() => (JObject)this._monthlyCategoryBudget.DeepClone();

        public override string ToString()
        {
            return $"Monthly category budget {this.Category}";
        }

        public bool Equals(MonthlyCategoryBudget other)
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

            return this.Equals((MonthlyCategoryBudget)obj);
        }

        public override int GetHashCode()
        {
            return this.Id?.GetHashCode() ?? 0;
        }
    }
}
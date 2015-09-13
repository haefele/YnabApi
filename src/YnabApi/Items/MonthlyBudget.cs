using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YnabApi.Items
{
    public class MonthlyBudget : IEquatable<MonthlyBudget>
    {
        private readonly JObject _monthlyBudget;

        internal MonthlyBudget(JObject monthlyBudget, IList<Category> allCategories)
        {
            this._monthlyBudget = monthlyBudget;

            this.Id = monthlyBudget.Value<string>("entityId");
            this.MonthAndYear = monthlyBudget.Value<DateTime>("month");
            this.CategoryBudgets = monthlyBudget
                .Value<JArray>("monthlySubCategoryBudgets")
                .Values<JObject>()
                .Select(f => new MonthlyCategoryBudget(f, allCategories))
                .ToList();
        }

        public string Id { get; }
        public DateTime MonthAndYear { get; }
        public IList<MonthlyCategoryBudget> CategoryBudgets { get; }
        
        internal JObject GetJson() => (JObject)this._monthlyBudget.DeepClone();

        public override string ToString()
        {
            return $"Monthly budget {this.MonthAndYear}";
        }

        public bool Equals(MonthlyBudget other)
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

            return this.Equals((MonthlyBudget)obj);
        }

        public override int GetHashCode()
        {
            return this.Id?.GetHashCode() ?? 0;
        }
    }
}
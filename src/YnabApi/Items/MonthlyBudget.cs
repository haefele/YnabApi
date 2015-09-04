using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YnabApi.Items
{
    public class MonthlyBudget
    {
        private readonly JObject _monthlyBudget;

        public MonthlyBudget(JObject monthlyBudget, IList<Category> allCategories)
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

        public override string ToString()
        {
            return $"Monthly budget {this.MonthAndYear}";
        }

        internal JObject GetJson() => (JObject)this._monthlyBudget.DeepClone();
    }
}
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace YnabApi.Items
{
    public class MonthlyCategoryBudget
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

        public override string ToString()
        {
            return $"Monthly category budget {this.Category}";
        }


        internal JObject GetJson() => (JObject)this._monthlyCategoryBudget.DeepClone();
    }
}
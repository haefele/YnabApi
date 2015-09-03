using System;
using Newtonsoft.Json.Linq;

namespace YnabApi.DeviceActions
{
    public class ChangeMonthlyBudgetAction : IDeviceAction
    {
        public IHaveCategoryId Category { get; set; }
        public DateTime MonthAndYear { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }

        public JObject ToJsonForYdiff(string deviceId, int knowledgeNumber)
        {
            return new JObject
            {
                { "budgeted", this.Amount },
                { "categoryId", this.Category.Id },
                { "entityId", $"MCB/{this.MonthAndYear.Year}-{this.MonthAndYear.Month}/{this.Category.Id}" },
                { "entityType", "monthlyCategoryBudget" },
                { "entityVersion", $"{deviceId}-{knowledgeNumber}" },
                { "isTombstone", false },
                { "madeWithKnowledge", null },
                { "note", this.Note },
                { "overspendingHandling", null },
                { "parentMonthlyBudgetId", $"MB/{this.MonthAndYear.Year}-{this.MonthAndYear.Month}" }
            };
        }
    }
}
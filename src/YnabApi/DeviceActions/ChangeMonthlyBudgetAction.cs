using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YnabApi.DeviceActions
{
    public class ChangeMonthlyBudgetAction : IDeviceAction
    {
        public IHaveCategoryId Category { get; set; }
        public DateTime MonthAndYear { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }

        public IEnumerable<JObject> ToJsonForYdiff(string deviceId, KnowledgeGenerator knowledgeGenerator)
        {
            yield return new JObject
            {
                { "budgeted", this.Amount },
                { "categoryId", this.Category.Id },
                { "entityId", $"MCB/{this.MonthAndYear.Year}-{this.MonthAndYear.Month}/{this.Category.Id}" },
                { "entityType", "monthlyCategoryBudget" },
                { "entityVersion", $"{deviceId}-{knowledgeGenerator.GetNext()}" },
                { "isTombstone", false },
                { "madeWithKnowledge", null },
                { "note", this.Note },
                { "overspendingHandling", null },
                { "parentMonthlyBudgetId", $"MB/{this.MonthAndYear.Year}-{this.MonthAndYear.Month}" }
            };
        }
    }
}
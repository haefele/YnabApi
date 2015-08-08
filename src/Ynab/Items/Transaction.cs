using System;
using Newtonsoft.Json.Linq;
using Ynab.Helpers;

namespace Ynab.Items
{
    public class Transaction : IYnabItem
    {
        public Transaction()
        {
            this.Id = EntityId.CreateNew();
        }

        public string AccountId { get; set; }
        public decimal Amount { get; set; }
        public string CategoryId { get; set; }
        public string PayeeId { get; set; }
        public string Memo { get; set; }

        public string Id { get; }

        public JObject ToJsonForYdiff(string deviceId, int knowledgeNumber)
        {
            return new JObject
            {
                { "accepted", true },
                { "accountId", this.AccountId },
                { "amount", this.Amount },
                { "categoryId", this.CategoryId },
                { "checkNumber", null },
                { "cleared", "Cleared" },
                { "date", DateTime.Today.ToString("yyyy-MM-dd") },
                { "dateEnteredFromSchedule", null },
                { "entityId", this.Id },
                { "entityType", "transaction" },
                { "entityVersion", $"{deviceId}-{knowledgeNumber}" },
                { "flag", null },
                { "importedPayee", null },
                { "isTombstone", false },
                { "madeWithKnowledge", null },
                { "memo", this.Memo },
                { "payeeId", this.PayeeId },
                { "subTransactions", new JArray() },
                { "targetAccountId", null },
                { "transferTransactionId", null }
            };
        }
    }
}
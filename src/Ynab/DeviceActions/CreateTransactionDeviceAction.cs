using System;
using Newtonsoft.Json.Linq;
using Ynab.Helpers;

namespace Ynab.DeviceActions
{
    public class CreateTransactionDeviceAction : IDeviceAction
    {
        public CreateTransactionDeviceAction()
        {
            this.Id = EntityId.CreateNew();
        }

        public string Id { get; }
        public IHaveAccountId Account { get; set; }
        public decimal Amount { get; set; }
        public IHaveCategoryId Category { get; set; }
        public IHavePayeeId Payee { get; set; }
        public string Memo { get; set; }

        public JObject ToJsonForYdiff(string deviceId, int knowledgeNumber)
        {
            return new JObject
            {
                { "accepted", true },
                { "accountId", this.Account.Id },
                { "amount", this.Amount },
                { "categoryId", this.Category.Id },
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
                { "payeeId", this.Payee.Id },
                { "subTransactions", new JArray() },
                { "targetAccountId", null },
                { "transferTransactionId", null }
            };
        }
    }
}
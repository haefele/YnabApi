using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using YnabApi.Helpers;

namespace YnabApi.DeviceActions
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
        public bool Cleared { get; set; }

        public IEnumerable<JObject> ToJsonForYdiff(string deviceId, KnowledgeGenerator knowledgeGenerator)
        {
            yield return new JObject
            {
                { "accepted", true },
                { "accountId", this.Account.Id },
                { "amount", this.Amount },
                { "categoryId", this.Category.Id },
                { "checkNumber", null },
                { "cleared", this.Cleared ? "Cleared" : "Uncleared" },
                { "date", DateTime.Today.ToString("yyyy-MM-dd") },
                { "dateEnteredFromSchedule", null },
                { "entityId", this.Id },
                { "entityType", "transaction" },
                { "entityVersion", $"{deviceId}-{knowledgeGenerator.GetNext()}" },
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
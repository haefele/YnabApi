using Newtonsoft.Json.Linq;
using YnabApi.Items;

namespace YnabApi.DeviceActions
{
    public class DeleteTransactionDeviceAction : IDeviceAction
    {
        public Transaction Transaction { get; set; }

        public JObject ToJsonForYdiff(string deviceId, int knowledgeNumber)
        {
            return new JObject
            {
                { "accepted", true },
                { "accountId", this.Transaction.Account.Id },
                { "amount", this.Transaction.Amount },
                { "categoryId", this.Transaction.Category.Id },
                { "cleared", this.Transaction.Cleared ? "Cleared" : "Uncleared" },
                { "date", this.Transaction.Date.ToString("yyyy-MM-dd") },
                { "entityId", this.Transaction.Id },
                { "entityType", "transaction" },
                { "entityVersion", $"{deviceId}-{knowledgeNumber}" },
                { "isTombstone", true },
                { "memo", this.Transaction.Memo },
                { "payeeId", this.Transaction.Payee.Id }
            };
        }
    }
}
using Newtonsoft.Json.Linq;
using Ynab.Helpers;

namespace Ynab.Items
{
    public class Payee : IYnabItem
    {
        public Payee()
        {
            this.Id = EntityId.CreateNew();
        }

        public string Name { get; set; }
        public string Id { get; }

        public JObject ToJsonForYdiff(string deviceId, int knowledgeNumber)
        {
            return new JObject
            {
                { "name", this.Name },
                { "autoFillCategoryId", null },
                { "isTombstone", false },
                { "madeWithKnowledge", null },
                { "entityVersion", $"{deviceId}-{knowledgeNumber}" },
                { "enabled", true },
                { "isResolvedConflict", false },
                { "autoFillMemo", string.Empty },
                { "autoFillAmount", 0m },
                { "entityId", this.Id },
                { "entityType", "payee" },
                { "targetAccountId", null }
            };
        }
    }
}
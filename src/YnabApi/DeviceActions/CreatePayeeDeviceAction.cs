using Newtonsoft.Json.Linq;
using YnabApi.Helpers;

namespace YnabApi.DeviceActions
{
    public class CreatePayeeDeviceAction : IDeviceAction, IHavePayeeId
    {
        public CreatePayeeDeviceAction()
        {
            this.Id = EntityId.CreateNew();
        }

        public string Id { get; }
        public string Name { get; set; }

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
using System.Collections.Generic;
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

        public IEnumerable<JObject> ToJsonForYdiff(RegisteredDevice device, KnowledgeGenerator knowledgeGenerator)
        {
            yield return new JObject
            {
                { "name", this.Name },
                { "autoFillCategoryId", null },
                { "isTombstone", false },
                { "madeWithKnowledge", null },
                { "entityVersion", $"{device.ShortDeviceId}-{knowledgeGenerator.GetNext()}" },
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
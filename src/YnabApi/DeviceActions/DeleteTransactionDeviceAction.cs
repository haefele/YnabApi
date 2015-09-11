using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using YnabApi.Items;

namespace YnabApi.DeviceActions
{
    public class DeleteTransactionDeviceAction : IDeviceAction
    {
        public Transaction Transaction { get; set; }

        public IEnumerable<JObject> ToJsonForYdiff(RegisteredDevice device, KnowledgeGenerator knowledgeGenerator)
        {
            var json = this.Transaction.GetJson();
            json["isTombstone"] = true;
            json["entityVersion"] = $"{device.ShortDeviceId}-{knowledgeGenerator.GetNext()}";

            yield return json;
        }
    }
}
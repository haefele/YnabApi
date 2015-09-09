using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using YnabApi.Items;

namespace YnabApi.DeviceActions
{
    public class DeleteTransactionDeviceAction : IDeviceAction
    {
        public Transaction Transaction { get; set; }

        public IEnumerable<JObject> ToJsonForYdiff(string deviceId, KnowledgeGenerator knowledgeGenerator)
        {
            var json = this.Transaction.GetJson();
            json["isTombstone"] = true;

            yield return json;
        }
    }
}
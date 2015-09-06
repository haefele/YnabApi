using Newtonsoft.Json.Linq;
using YnabApi.Items;

namespace YnabApi.DeviceActions
{
    public class DeleteTransactionDeviceAction : IDeviceAction
    {
        public Transaction Transaction { get; set; }

        public JObject ToJsonForYdiff(string deviceId, int knowledgeNumber)
        {
            var json = this.Transaction.GetJson();

            json["isTombstone"] = true;

            return json;
        }
    }
}
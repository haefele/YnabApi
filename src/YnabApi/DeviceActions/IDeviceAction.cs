using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YnabApi.DeviceActions
{
    public interface IDeviceAction
    {
        IEnumerable<JObject> ToJsonForYdiff(string deviceId, KnowledgeGenerator knowledgeGenerator);
    }
}
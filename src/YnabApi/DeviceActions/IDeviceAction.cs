using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YnabApi.DeviceActions
{
    public interface IDeviceAction
    {
        IEnumerable<JObject> ToJsonForYdiff(RegisteredDevice device, KnowledgeGenerator knowledgeGenerator);
    }
}
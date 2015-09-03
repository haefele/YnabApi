using Newtonsoft.Json.Linq;

namespace YnabApi.DeviceActions
{
    public interface IDeviceAction
    {
        JObject ToJsonForYdiff(string deviceId, int knowledgeNumber);
    }
}
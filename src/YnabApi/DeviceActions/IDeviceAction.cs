using Newtonsoft.Json.Linq;

namespace YnabApi.DeviceActions
{
    public interface IDeviceAction
    {
        string Id { get; }
        JObject ToJsonForYdiff(string deviceId, int knowledgeNumber);
    }
}
using Newtonsoft.Json.Linq;

namespace Ynab.DeviceActions
{
    public interface IDeviceAction
    {
        string Id { get; }
        JObject ToJsonForYdiff(string deviceId, int knowledgeNumber);
    }
}
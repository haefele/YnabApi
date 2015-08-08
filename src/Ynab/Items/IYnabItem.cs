using Newtonsoft.Json.Linq;

namespace Ynab.Items
{
    public interface IYnabItem
    {
        string Id { get; }
        JObject ToJsonForYdiff(string deviceId, int knowledgeNumber);
    }
}
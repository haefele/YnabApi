using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ynab.Helpers
{
    public static class Knowledge
    {
        public static int ExtractKnowledgeForDevice(string knowledgeString, string deviceId)
        {
            var parsed = ParseKnowledge(knowledgeString);

            return parsed.ContainsKey(deviceId) 
                ? parsed[deviceId] 
                : 0;
        }

        public static Dictionary<string, int> ParseKnowledge(string knowledgeString)
        {
            var result =
                from pair in knowledgeString.Split(',')
                let deviceId = pair.Split('-').First()
                let knowledge = int.Parse(pair.Split('-').Last())
                select new
                {
                    Device = deviceId,
                    Knowledge = knowledge
                };

            return result.ToDictionary(f => f.Device, f => f.Knowledge);
        }

        public static string CreateKnowledgeForNewDevice(IEnumerable<RegisteredDevice> device, string deviceId)
        {
            var knowledge = CreateKnowledge(device);
            return $"{knowledge},{deviceId}-0";
        }

        public static string CreateKnowledgeForYdiff(IEnumerable<RegisteredDevice> devices, string shortDeviceId, int knowledgeCount)
        {
            var knowledge = CreateKnowledge(devices.Where(f => f.ShortDeviceId != shortDeviceId));
            return $"{knowledge},{shortDeviceId}-{knowledgeCount}";
        }

        public static string CreateKnowledge(IEnumerable<RegisteredDevice> devices)
        {
            return string.Join(",", devices.Select(f => $"{f.ShortDeviceId}-{f.CurrentKnowledge}"));
        }
    }
}
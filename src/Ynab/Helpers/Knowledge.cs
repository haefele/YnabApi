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

        public static string CreateKnowledgeForNewDevice(string knowledgeString, string deviceId)
        {
            if (knowledgeString.Contains(deviceId))
                return knowledgeString;
            
            return $"{knowledgeString},{deviceId}-0";
        }

        public static string CreateKnowledgeForYdiff(string knowledgeString, string shortDeviceId, int knowledgeCount)
        {
            var parsed = ParseKnowledge(knowledgeString);
            parsed[shortDeviceId] = knowledgeCount;

            return CreateKnowledge(parsed);
        }

        public static string CreateKnowledge(Dictionary<string, int> knowledge)
        {
            return string.Join(",", knowledge.Select(f => $"{f.Key}-{f.Value}"));
        }
    }
}
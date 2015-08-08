using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ynab.Helpers;

namespace Ynab
{
    public class YnabApi
    {
        public async Task<IList<Budget>> GetBudgetsAsync()
        {
            string ynabSettingsFilePath = YnabPaths.YnabSettingsFile().ToFullPath();
            
            var ynabSettingsJson = await FileHelpers.ReadFileAsync(ynabSettingsFilePath);
            var ynabSettings = JObject.Parse(ynabSettingsJson);

            string relativeBudgetsFolder = ynabSettings.Value<string>("relativeDefaultBudgetsFolder");

            return ynabSettings
                .Value<JArray>("relativeKnownBudgets")
                .Values()
                .Select(f => f.Value<string>())
                .Select(f => new Budget(this.ExtractBudgetName(f, relativeBudgetsFolder), f))
                .ToArray();
        }

        private string ExtractBudgetName(string budgetPath, string budgetsFolderPath)
        {
            var regex = new Regex(budgetsFolderPath + "/(.*)~.*");

            if (regex.IsMatch(budgetPath))
                return regex.Match(budgetPath).Groups[1].Value;

            return budgetPath;
        }
    }
}
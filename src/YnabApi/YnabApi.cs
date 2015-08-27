using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YnabApi.Files;
using YnabApi.Helpers;

namespace YnabApi
{
    public class YnabApi
    {
        private readonly IFileSystem _fileSystem;

        public YnabApi(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem;
        }

        public async Task<IList<Budget>> GetBudgetsAsync()
        {
            string ynabSettingsFilePath = YnabPaths.YnabSettingsFile();

            var ynabSettingsJson = await this._fileSystem.ReadFileAsync(ynabSettingsFilePath);
            var ynabSettings = JObject.Parse(ynabSettingsJson);

            string relativeBudgetsFolder = ynabSettings.Value<string>("relativeDefaultBudgetsFolder");

            return ynabSettings
                .Value<JArray>("relativeKnownBudgets")
                .Values()
                .Select(f => f.Value<string>())
                .Select(f => new Budget(this._fileSystem, this.ExtractBudgetName(f, relativeBudgetsFolder), f))
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
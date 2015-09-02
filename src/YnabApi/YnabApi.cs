using System;
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
        private readonly YnabApiSettings _settings;

        private Lazy<Task<IList<Budget>>> _cachedBudgets; 

        public YnabApi(YnabApiSettings settings)
        {
            this._settings = settings;
        }

        public Task<IList<Budget>> GetBudgetsAsync()
        {
            if (this._cachedBudgets == null || this._settings.CacheBudgets == false)
            {
                this._cachedBudgets = new Lazy<Task<IList<Budget>>>(async () =>
                {
                    string ynabSettingsFilePath = YnabPaths.YnabSettingsFile();

                    var ynabSettingsJson = await this._settings.FileSystem.ReadFileAsync(ynabSettingsFilePath);
                    var ynabSettings = JObject.Parse(ynabSettingsJson);

                    string relativeBudgetsFolder = ynabSettings.Value<string>("relativeDefaultBudgetsFolder");

                    return ynabSettings
                        .Value<JArray>("relativeKnownBudgets")
                        .Values()
                        .Select(f => f.Value<string>())
                        .Select(f => new Budget(this._settings, this.ExtractBudgetName(f, relativeBudgetsFolder), f))
                        .ToArray();
                });
            }

            return this._cachedBudgets.Value;
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
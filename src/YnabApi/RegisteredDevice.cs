using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;
using YnabApi.Extensions;
using YnabApi.Helpers;
using YnabApi.Items;

namespace YnabApi
{
    public class RegisteredDevice
    {
        private readonly YnabApiSettings _settings;
        private readonly JObject _device;

        private Lazy<Task<IList<Payee>>> _cachedPayees;
        private Lazy<Task<IList<Account>>> _cachedAccounts;
        private Lazy<Task<IList<MasterCategory>>> _cachedCategories;
        private Lazy<Task<IList<Transaction>>>  _cachedTransactions;
        private Lazy<Task<IList<MonthlyBudget>>> _cachedMonthlyBudgets;
        private Lazy<Task<JObject>> _cachedBudgetFile;
        private Lazy<Task<IList<LocalChange>>> _cachedAllLocalChanges;

        public RegisteredDevice(YnabApiSettings settings, Budget budget, JObject device)
        {
            this._settings = settings;
            this._device = device;

            this.Budget = budget;
            this.FriendlyName = device.Value<string>("friendlyName");
            this.ShortDeviceId = device.Value<string>("shortDeviceId");
            this.HasFullKnowledge = device.Value<bool>("hasFullKnowledge");
            this.KnowledgeString = device.Value<string>("knowledge");
            this.CurrentKnowledge = Knowledge.ExtractKnowledgeForDevice(this.KnowledgeString, this.ShortDeviceId);
            this.DeviceGuid = device.Value<string>("deviceGUID");
            this.YnabVersion = device.Value<string>("YNABVersion");
        }

        public Budget Budget { get; }
        public string FriendlyName { get; }
        public string ShortDeviceId { get; }
        public bool HasFullKnowledge { get; }
        public int CurrentKnowledge { get; private set; }
        public string KnowledgeString { get; private set; }
        public string DeviceGuid { get; }
        public string YnabVersion { get; }
        
        public Task<IList<Payee>> GetPayeesAsync()
        {
            if (this._cachedPayees == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedPayees = new Lazy<Task<IList<Payee>>>(async () =>
                {
                    try
                    {
                        var budgetFile = await this.GetBudgetFileAsync();

                        var payees = budgetFile
                            .Value<JArray>("payees")
                            .Values<JObject>()
                            .Select(f => new Payee(f))
                            .ToHashSet(HashSetDuplicateOptions.Override);

                        var localChanges = await this.GetAllLocalChangesAsync("payee");

                        foreach (var localChange in localChanges)
                        {
                            var localPayee = new Payee(localChange);
                            payees.Add(localPayee, HashSetDuplicateOptions.Override);
                        }

                        return payees.ToList();
                    }
                    catch (Exception exception) when (exception is YnabApiException == false)
                    {
                        throw new YnabApiException("Error while loading the payees.", exception);
                    }
                });
            }

            return this._cachedPayees.Value;
        }

        public Task<IList<Account>> GetAccountsAsync()
        {
            if (this._cachedAccounts == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedAccounts = new Lazy<Task<IList<Account>>>(async () =>
                {
                    try
                    {
                        var budgetFile = await this.GetBudgetFileAsync();
                        var accounts = budgetFile
                            .Value<JArray>("accounts")
                            .Values<JObject>()
                            .Select(f => new Account(f))
                            .ToHashSet(HashSetDuplicateOptions.Override);

                        var localChanges = await this.GetAllLocalChangesAsync("account");

                        foreach (var localChange in localChanges)
                        {
                            var localAccount = new Account(localChange);
                            accounts.Add(localAccount, HashSetDuplicateOptions.Override);
                        }

                        return accounts.ToList();
                    }
                    catch (Exception exception) when (exception is YnabApiException == false)
                    {
                        throw new YnabApiException("Error while loading the accounts.", exception);
                    }
                });
            }

            return this._cachedAccounts.Value;
        }

        public Task<IList<MasterCategory>> GetCategoriesAsync()
        {
            if (this._cachedCategories == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedCategories = new Lazy<Task<IList<MasterCategory>>>(async () =>
                {
                    try
                    {
                        var budgetFile = await this.GetBudgetFileAsync();

                        var allCategories = budgetFile
                            .Value<JArray>("masterCategories")
                            .Values<JObject>()
                            .Select(f => new MasterCategory(f))
                            .ToList();

                        return allCategories;
                    }
                    catch (Exception exception) when (exception is YnabApiException == false)
                    {
                        throw new YnabApiException("Error while loading the categories.", exception);
                    }
                });
            }

            return this._cachedCategories.Value;
        }

        public Task<IList<Transaction>> GetTransactionsAsync()
        {
            if (this._cachedTransactions == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedTransactions = new Lazy<Task<IList<Transaction>>>(async () =>
                {
                    try
                    {
                        var budgetFile = await this.GetBudgetFileAsync();

                        var allAccounts = await this.GetAccountsAsync();
                        var allCategories = (await this.GetCategoriesAsync()).SelectMany(f => f.SubCategories).ToList();
                        var allPayees = await this.GetPayeesAsync();
                        
                        var transactions = budgetFile
                            .Value<JArray>("transactions")
                            .Values<JObject>()
                            .Select(f => new Transaction(f, allAccounts, allCategories, allPayees))
                            .ToHashSet(HashSetDuplicateOptions.Override);

                        var localChanges = await this.GetAllLocalChangesAsync("transaction");

                        foreach (var localChange in localChanges)
                        {
                            var localTransaction = new Transaction(localChange, allAccounts, allCategories, allPayees);
                            transactions.Add(localTransaction, HashSetDuplicateOptions.Override);
                        }

                        return transactions.ToList();
                    }
                    catch (Exception exception) when (exception is YnabApiException == false)
                    {
                        throw new YnabApiException("Error while loading the transactions.", exception);
                    }
                });
            }

            return this._cachedTransactions.Value;
        }

        public Task<IList<MonthlyBudget>> GetMonthlyBudgetsAsync()
        {
            if (this._cachedMonthlyBudgets == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedMonthlyBudgets = new Lazy<Task<IList<MonthlyBudget>>>(async () =>
                {
                    try
                    {
                        var budgetFile = await this.GetBudgetFileAsync();

                        var allCategories = (await this.GetCategoriesAsync()).SelectMany(f => f.SubCategories).ToList();

                        return budgetFile
                            .Value<JArray>("monthlyBudgets")
                            .Values<JObject>()
                            .Select(f => new MonthlyBudget(f, allCategories))
                            .ToList();
                    }
                    catch (Exception exception) when (exception is YnabApiException == false)
                    {
                        throw new YnabApiException("Error while loading the monthly budgets.", exception);
                    }
                });
            }

            return this._cachedMonthlyBudgets.Value;
        }

        public void ClearCache()
        {
            this._cachedPayees = null;
            this._cachedAccounts = null;
            this._cachedCategories = null;
            this._cachedTransactions = null;
            this._cachedMonthlyBudgets = null;
            this._cachedBudgetFile = null;
            this._cachedAllLocalChanges = null;
        }
        
        public async Task ExecuteActions(params IDeviceAction[] actions)
        {
            try
            {
                var startKnowledge = this.CurrentKnowledge;

                var knowledgeGenerator = new KnowledgeGenerator(startKnowledge);

                JArray itemsJsonArray = new JArray(from action in actions
                                                   from item in action.ToJsonForYdiff(this, knowledgeGenerator)
                                                   select item);

                this.CurrentKnowledge = knowledgeGenerator.GetCurrent();

                var startVersion = Knowledge.CreateKnowledgeForYdiff(this.KnowledgeString, this.ShortDeviceId, startKnowledge);
                var endVersion = Knowledge.CreateKnowledgeForYdiff(this.KnowledgeString, this.ShortDeviceId, this.CurrentKnowledge);

                var dataPath = await this.Budget.GetDataFolderPathAsync();
                var json = new JObject
                {
                    {"budgetDataGUID", YnabPaths.DataFolderName(dataPath)},
                    {"budgetGUID", this.Budget.BudgetPath},
                    {"dataVersion", "4.2"},
                    {"deviceGUID", this.DeviceGuid},
                    {"startVersion", startVersion},
                    {"endVersion", endVersion},
                    {"shortDeviceId", this.ShortDeviceId},
                    {"publishTime", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")},
                    {"items", itemsJsonArray}
                };

                await this._settings.FileSystem.WriteFileAsync(YnabPaths.YdiffFile(dataPath, this.DeviceGuid, startVersion, this.ShortDeviceId, this.CurrentKnowledge), json.ToString());

                this.KnowledgeString = endVersion;
                this._device["knowledge"] = endVersion;
                var deviceFilePath = YnabPaths.DeviceFile(dataPath, this.ShortDeviceId);
                await this._settings.FileSystem.WriteFileAsync(deviceFilePath, this._device.ToString());

                await this._settings.FileSystem.FlushWritesAsync();
            }
            catch (Exception exception) when (exception is YnabApiException == false)
            {
                throw new YnabApiException("Error while executing device actions.", exception);
            }
        }
        
        #region Private Methods
        private Task<JObject> GetBudgetFileAsync()
        {
            if (this._cachedBudgetFile == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedBudgetFile = new Lazy<Task<JObject>>(async () =>
                {
                    string budgetFilePath;

                    if (this.HasFullKnowledge)
                    {
                        budgetFilePath = YnabPaths.BudgetFile(YnabPaths.DeviceFolder(await this.Budget.GetDataFolderPathAsync(), this.DeviceGuid));
                    }
                    else
                    {
                        var allDevices = await this.Budget.GetRegisteredDevicesAsync();
                        var fullKnowledgeDevice = allDevices.First(f => f.HasFullKnowledge);

                        budgetFilePath = YnabPaths.BudgetFile(YnabPaths.DeviceFolder(await this.Budget.GetDataFolderPathAsync(), fullKnowledgeDevice.DeviceGuid));
                    }

                    string file = await this._settings.FileSystem.ReadFileAsync(budgetFilePath);
                    
                    return JObject.Parse(file);
                });
            }

            return this._cachedBudgetFile.Value;
        }

        private async Task<IList<JObject>> GetAllLocalChangesAsync(string entityType)
        {
            if (this._cachedAllLocalChanges == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedAllLocalChanges = new Lazy<Task<IList<LocalChange>>>(async () =>
                {
                    var budgetFile = await this.GetBudgetFileAsync();
                    var budgetFileKnowledge = budgetFile.Value<JObject>("fileMetaData").Value<string>("currentKnowledge");

                    var result = new List<LocalChange>();
                    var regex = new Regex(".*_.-([0-9]*).ydiff");

                    foreach (var device in await this.Budget.GetRegisteredDevicesAsync())
                    { 
                        var budgetFileKnowledgeOfThatDevice = Knowledge.ExtractKnowledgeForDevice(budgetFileKnowledge, device.ShortDeviceId);

                        var files = await this._settings.FileSystem.GetFilesAsync(YnabPaths.DeviceFolder(await this.Budget.GetDataFolderPathAsync(), device.DeviceGuid));
                        
                        foreach (var file in files)
                        {
                            var fileName = YnabPaths.GetFileName(file);

                            if (regex.IsMatch(fileName) == false)
                                continue;

                            var match = regex.Match(fileName);
                            var endKnowledge = int.Parse(match.Groups[1].Captures[0].Value);

                            if (endKnowledge > budgetFileKnowledgeOfThatDevice)
                            {
                                var fileContent = await this._settings.FileSystem.ReadFileAsync(file);

                                var json = JObject.Parse(fileContent);
                                var date = this.ExtractDateFromYdiff(json);

                                result.Add(new LocalChange(date, endKnowledge, json));
                            }
                        }
                    }

                    return result;
                });
            }
            
            var localChanges = await this._cachedAllLocalChanges.Value;

            return localChanges
                .OrderBy(f => f.Date)
                .ThenBy(f => f.EndEntityVersion)
                .Select(f => f.DiffFileJson)
                .Select(f => f.Value<JArray>("items"))
                .SelectMany(f => f.Values<JObject>())
                .Where(f => f.Value<string>("entityType") == entityType)
                .ToList();
        }

        private DateTime ExtractDateFromYdiff(JObject ydiff)
        {
            string date = ydiff.Value<string>("publishTime");
            
            string[] dateFormats = 
            {
                "yyyy-MM-dd hh:mm:ss",
                "ddd MMM d HH:mm:ss 'GMT'K yyyy",
            };

            DateTime result;

            if (DateTime.TryParseExact(date, dateFormats, null, DateTimeStyles.AdjustToUniversal, out result))
                return result;

            throw new InvalidOperationException("Could not parse publish-time");
        }
        #endregion

        #region Internal

        private class LocalChange
        {
            public LocalChange(DateTime date, int endEntityVersion, JObject diffFileJson)
            {
                this.Date = date;
                this.EndEntityVersion = endEntityVersion;
                this.DiffFileJson = diffFileJson;
            }

            public DateTime Date { get; }
            public int EndEntityVersion { get; }
            public JObject DiffFileJson { get; }
        }
        #endregion
    }
}
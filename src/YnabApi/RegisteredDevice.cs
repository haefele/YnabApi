using System;
using System.Collections.Generic;
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
        private Lazy<Task<IList<Category>>> _cachedCategories;
        private Lazy<Task<IList<Transaction>>>  _cachedTransactions;
        private Lazy<Task<IList<MonthlyBudget>>> _cachedMonthlyBudgets;
        private Lazy<Task<JObject>> _cachedBudgetFile;
        private Lazy<Task<IList<JObject>>> _cachedLocalChanges;

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

                        var localChanges = await this.GetLocalChangesAsync();

                        foreach (var localChange in localChanges)
                        {
                            var localPayees = localChange
                                .Value<JArray>("items")
                                .Values<JObject>()
                                .Where(f => f.Value<string>("entityType") == "payee")
                                .Select(f => new Payee(f))
                                .ToList();

                            foreach (var localPayee in localPayees)
                            {
                                payees.Add(localPayee, HashSetDuplicateOptions.Override);
                            }
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

                        var localChanges = await this.GetLocalChangesAsync();

                        foreach (var localChange in localChanges)
                        {
                            var localAccounts = localChange
                             .Value<JArray>("items")
                             .Values<JObject>()
                             .Where(f => f.Value<string>("entityType") == "account")
                             .Select(f => new Account(f))
                             .ToList();

                            foreach (var localAccount in localAccounts)
                            {
                                accounts.Add(localAccount, HashSetDuplicateOptions.Override);
                            }
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

        public Task<IList<Category>> GetCategoriesAsync()
        {
            if (this._cachedCategories == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedCategories = new Lazy<Task<IList<Category>>>(async () =>
                {
                    try
                    {
                        var budgetFile = await this.GetBudgetFileAsync();

                        var allCategories = budgetFile
                            .Value<JArray>("masterCategories")
                            .Values<JObject>()
                            .SelectMany(masterCategory =>
                            {
                                var subCategories = masterCategory
                                    .Value<JArray>("subCategories");

                                if (subCategories == null)
                                    return new List<Category>();

                                return subCategories?
                                    .Values<JObject>()
                                    .Select(f => new Category(f, masterCategory));
                            })
                            .ToList();
                    
                        allCategories.Add(new Category("Category/__ImmediateIncome__", "Income"));
                        allCategories.Add(new Category("Category/__DeferredIncome__", "Income"));
                        allCategories.Add(new Category("Category/__Split__", "Split"));

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
                        var allCategories = await this.GetCategoriesAsync();
                        var allPayees = await this.GetPayeesAsync();
                        
                        var transactions = budgetFile
                            .Value<JArray>("transactions")
                            .Values<JObject>()
                            .Select(f => new Transaction(f, allAccounts, allCategories, allPayees))
                            .ToHashSet(HashSetDuplicateOptions.Override);

                        var localChanges = await this.GetLocalChangesAsync();

                        foreach (var localChange in localChanges)
                        {
                            var localTransactions = localChange
                                .Value<JArray>("items")
                                .Values<JObject>()
                                .Where(f => f.Value<string>("entityType") == "transaction")
                                .Select(f => new Transaction(f, allAccounts, allCategories, allPayees))
                                .ToList();

                            foreach (var localTransaction in localTransactions)
                            {
                                transactions.Add(localTransaction, HashSetDuplicateOptions.Override);
                            }
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

                        var allCategories = await this.GetCategoriesAsync();

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
            this._cachedLocalChanges = null;
        }
        
        public async Task ExecuteActions(params IDeviceAction[] actions)
        {
            try
            {
                var startKnowledge = this.CurrentKnowledge;

                var knowledgeGenerator = new KnowledgeGenerator(startKnowledge);

                JArray itemsJsonArray = new JArray(from action in actions
                                                   from item in action.ToJsonForYdiff(this.ShortDeviceId, knowledgeGenerator)
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
        private Task<IList<JObject>> GetLocalChangesAsync()
        {
            if (this._cachedLocalChanges == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedLocalChanges = new Lazy<Task<IList<JObject>>>(async () =>
                {
                    var budgetFile = await this.GetBudgetFileAsync();
                    var budgetFileKnowledge = budgetFile.Value<JObject>("fileMetaData").Value<string>("currentKnowledge");

                    var budgetFileKnowledgeOfThisDevice = Knowledge.ExtractKnowledgeForDevice(budgetFileKnowledge, this.ShortDeviceId);
                    var files = await this._settings.FileSystem.GetFilesAsync(YnabPaths.DeviceFolder(await this.Budget.GetDataFolderPathAsync(), this.DeviceGuid));

                    var regex = new Regex(".*_.-([0-9]*).ydiff");

                    var result = new List<JObject>();

                    foreach (var file in files)
                    {
                        var fileName = YnabPaths.GetFileName(file);

                        if (regex.IsMatch(fileName) == false)
                            continue;

                        var match = regex.Match(fileName);
                        var endKnowledge = int.Parse(match.Groups[1].Captures[0].Value);

                        if (endKnowledge > budgetFileKnowledgeOfThisDevice)
                        {
                            var fileContent = await this._settings.FileSystem.ReadFileAsync(file);
                            result.Add(JObject.Parse(fileContent));
                        }
                    }

                    return result;
                });
            }

            return this._cachedLocalChanges.Value;
        }
        #endregion
    }

    public class KnowledgeGenerator
    {
        private int _knowledge;

        internal KnowledgeGenerator(int knowledge)
        {
            this._knowledge = knowledge;
        }

        public int GetNext()
        {
            return ++this._knowledge;
        }

        internal int GetCurrent()
        {
            return this._knowledge;
        }
    }
}
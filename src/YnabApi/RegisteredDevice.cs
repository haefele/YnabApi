using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;
using YnabApi.Files;
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
        private Lazy<Task<JObject>> _cachedBudgetFile; 

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
                    if (this.HasFullKnowledge == false)
                        return new List<Payee>();

                    var budgetFile = await this.GetBudgetFileAsync();
                    return budgetFile
                        .Value<JArray>("payees")
                        .Values<JObject>()
                        .Select(f => new Payee(f))
                        .ToList();
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
                    if (this.HasFullKnowledge == false)
                        return new List<Account>();

                    var budgetFile = await this.GetBudgetFileAsync();
                    return budgetFile
                        .Value<JArray>("accounts")
                        .Values<JObject>()
                        .Select(f => new Account(f))
                        .ToList();
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
                    if (this.HasFullKnowledge == false)
                        return new List<Category>();

                    var budgetFile = await this.GetBudgetFileAsync();

                    return budgetFile
                        .Value<JArray>("masterCategories")
                        .Values<JObject>()
                        .SelectMany(masterCategory => masterCategory
                            .Value<JArray>("subCategories")
                            .Values<JObject>()
                            .Select(f => new Category(f, masterCategory)))
                        .Where(f => f.MasterId != "MasterCategory/__Hidden__")
                        .ToList();
                });
            }

            return this._cachedCategories.Value;
        }
        
        public async Task ExecuteActions(params IDeviceAction[] actions)
        {
            var startKnowledge = this.CurrentKnowledge;

            JArray itemsJsonArray = new JArray(from action in actions
                                               let knowledge = ++this.CurrentKnowledge
                                               select action.ToJsonForYdiff(this.ShortDeviceId, knowledge));
            
            var startVersion = Knowledge.CreateKnowledgeForYdiff(this.KnowledgeString, this.ShortDeviceId, startKnowledge);
            var endVersion = Knowledge.CreateKnowledgeForYdiff(this.KnowledgeString, this.ShortDeviceId, this.CurrentKnowledge);

            var dataPath = await this.Budget.GetDataFolderPathAsync();
            var json = new JObject
            {
                { "budgetDataGUID", YnabPaths.DataFolderName(dataPath) },
                { "budgetGUID", this.Budget.BudgetPath },
                { "dataVersion", "4.2" },
                { "deviceGUID", this.DeviceGuid },
                { "startVersion", startVersion },
                { "endVersion", endVersion },
                { "shortDeviceId", this.ShortDeviceId },
                { "publishTime", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") },
                { "items", itemsJsonArray }
            };

            await this._settings.FileSystem.WriteFileAsync(YnabPaths.YdiffFile(dataPath, this.DeviceGuid, startVersion, this.ShortDeviceId, this.CurrentKnowledge), json.ToString());

            this.KnowledgeString = endVersion;
            this._device["knowledge"] = endVersion;
            var deviceFilePath = YnabPaths.DeviceFile(dataPath, this.ShortDeviceId);
            await this._settings.FileSystem.WriteFileAsync(deviceFilePath, this._device.ToString());
        }


        #region Private Methods
        private Task<JObject> GetBudgetFileAsync()
        {
            if (this._cachedBudgetFile == null || this._settings.CacheRegisteredDeviceData == false)
            {
                this._cachedBudgetFile = new Lazy<Task<JObject>>(async () =>
                {
                    if (this.HasFullKnowledge == false)
                        throw new NotSupportedException("Accessing the budget file only works if the device has full budget knowledge. In this case, it doesn't.");

                    var filePath = YnabPaths.BudgetFile(YnabPaths.DeviceFolder(await this.Budget.GetDataFolderPathAsync(), this.DeviceGuid));
                    string file = await this._settings.FileSystem.ReadFileAsync(filePath);

                    return JObject.Parse(file);
                });
            }

            return this._cachedBudgetFile.Value;
        }
        #endregion
    }
}
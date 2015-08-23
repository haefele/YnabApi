using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ynab.DeviceActions;
using Ynab.Files;
using Ynab.Helpers;

namespace Ynab
{
    public class RegisteredDevice
    {
        private readonly IFileSystem _fileSystem;
        private readonly JObject _device;

        public RegisteredDevice(IFileSystem fileSystem, Budget budget, JObject device)
        {
            this._fileSystem = fileSystem;
            this._device = device;

            this.Budget = budget;
            this.FriendlyName = device.Value<string>("friendlyName");
            this.ShortDeviceId = device.Value<string>("shortDeviceId");
            this.HasFullKnowledge = device.Value<bool>("hasFullKnowledge");
            this.CurrentKnowledge = Knowledge.ExtractKnowledgeForDevice(this.GetKnowledgeString(), this.ShortDeviceId);
            this.DeviceGuid = device.Value<string>("deviceGUID");
            this.YnabVersion = device.Value<string>("YNABVersion");
        }

        public Budget Budget { get; }
        public string FriendlyName { get; }
        public string ShortDeviceId { get; }
        public bool HasFullKnowledge { get; }
        public int CurrentKnowledge { get; private set; }
        public string DeviceGuid { get; }
        public string YnabVersion { get; }

        public string GetKnowledgeString()
        {
            return this._device.Value<string>("knowledge");
        }
        
        public async Task ExecuteActions(params IDeviceAction[] actions)
        {
            var startKnowledge = this.CurrentKnowledge;

            JArray itemsJsonArray = new JArray(from action in actions
                                               let knowledge = ++this.CurrentKnowledge
                                               select action.ToJsonForYdiff(this.ShortDeviceId, knowledge));
            
            var startVersion = Knowledge.CreateKnowledgeForYdiff(this.GetKnowledgeString(), this.ShortDeviceId, startKnowledge);
            var endVersion = Knowledge.CreateKnowledgeForYdiff(this.GetKnowledgeString(), this.ShortDeviceId, this.CurrentKnowledge);

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

            await this._fileSystem.WriteFileAsync(YnabPaths.YdiffFile(dataPath, this.DeviceGuid, startVersion, this.ShortDeviceId, this.CurrentKnowledge), json.ToString());
            
            this._device["knowledge"] = endVersion;

            var deviceFilePath = YnabPaths.DeviceFile(dataPath, this.ShortDeviceId);
            await this._fileSystem.WriteFileAsync(deviceFilePath, this._device.ToString());
        }
    }
}
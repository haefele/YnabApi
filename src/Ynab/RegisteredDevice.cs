using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ynab.Helpers;
using Ynab.Items;

namespace Ynab
{
    public class RegisteredDevice
    {
        public RegisteredDevice(Budget budget, JObject device)
        {
            this.Budget = budget;
            this.FriendlyName = device.Value<string>("friendlyName");
            this.ShortDeviceId = device.Value<string>("shortDeviceId");
            this.HasFullKnowledge = device.Value<bool>("hasFullKnowledge");
            this.CurrentKnowledge = Knowledge.ExtractKnowledgeForDevice(device.Value<string>("knowledge"), this.ShortDeviceId);
            this.DeviceGuid = device.Value<string>("deviceGUID");
        }

        public Budget Budget { get; }
        public string FriendlyName { get; }
        public string ShortDeviceId { get; }
        public bool HasFullKnowledge { get; }
        public int CurrentKnowledge { get; private set; }
        public string DeviceGuid { get; }

        public async Task InsertItems(params IYnabItem[] items)
        {
            var startKnowledge = this.CurrentKnowledge;

            JArray itemsJsonArray = new JArray(from item in items
                                               let knowledge = ++this.CurrentKnowledge
                                               select item.ToJsonForYdiff(this.ShortDeviceId, knowledge));

            var allDevices = await this.Budget.GetRegisteredDevicesAsync();

            var startVersion = Knowledge.CreateKnowledgeForYdiff(allDevices, this.ShortDeviceId, startKnowledge);
            var endVersion = Knowledge.CreateKnowledgeForYdiff(allDevices, this.ShortDeviceId, this.CurrentKnowledge);

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
            
            File.WriteAllText(YnabPaths.YdiffFile(dataPath, this.DeviceGuid, startVersion, this.ShortDeviceId, this.CurrentKnowledge).ToFullPath(), json.ToString());

            var deviceFilePath = YnabPaths.DeviceFile(dataPath, this.ShortDeviceId);
            var deviceFileContent = await FileHelpers.ReadFileAsync(deviceFilePath.ToFullPath());

            var deviceFile = JObject.Parse(deviceFileContent);
            deviceFile["knowledge"] = endVersion;

            File.WriteAllText(deviceFilePath.ToFullPath(), deviceFile.ToString());
        }
    }
}
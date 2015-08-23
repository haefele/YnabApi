using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ynab.Files;
using Ynab.Helpers;

namespace Ynab
{
    public class Budget
    {
        private readonly IFileSystem _fileSystem;
        
        private Lazy<Task<IList<RegisteredDevice>>> _cachedRegisteredDevices;
        private Lazy<Task<string>> _cachedDataFolderPath;

        public string BudgetName { get; }
        public string BudgetPath { get; }

        public Budget(IFileSystem fileSystem, string budgetName, string budgetPath)
        {
            this._fileSystem = fileSystem;

            this.BudgetName = budgetName;
            this.BudgetPath = budgetPath;
        }

        public Task<IList<RegisteredDevice>> GetRegisteredDevicesAsync()
        {
            if (this._cachedRegisteredDevices == null)
            {
                this._cachedRegisteredDevices = new Lazy<Task<IList<RegisteredDevice>>>(async () =>
                {
                    var dataFolderPath = await this.GetDataFolderPathAsync();

                    var result = new List<RegisteredDevice>();

                    foreach (var deviceFile in await this._fileSystem.GetFilesAsync(YnabPaths.DevicesFolder(dataFolderPath)))
                    {
                        var deviceJson = await this._fileSystem.ReadFileAsync(deviceFile);
                        var device = JObject.Parse(deviceJson);

                        result.Add(new RegisteredDevice(this._fileSystem, this, device));
                    }

                    return result;
                });
            }
            return this._cachedRegisteredDevices.Value;
        }
        
        public async Task<RegisteredDevice> RegisterDevice(string deviceName)
        {
            var alreadyRegisteredDevices = await this.GetRegisteredDevicesAsync();

            var existingDevice = alreadyRegisteredDevices.FirstOrDefault(f => f.FriendlyName == deviceName && f.YnabVersion == Constants.YnabVersion);

            if (existingDevice != null)
                return existingDevice;

            var deviceId = await this.GetNextFreeDeviceIdAsync();
            var deviceGuid = EntityId.CreateNew();

            var json = new JObject
            {
                { "deviceType", "Desktop (Xemio)" },
                { "highestDataVersionImported", "4.2" },
                { "friendlyName", deviceName },
                { "shortDeviceId", deviceId },
                { "hasFullKnowledge", false },
                { "knowledge", Knowledge.CreateKnowledgeForNewDevice(alreadyRegisteredDevices.First(f => f.HasFullKnowledge).KnowledgeString, deviceId) },
                { "deviceGUID", deviceGuid },
                { "knowledgeInFullBudgetFile", null },
                { "YNABVersion", Constants.YnabVersion },
                { "formatVersion", "1.2" },
                { "lastDataVersionFullyKnown", "4.2" }
            };
            
            var dataFolderPath = await this.GetDataFolderPathAsync();
            var deviceFilePath = YnabPaths.DeviceFile(dataFolderPath, deviceId);

            await this._fileSystem.WriteFileAsync(deviceFilePath, json.ToString());
            await this._fileSystem.CreateDirectoryAsync(YnabPaths.DeviceFolder(dataFolderPath, deviceGuid));
            
            var result = new RegisteredDevice(this._fileSystem, this, json);
            (await this.GetRegisteredDevicesAsync()).Add(result);

            return result;
        }
        
        private async Task<string> GetNextFreeDeviceIdAsync()
        {
            var registeredDevices = await this.GetRegisteredDevicesAsync();
            var latestDeviceIdAsCharacter = registeredDevices
                .Select(f => f.ShortDeviceId)
                .OrderByDescending(f => f)
                .First() 
                .ToCharArray()
                .First();

            var nextDeviceIdAsInt = latestDeviceIdAsCharacter + 1;
            var nextDeviceIdAsChar = (char)nextDeviceIdAsInt;

            return nextDeviceIdAsChar.ToString();
        }

        internal Task<string> GetDataFolderPathAsync()
        {
            if (this._cachedDataFolderPath == null)
            {
                this._cachedDataFolderPath = new Lazy<Task<string>>(async () =>
                {
                    var budgetFilePath = YnabPaths.BudgetMetadataFile(this.BudgetPath);

                    var budgetJson = await this._fileSystem.ReadFileAsync(budgetFilePath);
                    var budget = JObject.Parse(budgetJson);

                    var relativeDataFolderPath = budget.Value<string>("relativeDataFolderName");
                    return YnabPaths.DataFolder(this.BudgetPath, relativeDataFolderPath);
                });
            }

            return this._cachedDataFolderPath.Value;
        }
    }
}
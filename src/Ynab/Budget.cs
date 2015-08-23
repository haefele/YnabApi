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

        private bool _hasCachedRegisteredDevices;
        private IList<RegisteredDevice> _cachedRegisteredDevices;
        private bool _hasCachedDataFolderPath;
        private string _cachedDataFolderPath;

        public string BudgetName { get; }
        public string BudgetPath { get; }

        public Budget(IFileSystem fileSystem, string budgetName, string budgetPath)
        {
            this._fileSystem = fileSystem;

            this.BudgetName = budgetName;
            this.BudgetPath = budgetPath;
        }

        public async Task<IList<RegisteredDevice>> GetRegisteredDevicesAsync()
        {
            if (this._hasCachedRegisteredDevices == false)
            { 
                var dataFolderPath = await this.GetDataFolderPathAsync();

                this._cachedRegisteredDevices = new List<RegisteredDevice>();

                foreach (var deviceFile in await this._fileSystem.GetFilesAsync(YnabPaths.DevicesFolder(dataFolderPath)))
                {
                    var deviceJson = await this._fileSystem.ReadFileAsync(deviceFile);
                    var device = JObject.Parse(deviceJson);

                    this._cachedRegisteredDevices.Add(new RegisteredDevice(this._fileSystem, this, device));
                }

                this._hasCachedRegisteredDevices = true;
            }

            return this._cachedRegisteredDevices;
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
                { "knowledge", Knowledge.CreateKnowledgeForNewDevice(alreadyRegisteredDevices, deviceId) },
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

            var deviceJson = await this._fileSystem.ReadFileAsync(deviceFilePath);
            var device = JObject.Parse(deviceJson);

            var result = new RegisteredDevice(this._fileSystem, this, device);
            this._cachedRegisteredDevices.Add(result);

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

        internal async Task<string> GetDataFolderPathAsync()
        {
            if (this._hasCachedDataFolderPath == false)
            { 
                var budgetFilePath = YnabPaths.BudgetMetadataFile(this.BudgetPath);

                var budgetJson = await this._fileSystem.ReadFileAsync(budgetFilePath);
                var budget = JObject.Parse(budgetJson);

                var relativeDataFolderPath = budget.Value<string>("relativeDataFolderName");
                this._cachedDataFolderPath = YnabPaths.DataFolder(this.BudgetPath, relativeDataFolderPath);

                this._hasCachedDataFolderPath = true;
            }

            return this._cachedDataFolderPath;
        }
    }
}
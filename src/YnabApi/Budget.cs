using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YnabApi.Files;
using YnabApi.Helpers;

namespace YnabApi
{
    public class Budget
    {
        private readonly YnabApiSettings _settings;
        
        private Lazy<Task<IList<RegisteredDevice>>> _cachedRegisteredDevices;
        private Lazy<Task<string>> _cachedDataFolderPath;

        public string BudgetName { get; }
        public string BudgetPath { get; }

        public Budget(YnabApiSettings settings, string budgetName, string budgetPath)
        {
            this._settings = settings;

            this.BudgetName = budgetName;
            this.BudgetPath = budgetPath;
        }

        public Task<IList<RegisteredDevice>> GetRegisteredDevicesAsync()
        {
            if (this._cachedRegisteredDevices == null || this._settings.CacheRegisteredDevices == false)
            {
                this._cachedRegisteredDevices = new Lazy<Task<IList<RegisteredDevice>>>(async () =>
                {
                    try
                    {
                        var dataFolderPath = await this.GetDataFolderPathAsync();

                        var result = new List<RegisteredDevice>();

                        foreach (var deviceFile in await this._settings.FileSystem.GetFilesAsync(YnabPaths.DevicesFolder(dataFolderPath)))
                        {
                            var deviceJson = await this._settings.FileSystem.ReadFileAsync(deviceFile);
                            var device = JObject.Parse(deviceJson);

                            result.Add(new RegisteredDevice(this._settings, this, device));
                        }

                        return result;
                    }
                    catch (Exception exception) when (exception is YnabApiException == false)
                    {
                        throw new YnabApiException("Error while loading the registered devices.", exception);
                    }
                });
            }
            return this._cachedRegisteredDevices.Value;
        }
        
        public async Task<RegisteredDevice> RegisterDevice(string deviceName)
        {
            try
            {
                var alreadyRegisteredDevices = await this.GetRegisteredDevicesAsync();

                var existingDevice = alreadyRegisteredDevices.FirstOrDefault(f => 
                    f.FriendlyName == deviceName && 
                    f.YnabVersion == this._settings.ApplicationName && 
                    f.DeviceType == this._settings.DeviceType);

                if (existingDevice != null)
                    return existingDevice;

                var deviceId = await this.GetNextFreeDeviceIdAsync();
                var deviceGuid = EntityId.CreateNew();

                var json = new JObject
                {
                    { "deviceType", this._settings.DeviceType },
                    { "highestDataVersionImported", "4.2" },
                    { "friendlyName", deviceName },
                    { "shortDeviceId", deviceId },
                    { "hasFullKnowledge", false },
                    { "knowledge", Knowledge.CreateKnowledgeForNewDevice(alreadyRegisteredDevices.First(f => f.HasFullKnowledge).KnowledgeString, deviceId) },
                    { "deviceGUID", deviceGuid },
                    { "knowledgeInFullBudgetFile", null },
                    { "YNABVersion", this._settings.ApplicationName },
                    { "formatVersion", "1.2" },
                    { "lastDataVersionFullyKnown", "4.2" }
                };
            
                var dataFolderPath = await this.GetDataFolderPathAsync();
                var deviceFilePath = YnabPaths.DeviceFile(dataFolderPath, deviceId);

                await this._settings.FileSystem.WriteFileAsync(deviceFilePath, json.ToString());
                await this._settings.FileSystem.CreateDirectoryAsync(YnabPaths.DeviceFolder(dataFolderPath, deviceGuid));
            
                var result = new RegisteredDevice(this._settings, this, json);
                (await this.GetRegisteredDevicesAsync()).Add(result);

                await this._settings.FileSystem.FlushWritesAsync();

                return result;
            }
            catch (Exception exception) when (exception is YnabApiException == false)
            {
                throw new YnabApiException($"Error while registering the device {deviceName}.", exception);
            }
        }

        public void ClearCache()
        {
            this._cachedRegisteredDevices = null;
            this._cachedDataFolderPath = null;
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

                    var budgetJson = await this._settings.FileSystem.ReadFileAsync(budgetFilePath);
                    var budget = JObject.Parse(budgetJson);

                    var relativeDataFolderPath = budget.Value<string>("relativeDataFolderName");
                    return YnabPaths.DataFolder(this.BudgetPath, relativeDataFolderPath);
                });
            }

            return this._cachedDataFolderPath.Value;
        }
    }
}
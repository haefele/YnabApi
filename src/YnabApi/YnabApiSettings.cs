using YnabApi.Files;

namespace YnabApi
{
    public class YnabApiSettings
    {
        public YnabApiSettings(IFileSystem fileSystem, string applicationName, string deviceType)
        {
            this.FileSystem = fileSystem;
            this.ApplicationName = applicationName;
            this.DeviceType = deviceType;
            this.CacheBudgets = true;
            this.CacheRegisteredDevices = true;
            this.CacheRegisteredDeviceData = true;
        }

        public IFileSystem FileSystem { get; }
        public string ApplicationName { get; }
        public string DeviceType { get; }
        public bool CacheBudgets { get; set; }
        public bool CacheRegisteredDevices { get; set; }
        public bool CacheRegisteredDeviceData { get; set; }
    }
}
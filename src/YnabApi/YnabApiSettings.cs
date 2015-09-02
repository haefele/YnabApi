using YnabApi.Files;

namespace YnabApi
{
    public class YnabApiSettings
    {
        public YnabApiSettings(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem;
            this.CacheBudgets = true;
            this.CacheRegisteredDevices = true;
            this.CacheRegisteredDeviceData = true;
        }

        public IFileSystem FileSystem { get; }
        public bool CacheBudgets { get; set; }
        public bool CacheRegisteredDevices { get; set; }
        public bool CacheRegisteredDeviceData { get; set; }
    }
}
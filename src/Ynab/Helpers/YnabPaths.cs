using System.IO;

namespace Ynab.Helpers
{
    public static class YnabPaths
    {

        public static string YnabSettingsFile()
        {
            return Path.Combine(".ynabSettings.yroot");
        }

        public static string BudgetMetadataFile(string budgetPath)
        {
            return Path.Combine(budgetPath, "Budget.ymeta");
        }

        public static string DataFolder(string budgetPath, string dataFolderPath)
        {
            return Path.Combine(budgetPath, dataFolderPath);
        }

        public static string DataFolderName(string dataFolderPath)
        {
            return Path.GetFileName(dataFolderPath);
        }

        public static string DeviceFile(string dataFolderPath, string deviceId)
        {
            return Path.Combine(dataFolderPath, "devices", deviceId + ".ydevice");
        }

        public static string DeviceFolder(string dataFolderPath, string deviceGuid)
        {
            return Path.Combine(dataFolderPath, deviceGuid);
        }

        public static string DevicesFolder(string dataFolderPath)
        {
            return Path.Combine(dataFolderPath, "devices");
        }

        public static string YdiffFile(string dataFolderPath, string deviceGuid, string startVersion, string shortDeviceId, int currentKnowledge)
        {
            return Path.Combine(dataFolderPath, deviceGuid, $"{startVersion}_{shortDeviceId}-{currentKnowledge}.ydiff");
        }
    }
}
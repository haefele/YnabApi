using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YnabApi.Desktop;
using YnabApi.DeviceActions;
using YnabApi.Dropbox;

namespace YnabApi.Tests.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var watch = Stopwatch.StartNew();

            var dropboxFileSystem = new DropboxFileSystem("");
            var desktopFileSystem = new DesktopFileSystem();

            YnabApi api = new YnabApi(desktopFileSystem);
            var budgets = await api.GetBudgetsAsync();

            var testBudget = budgets.First(f => f.BudgetName == "Test-Budget");

            var registeredDevice = await testBudget.RegisterDevice(Environment.MachineName);

            var allDevices = await testBudget.GetRegisteredDevicesAsync();
            var fullKnowledgeDevice = allDevices.First(f => f.HasFullKnowledge);

            var createPayee = new CreatePayeeDeviceAction
            {
                Name = "Bücherei"
            };
            var createTransaction = new CreateTransactionDeviceAction
            {
                Amount = -20.0m,
                Account = (await fullKnowledgeDevice.GetAccountsAsync()).First(f => f.Name == "Geldbeutel"),
                Category = null,
                Memo = "#Mittagspause",
                Payee = createPayee
            };

            await registeredDevice.ExecuteActions(createPayee, createTransaction);

            watch.Stop();
            System.Console.WriteLine(watch.Elapsed);

            System.Console.ReadLine();
        }
    }
}

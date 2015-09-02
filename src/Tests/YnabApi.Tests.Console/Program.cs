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
        private static Stopwatch _watch;

        private static void TimeMeasureCheckpoint(string title = null)
        {
            if (_watch != null)
                System.Console.WriteLine($"{title}{_watch.Elapsed}");

            _watch = Stopwatch.StartNew();
        }

        public static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var dropboxFileSystem = new DropboxFileSystem("");

            var settings = new YnabApiSettings(dropboxFileSystem);
            settings.CacheBudgets = false;

            YnabApi api = new YnabApi(settings);

            TimeMeasureCheckpoint();
            var budgets = await api.GetBudgetsAsync();
            TimeMeasureCheckpoint("Get Budgets: ");
            var testBudget = budgets.First(f => f.BudgetName == "Test-Budget");
            TimeMeasureCheckpoint("LINQ Get Budget in Memory: ");
            var registeredDevice = await testBudget.RegisterDevice("My-Test-Device");
            TimeMeasureCheckpoint("Register Device: ");
            var allDevices = await testBudget.GetRegisteredDevicesAsync();
            TimeMeasureCheckpoint("Get Registered Devices: ");
            var fullKnowledgeDevice = allDevices.First(f => f.HasFullKnowledge);
            TimeMeasureCheckpoint("Get Full Knowledge Device: ");
            await fullKnowledgeDevice.GetPayeesAsync();
            TimeMeasureCheckpoint("Get Payees: ");
            //var createPayee = new CreatePayeeDeviceAction
            //{
            //    Name = "Bücherei"
            //};
            //var createTransaction = new CreateTransactionDeviceAction
            //{
            //    Amount = -20.0m,
            //    Account = (await fullKnowledgeDevice.GetAccountsAsync()).First(f => f.Name == "Geldbeutel"),
            //    Category = null,
            //    Memo = "#Mittagspause",
            //    Payee = createPayee
            //};

            //await registeredDevice.ExecuteActions(createPayee, createTransaction);

            System.Console.ReadLine();
        }
    }
}

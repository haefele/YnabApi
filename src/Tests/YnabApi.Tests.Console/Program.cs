using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YnabApi.Desktop;
using YnabApi.DeviceActions;
using YnabApi.Dropbox;
using YnabApi.Extensions;

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

            var settings = new YnabApiSettings(new DesktopFileSystem());

            YnabApi api = new YnabApi(settings);
            
            var budgets = await api.GetBudgetsAsync();

            var testBudget = budgets.First(f => f.BudgetName == "Test-Budget");

            var allDevices = await testBudget.GetRegisteredDevicesAsync();
            var fullKnowledgeDevice = allDevices.First(f => f.HasFullKnowledge);
            var categories = (await fullKnowledgeDevice.GetCategoriesAsync()).OnlyActive();
            var transactions = await fullKnowledgeDevice.GetTransactionsAsync();

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

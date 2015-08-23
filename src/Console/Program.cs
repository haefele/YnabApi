using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ynab;
using Ynab.Desktop;
using Ynab.Dropbox;
using Ynab.Items;

namespace Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            Run().Wait();
        }

        private static async Task Run()
        {
            var dropboxFileSystem = new DropboxFileSystem("");
            var desktopFileSystem = new DesktopFileSystem();

            YnabApi api = new YnabApi(dropboxFileSystem);
            var budgets = await api.GetBudgetsAsync();

            var testBudget = budgets.First(f => f.BudgetName == "Test-Budget");
            var devices = await testBudget.GetRegisteredDevicesAsync();

            var registeredDevice = await testBudget.RegisterDevice(Environment.MachineName);

            var transaction = new Transaction
            {
                Amount = -20.0m,
                AccountId = "ACCOUNT-ID-1",
                CategoryId = "CATEGORY-ID-1",
                Memo = "#Mittagspause",
                PayeeId = "PAYEE-ID-1"
            };

            await registeredDevice.InsertItems(transaction);
        }
    }
}

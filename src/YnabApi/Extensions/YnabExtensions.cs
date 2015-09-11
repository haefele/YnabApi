using System.Collections.Generic;
using System.Linq;
using YnabApi.Items;

namespace YnabApi.Extensions
{
    public static class YnabExtensions
    {
        public static IEnumerable<MasterCategory> OnlyActive(this IEnumerable<MasterCategory> categories)
        {
            string[] invalidMasterCategoryIds =
            {
                "MasterCategory/__Hidden__"
            };

            return categories
                .Where(f => f.IsTombstone == false)
                .Where(f => invalidMasterCategoryIds.Contains(f.Id) == false);
        }

        public static IEnumerable<Category> OnlyActive(this IEnumerable<Category> categories)
        {
            string[] invalidCategoryIds = 
            {
                "Category/__ImmediateIncome__",
                "Category/__DeferredIncome__",
                "Category/__Split__",
            };

            return categories
                .Where(f => f.IsTombstone == false)
                .Where(f => invalidCategoryIds.Contains(f.Id) == false);
        }

        public static IEnumerable<Payee> OnlyActive(this IEnumerable<Payee> payees)
        {
            return payees
                .Where(f => f.IsTombstone == false)
                .Where(f => f.Id.StartsWith("Payee/Transfer:") == false);
        }

        public static IEnumerable<Transaction> OnlyActive(this IEnumerable<Transaction> transactions)
        {
            return transactions
                .Where(f => f.IsTombstone == false);
        }
    }
}
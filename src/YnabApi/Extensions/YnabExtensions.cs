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
                Constants.MasterCategory.Hidden
            };

            return categories
                .Where(f => f.IsTombstone == false)
                .Where(f => invalidMasterCategoryIds.Contains(f.Id) == false);
        }

        public static IEnumerable<Category> OnlyActive(this IEnumerable<Category> categories)
        {
            string[] systemCategoryIds = 
            {
                Constants.Category.ImmediateIncomeId,
                Constants.Category.DeferredIncomeId,
                Constants.Category.SplitId,
            };

            return categories
                .Where(f => f.IsTombstone == false)
                .Where(f => systemCategoryIds.Contains(f.Id) == false);
        }

        public static IEnumerable<Payee> WithoutTransfers(this IEnumerable<Payee> payees)
        {
            return payees
                .Where(f => f.Id.StartsWith(Constants.Payee.TransferIdStart) == false);
        }

        public static IEnumerable<Payee> OnlyActive(this IEnumerable<Payee> payees)
        {
            return payees
                .Where(f => f.IsTombstone == false);
        }

        public static IEnumerable<Transaction> OnlyActive(this IEnumerable<Transaction> transactions)
        {
            return transactions
                .Where(f => f.IsTombstone == false);
        }
    }
}
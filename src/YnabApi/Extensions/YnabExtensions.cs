using System.Collections.Generic;
using System.Linq;
using YnabApi.Items;

namespace YnabApi.Extensions
{
    public static class YnabExtensions
    {
        public static IEnumerable<Category> OnlyActive(this IEnumerable<Category> categories)
        {
            return categories
                .Where(f => f.IsTombstone == false)
                .Where(f => f.MasterId != "MasterCategory/__Hidden__");
        }
    }
}
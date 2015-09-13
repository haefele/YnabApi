namespace YnabApi
{
    public static class Constants
    {
        public static class MasterCategory
        {
            public const string System = "System";
            public const string SystemId = "Category/__System__";

            public const string Hidden = "MasterCategory/__Hidden__";
        }

        public static class Category
        {
            public const string ImmediateIncome = "Income for this month";
            public const string ImmediateIncomeId = "Category/__ImmediateIncome__";
            public const string DeferredIncome = "Income for next month";
            public const string DeferredIncomeId = "Category/__DeferredIncome__";
            public const string Split = "Split";
            public const string SplitId = "Category/__Split__";
        }

        public static class Payee
        {
            public const string TransferIdStart = "Payee/Transfer:";
        }

        public static class Ynab
        {
            public const string FormatVersion = "1.2";
            public const string LastDataVersionFullyKnown = "4.2";
        }
    }
}
using System;
using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Transaction : IHaveTransactionId, IHaveAccountId, IHaveCategoryId, IHavePayeeId
    {
        private readonly JObject _transaction;

        public Transaction(JObject transaction)
        {
            this._transaction = transaction;

            this.Id = transaction.Value<string>("entityId");
            this.AccountId = transaction.Value<string>("accountId");
            this.Amount = transaction.Value<decimal>("amount");
            this.CategoryId = transaction.Value<string>("categoryId");
            this.Cleared = transaction.Value<string>("cleared") == "Cleared";
            this.Date = transaction.Value<DateTime>("date");
            this.PayeeId = transaction.Value<string>("payeeId");
            this.Memo = transaction.Value<string>("memo");
        }

        public string Id { get; }
        public string AccountId { get; }
        public decimal Amount { get; }
        public string CategoryId { get; }
        public bool Cleared { get; }
        public DateTime Date { get; }
        public string PayeeId { get; }
        public string Memo { get; }


        string IHaveTransactionId.Id => this.Id;
        string IHaveAccountId.Id => this.AccountId;
        string IHaveCategoryId.Id => this.CategoryId;
        string IHavePayeeId.Id => this.PayeeId;

        public override string ToString()
        {
            return $"{this.Date}: {this.Amount}";
        }
    }
}
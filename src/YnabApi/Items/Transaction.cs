﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Transaction : IHaveTransactionId
    {
        private readonly JObject _transaction;

        internal Transaction(JObject transaction, IList<Account> allAccounts, IList<Category> allCategories, IList<Payee> allPayees)
        {
            this._transaction = transaction;

            this.Id = transaction.Value<string>("entityId");
            this.Account = allAccounts.FirstOrDefault(f => f.Id == transaction.Value<string>("accountId"));
            this.Amount = transaction.Value<decimal>("amount");
            this.Category = allCategories.FirstOrDefault(f => f.Id == transaction.Value<string>("categoryId"));
            this.Cleared = transaction.Value<string>("cleared") == "Cleared";
            this.Date = transaction.Value<DateTime>("date");
            this.Payee = allPayees.FirstOrDefault(f => f.Id == transaction.Value<string>("payeeId"));
            this.Memo = transaction.Value<string>("memo");
        }

        public string Id { get; }
        public Account Account { get; }
        public decimal Amount { get; }
        public Category Category { get; }
        public bool Cleared { get; }
        public DateTime Date { get; }
        public Payee Payee { get; }
        public string Memo { get; }
        
        string IHaveTransactionId.Id => this.Id;

        public override string ToString()
        {
            return $"{this.Date}: {this.Amount}";
        }


        internal JObject GetJson() => (JObject)this._transaction.DeepClone();
    }
}
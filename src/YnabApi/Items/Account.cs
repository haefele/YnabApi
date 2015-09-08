using System;
using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Account : IHaveAccountId, IEquatable<Account>
    {
        private readonly JObject _account;

        internal Account(JObject account)
        {
            this._account = account;

            this.Id = account.Value<string>("entityId");
            this.Name = account.Value<string>("accountName");
            this.OnBudget = account.Value<bool>("onBudget");
        }

        public string Id { get; }
        public string Name { get; }
        public bool OnBudget { get; }

        string IHaveAccountId.Id => this.Id;
        
        internal JObject GetJson() => (JObject)this._account.DeepClone();

        public override string ToString()
        {
            return this.Name;
        }

        public bool Equals(Account other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(this.Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != this.GetType())
                return false;

            return this.Equals((Account)obj);
        }

        public override int GetHashCode()
        {
            return this.Id?.GetHashCode() ?? 0;
        }
    }
}
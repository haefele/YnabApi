using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Account : IHaveAccountId
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

        public override string ToString()
        {
            return this.Name;
        }

        internal JObject GetJson() => (JObject)this._account.DeepClone();
    }
}
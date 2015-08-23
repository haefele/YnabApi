using Newtonsoft.Json.Linq;
using Ynab.DeviceActions;

namespace Ynab.Items
{
    public class Account : IHaveAccountId
    {
        private readonly JObject _account;

        public Account(JObject account)
        {
            this._account = account;

            this.Id = account.Value<string>("entityId");
            this.Name = account.Value<string>("accountName");
            this.OnBudget = account.Value<bool>("onBudget");
        }

        public string Id { get; }
        public string Name { get; }
        public bool OnBudget { get; }
    }
}
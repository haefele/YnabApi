using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Payee : IHavePayeeId
    {
        private readonly JObject _payee;

        internal Payee(JObject payee)
        {
            this._payee = payee;

            this.Id = payee.Value<string>("entityId");
            this.Name = payee.Value<string>("name");
        }

        public string Id { get; }
        public string Name { get; }

        string IHavePayeeId.Id => this.Id;

        public override string ToString()
        {
            return this.Name;
        }


        internal JObject GetJson() => (JObject)this._payee.DeepClone();
    }
}
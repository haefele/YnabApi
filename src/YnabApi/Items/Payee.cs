using System;
using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Payee : IHavePayeeId, IEquatable<Payee>
    {
        private readonly JObject _payee;

        internal Payee(JObject payee)
        {
            this._payee = payee;

            this.Id = payee.Value<string>("entityId");
            this.Name = payee.Value<string>("name");
            this.IsTombstone = payee.Value<bool>("isTombstone");
        }

        public string Id { get; }
        public string Name { get; }
        public bool IsTombstone { get; }

        string IHavePayeeId.Id => this.Id;

        internal JObject GetJson() => (JObject)this._payee.DeepClone();

        public override string ToString()
        {
            return this.Name;
        }

        public bool Equals(Payee other)
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

            return this.Equals((Payee)obj);
        }

        public override int GetHashCode()
        {
            return this.Id?.GetHashCode() ?? 0;
        }
    }
}
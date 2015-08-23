﻿using Newtonsoft.Json.Linq;
using Ynab.DeviceActions;

namespace Ynab.Items
{
    public class Payee : IHavePayeeId
    {
        private readonly JObject _payee;

        public Payee(JObject payee)
        {
            this._payee = payee;

            this.Id = payee.Value<string>("entityId");
            this.Name = payee.Value<string>("name");
        }

        public string Id { get; }
        public string Name { get; }
    }
}
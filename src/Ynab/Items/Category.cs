using Newtonsoft.Json.Linq;
using Ynab.DeviceActions;

namespace Ynab.Items
{
    public class Category : IHaveCategoryId
    {
        private readonly JObject _category;
        private readonly JObject _masterCategory;

        public Category(JObject category, JObject masterCategory)
        {
            this._category = category;
            this._masterCategory = masterCategory;

            this.Id = category.Value<string>("entityId");
            this.Name = category.Value<string>("name");

            this.MasterId = masterCategory.Value<string>("entityId");
            this.MasterName = masterCategory.Value<string>("name");
        }

        public string Id { get; }
        public string Name { get; }

        public string MasterId { get; }
        public string MasterName { get; }
    }
}
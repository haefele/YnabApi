using Newtonsoft.Json.Linq;
using YnabApi.DeviceActions;

namespace YnabApi.Items
{
    public class Category : IHaveCategoryId
    {
        private readonly JObject _category;
        private readonly JObject _masterCategory;

        internal Category(JObject category, JObject masterCategory)
        {
            this._category = category;
            this._masterCategory = masterCategory;

            this.Id = category.Value<string>("entityId");
            this.Name = category.Value<string>("name");
            this.IsTombstone = category.Value<bool>("isTombstone");

            this.MasterId = masterCategory.Value<string>("entityId");
            this.MasterName = masterCategory.Value<string>("name");

        }

        internal Category(string id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.IsTombstone = true;

            this.MasterId = id;
            this.MasterName = name;
        }

        public string Id { get; }
        public string Name { get; }

        public string MasterId { get; }
        public string MasterName { get; }

        public bool IsTombstone { get; }

        string IHaveCategoryId.Id => this.Id;

        public override string ToString()
        {
            return $"{this.MasterName} - {this.Name}";
        }
    }
}
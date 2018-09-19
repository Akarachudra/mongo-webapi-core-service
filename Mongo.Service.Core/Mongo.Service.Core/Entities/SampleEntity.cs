using Mongo.Service.Core.Entities.Base;
using Mongo.Service.Core.Repository.Attributes;

namespace Mongo.Service.Core.Entities
{
    [CollectionName("Sample")]
    public class SampleEntity : BaseEntity
    {
        public string SomeData { get; set; }
    }
}
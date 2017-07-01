using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;

namespace Mongo.Service.Core.Storable
{
    [CollectionName("Sample")]
    public class SampleEntity : BaseEntity
    {
        public string SomeData { get; set; }
    }
}
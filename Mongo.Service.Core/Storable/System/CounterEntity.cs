using Mongo.Service.Core.Storage;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Service.Core.Storable.System
{
    [CollectionName("Counter")]
    public class CounterEntity
    {
        [BsonId]
        public string Id { get; set; }
        public long CurrentTicks { get; set; }
    }
}
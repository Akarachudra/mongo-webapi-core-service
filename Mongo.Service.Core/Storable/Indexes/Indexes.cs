using Mongo.Service.Core.Storable.Base;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storable.Indexes
{
    public class Indexes<TEntity> : IIndexes<TEntity> where TEntity : IBaseEntity
    {
        public virtual void CreateIndexes(IMongoCollection<TEntity> collection)
        {
            collection.Indexes.CreateOne(new BsonDocumentIndexKeysDefinition<TEntity>(new BsonDocument()).Descending(x => x.Ticks),
                                         new CreateIndexOptions { Background = true, Unique = true });
        }
    }
}
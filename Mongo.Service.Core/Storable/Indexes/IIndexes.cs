using Mongo.Service.Core.Storable.Base;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storable.Indexes
{
    public interface IIndexes<TEntity> where TEntity : IBaseEntity
    {
        void CreateIndexes(IMongoCollection<TEntity> collection);
    }
}
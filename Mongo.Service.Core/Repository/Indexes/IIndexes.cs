using Mongo.Service.Core.Entities.Base;
using MongoDB.Driver;

namespace Mongo.Service.Core.Repository.Indexes
{
    public interface IIndexes<TEntity>
        where TEntity : IBaseEntity
    {
        void CreateIndexes(IMongoCollection<TEntity> collection);
    }
}
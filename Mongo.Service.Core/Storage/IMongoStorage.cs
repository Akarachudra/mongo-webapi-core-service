using Mongo.Service.Core.Storable.Base;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public interface IMongoStorage
    {
        IMongoCollection<TEntity> GetCollection<TEntity>() where TEntity : IBaseEntity;
        void DropCollection<T>();
    }
}
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storable.System;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public interface IMongoStorage
    {
        IMongoCollection<TEntity> GetCollection<TEntity>(out string collectionName) where TEntity : IBaseEntity;
        void DropCollection<T>();
        IMongoCollection<CounterEntity> GetSyncCollection();
    }
}
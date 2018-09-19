using Mongo.Service.Core.Entities.Base;
using MongoDB.Driver;

namespace Mongo.Service.Core.Repository
{
    public interface IMongoStorage
    {
        IMongoCollection<TEntity> GetCollection<TEntity>()
            where TEntity : IBaseEntity;

        void DropCollection<TEntity>()
            where TEntity : IBaseEntity;

        void ClearCollection<TEntity>()
            where TEntity : IBaseEntity;
    }
}
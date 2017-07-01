using System;
using System.Linq.Expressions;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Storable.System;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public class EntityStorage<TEntity> : IEntityStorage<TEntity> where TEntity : IBaseEntity
    {
        private readonly IMongoCollection<CounterEntity> syncCollection;
        private readonly string collectionName;

        public EntityStorage(IMongoStorage mongoStorage, IIndexes<TEntity> indexes)
        {
            Collection = mongoStorage.GetCollection<TEntity>(out collectionName);
            syncCollection = mongoStorage.GetSyncCollection();
            indexes.CreateIndexes(Collection);
        }

        public IMongoCollection<TEntity> Collection { get; }

        public TEntity Read(Guid id)
        {
            var entity = Collection.FindSync(x => x.Id == id).FirstOrDefault();
            if (entity == null)
            {
                throw new Exception($"{typeof(TEntity).Name} with id {id} not found");
            }
            return entity;
        }

        public TEntity[] Read(Expression<Func<TEntity, bool>> filter)
        {
            var entities = Collection.FindSync(filter).ToList();
            return entities.ToArray();
        }

        public TEntity[] Read(int skip, int limit)
        {
            throw new NotImplementedException();
        }

        public TEntity[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            throw new NotImplementedException();
        }

        public bool TryRead(Guid id, out TEntity outEntity)
        {
            var entity = Collection.FindSync(x => x.Id == id).FirstOrDefault();
            if (entity == null)
            {
                outEntity = default(TEntity);
                return false;
            }
            outEntity = entity;
            return true;
        }

        public TEntity[] ReadAll()
        {
            throw new NotImplementedException();
        }

        public Guid[] ReadIds(Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public long ReadSyncedData(long lastSync, out TEntity[] newData, out TEntity[] deletedData,
                                   Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Write(TEntity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            entity.LastModified = DateTime.UtcNow;
            Collection.ReplaceOne(x => x.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
        }

        public void Write(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                Write(entity);
            }
        }

        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Remove(Guid[] ids)
        {
            throw new NotImplementedException();
        }

        public void Remove(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Remove(TEntity[] entities)
        {
            throw new NotImplementedException();
        }

        public long Count()
        {
            throw new NotImplementedException();
        }

        public long Count(Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public long GetLastTick()
        {
            throw new NotImplementedException();
        }
    }
}
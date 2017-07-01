using System;
using System.Linq;
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
            var entities = Collection.Aggregate().Skip(skip).Limit(limit).ToList();
            return entities.ToArray();
        }

        public TEntity[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = Collection.Aggregate().Match(filter).Skip(skip).Limit(limit).ToList();
            return entities.ToArray();
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
            var entities = Collection.FindSync(FilterDefinition<TEntity>.Empty).ToList();
            return entities.ToArray();
        }

        public Guid[] ReadIds(Expression<Func<TEntity, bool>> filter)
        {
            var ids = Collection.FindSync(filter).ToList().Select(x => x.Id).ToArray();
            return ids;
        }

        public long ReadSyncedData(long lastSync, out TEntity[] newData, out TEntity[] deletedData,
                                   Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Guid id)
        {
            return Collection.FindSync(x => x.Id == id).FirstOrDefault() != null;
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
            var entity = Read(id);
            entity.IsDeleted = true;

            Write(entity);
        }

        public void Remove(Guid[] ids)
        {
            foreach (var id in ids)
            {
                Remove(id);
            }
        }

        public void Remove(TEntity entity)
        {
            Remove(entity.Id);
        }

        public void Remove(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                Remove(entity.Id);
            }
        }

        public long Count()
        {
            return Collection.Count(FilterDefinition<TEntity>.Empty);
        }

        public long Count(Expression<Func<TEntity, bool>> filter)
        {
            return Collection.Count(filter);
        }

        public long GetLastTick()
        {
            throw new NotImplementedException();
        }
    }
}
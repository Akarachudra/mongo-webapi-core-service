using System;
using System.Linq;
using System.Linq.Expressions;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Extensions;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public class EntityStorage<TEntity> : IEntityStorage<TEntity>
        where TEntity : IBaseEntity
    {
        private const int TicksWriteTries = 100;

        public EntityStorage(IMongoStorage mongoStorage, IIndexes<TEntity> indexes)
        {
            Collection = mongoStorage.GetCollection<TEntity>();
            indexes.CreateIndexes(Collection);
        }

        public IMongoCollection<TEntity> Collection { get; }

        public UpdateDefinitionBuilder<TEntity> Updater => Builders<TEntity>.Update;

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
            var newLastSync = GetLastTick();

            Expression<Func<TEntity, bool>> newFilter = x => !x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;
            Expression<Func<TEntity, bool>> deletedFilter = x => x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;

            if (additionalFilter != null)
            {
                newFilter = newFilter.And(additionalFilter);
                deletedFilter = deletedFilter.And(additionalFilter);
            }

            newData = Read(newFilter);
            deletedData = Read(deletedFilter);

            return newLastSync;
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
            else
            {
                TEntity currentEntity;
                var exists = TryRead(entity.Id, out currentEntity);
                if (exists && currentEntity.IsDeleted)
                {
                    entity.IsDeleted = true;
                }
            }
            entity.LastModified = DateTime.UtcNow;

            TryWriteSyncedEntity(entity);
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
            var sort = Builders<TEntity>.Sort.Descending(x => x.Ticks);
            var result = Collection.Find(FilterDefinition<TEntity>.Empty).Sort(sort).Limit(1).ToList();
            return result.Count == 0 ? 0 : result[0].Ticks;
        }

        public void UpdateTicks(Guid id)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = GetLastTick() + 1;
                    var updateTicks = Builders<TEntity>.Update.Set(x => x.Ticks, lastTicks);
                    Collection.UpdateOne(x => x.Id == id, updateTicks);
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception($"Update ticks tries of {nameof(TEntity)} limit exceeded");
        }

        public void Update(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition)
        {
            Collection.UpdateOne(filter, updateDefinition);
        }

        public void UpdateWithTicks(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = GetLastTick() + 1;
                    var updateWithTicks = updateDefinition.Set(x => x.Ticks, lastTicks);
                    Collection.UpdateOne(filter, updateWithTicks);
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception($"Update with ticks tries of {nameof(TEntity)} limit exceeded");
        }

        private void TryWriteSyncedEntity(TEntity entity)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                entity.Ticks = GetLastTick() + 1;

                try
                {
                    Collection.ReplaceOne(x => x.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception("Write tries limit exceeded.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Extensions;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storable.Indexes;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public class MongoRepository<TEntity> : IMongoRepository<TEntity>
        where TEntity : IBaseEntity
    {
        private const int TicksWriteTries = 100;

        public MongoRepository(IMongoStorage mongoStorage, IIndexes<TEntity> indexes)
        {
            this.Collection = mongoStorage.GetCollection<TEntity>();
            indexes.CreateIndexes(this.Collection);
        }

        public IMongoCollection<TEntity> Collection { get; }

        public UpdateDefinitionBuilder<TEntity> Updater => Builders<TEntity>.Update;

        public async Task<TEntity> ReadAsync(Guid id)
        {
            var entity = (await this.Collection.FindAsync(x => x.Id == id).ConfigureAwait(false)).FirstOrDefault();
            if (entity == null)
            {
                throw new Exception($"{typeof(TEntity).Name} with id {id} not found");
            }

            return entity;
        }

        public async Task<IList<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> filter)
        {
            var entities = await
                (await this.Collection.FindAsync(filter).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
            return entities;
        }

        public async Task<IList<TEntity>> ReadAsync(int skip, int limit)
        {
            var entities = await this.Collection.Aggregate().Skip(skip).Limit(limit).ToListAsync().ConfigureAwait(false);
            return entities;
        }

        public async Task<IList<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = await this.Collection.Aggregate().Match(filter).Skip(skip).Limit(limit).ToListAsync().ConfigureAwait(false);
            return entities;
        }

        public async Task<IList<TEntity>> ReadAllAsync()
        {
            var entities = await
                (await this.Collection.FindAsync(FilterDefinition<TEntity>.Empty).ConfigureAwait(false)).ToListAsync().ConfigureAwait(false);
            return entities;
        }

        public long ReadSyncedData(
            long lastSync,
            out TEntity[] newData,
            out TEntity[] deletedData,
            Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var newLastSync = this.GetLastTick();

            Expression<Func<TEntity, bool>> newFilter = x => !x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;
            Expression<Func<TEntity, bool>> deletedFilter = x => x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;

            if (additionalFilter != null)
            {
                newFilter = newFilter.And(additionalFilter);
                deletedFilter = deletedFilter.And(additionalFilter);
            }

            newData = this.ReadAsync(newFilter);
            deletedData = this.ReadAsync(deletedFilter);

            return newLastSync;
        }

        public bool Exists(Guid id)
        {
            return this.Collection.FindSync(x => x.Id == id).FirstOrDefault() != null;
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
                var exists = this.TryRead(entity.Id, out currentEntity);
                if (exists && currentEntity.IsDeleted)
                {
                    entity.IsDeleted = true;
                }
            }

            entity.LastModified = DateTime.UtcNow;

            this.TryWriteSyncedEntity(entity);
        }

        public void Write(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                this.Write(entity);
            }
        }

        public void Remove(Guid id)
        {
            var entity = this.ReadAsync(id);
            entity.IsDeleted = true;

            this.Write(entity);
        }

        public void Remove(Guid[] ids)
        {
            foreach (var id in ids)
            {
                this.Remove(id);
            }
        }

        public void Remove(TEntity entity)
        {
            this.Remove(entity.Id);
        }

        public void Remove(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                this.Remove(entity.Id);
            }
        }

        public long Count()
        {
            return this.Collection.Count(FilterDefinition<TEntity>.Empty);
        }

        public long Count(Expression<Func<TEntity, bool>> filter)
        {
            return this.Collection.Count(filter);
        }

        public long GetLastTick()
        {
            var sort = Builders<TEntity>.Sort.Descending(x => x.Ticks);
            var result = this.Collection.Find(FilterDefinition<TEntity>.Empty).Sort(sort).Limit(1).ToList();
            return result.Count == 0 ? 0 : result[0].Ticks;
        }

        public void UpdateTicks(Guid id)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = this.GetLastTick() + 1;
                    var updateTicks = Builders<TEntity>.Update.Set(x => x.Ticks, lastTicks);
                    this.Collection.UpdateOne(x => x.Id == id, updateTicks);
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
            this.Collection.UpdateOne(filter, updateDefinition);
        }

        public void UpdateWithTicks(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = this.GetLastTick() + 1;
                    var updateWithTicks = updateDefinition.Set(x => x.Ticks, lastTicks);
                    this.Collection.UpdateOne(filter, updateWithTicks);
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
                entity.Ticks = this.GetLastTick() + 1;

                try
                {
                    this.Collection.ReplaceOne(x => x.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
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
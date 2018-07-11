using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<SyncResult<TEntity>> ReadSyncedDataAsync(
            long lastSync,
            Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var syncResult = new SyncResult<TEntity>();
            var newLastSync = this.GetLastTick();

            Expression<Func<TEntity, bool>> newFilter = x => !x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;
            Expression<Func<TEntity, bool>> deletedFilter = x => x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;

            if (additionalFilter != null)
            {
                newFilter = newFilter.And(additionalFilter);
                deletedFilter = deletedFilter.And(additionalFilter);
            }

            syncResult.LastSync = newLastSync;
            syncResult.NewData = await this.ReadAsync(newFilter).ConfigureAwait(false);
            syncResult.DeletedData = await this.ReadAsync(deletedFilter).ConfigureAwait(false);

            return syncResult;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return (await this.Collection.FindAsync(x => x.Id == id).ConfigureAwait(false)).FirstOrDefault() != null;
        }

        public async Task WriteAsync(TEntity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            else
            {
                var currentEntity = (await this.ReadAsync(x => x.Id == entity.Id).ConfigureAwait(false)).FirstOrDefault();
                if (currentEntity != null && currentEntity.IsDeleted)
                {
                    entity.IsDeleted = true;
                }
            }

            entity.LastModified = DateTime.UtcNow;

            this.TryWriteSyncedEntity(entity);
        }

        public async Task WriteAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                await this.WriteAsync(entity).ConfigureAwait(false);
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            var entity = await this.ReadAsync(id).ConfigureAwait(false);
            entity.IsDeleted = true;

            await this.WriteAsync(entity).ConfigureAwait(false);
        }

        public async Task RemoveAsync(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                await this.RemoveAsync(id).ConfigureAwait(false);
            }
        }

        public async Task RemoveAsync(TEntity entity)
        {
            await this.RemoveAsync(entity.Id).ConfigureAwait(false);
        }

        public async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                await this.RemoveAsync(entity.Id).ConfigureAwait(false);
            }
        }

        public async Task<long> CountAsync()
        {
            return await this.Collection.CountAsync(FilterDefinition<TEntity>.Empty).ConfigureAwait(false);
        }

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await this.Collection.CountAsync(filter).ConfigureAwait(false);
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
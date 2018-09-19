using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Entities.Base;
using MongoDB.Driver;

namespace Mongo.Service.Core.Repository
{
    public interface IMongoRepository<TEntity>
        where TEntity : IBaseEntity
    {
        IMongoCollection<TEntity> Collection { get; }

        UpdateDefinitionBuilder<TEntity> Updater { get; }

        Task<TEntity> ReadAsync(Guid id);

        Task<IList<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> filter);

        Task<IList<TEntity>> ReadAsync(int skip, int limit);

        Task<IList<TEntity>> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit);

        Task<IList<TEntity>> ReadAllAsync();

        Task<SyncResult<TEntity>> ReadSyncedDataAsync(
            long lastSync,
            Expression<Func<TEntity, bool>> additionalFilter = null);

        Task<bool> ExistsAsync(Guid id);

        Task WriteAsync(TEntity entity);

        Task WriteAsync(IEnumerable<TEntity> entities);

        Task RemoveAsync(Guid id);

        Task RemoveAsync(IEnumerable<Guid> ids);

        Task RemoveAsync(TEntity entity);

        Task RemoveAsync(IEnumerable<TEntity> entities);

        Task<long> CountAsync();

        Task<long> CountAsync(Expression<Func<TEntity, bool>> filter);

        Task<long> GetLastTickAsync();

        Task UpdateTicksAsync(Guid id);

        Task UpdateAsync(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition);

        Task UpdateWithTicksAsync(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition);
    }
}
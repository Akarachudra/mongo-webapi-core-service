using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Entities.Base;
using Mongo.Service.Core.Repository;
using Mongo.Service.Core.Types;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services
{
    public interface IEntityService<TApi, TEntity>
        where TApi : IApiBase
        where TEntity : IBaseEntity
    {
        IMongoRepository<TEntity> Repository { get; }

        Task<TApi> ReadAsync(Guid id);

        Task<IList<TApi>> ReadAsync(int skip, int limit);

        Task<IList<TApi>> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit);

        Task<IList<TApi>> ReadAsync(Expression<Func<TEntity, bool>> filter);

        Task<IList<TApi>> ReadAllAsync();

        Task<ApiSync<TApi>> ReadSyncedDataAsync(
            long lastSync,
            Expression<Func<TEntity, bool>> additionalFilter = null);

        Task<bool> ExistsAsync(Guid id);

        Task WriteAsync(TApi apiEntity);

        Task WriteAsync(IEnumerable<TApi> apiEntities);

        Task RemoveAsync(Guid id);

        Task RemoveAsync(IEnumerable<Guid> ids);

        Task RemoveAsync(TApi apiEntity);

        Task RemoveAsync(IEnumerable<TApi> apiEntities);
    }
}
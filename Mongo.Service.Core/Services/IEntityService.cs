using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;
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

        long ReadSyncedData(
            long lastSync,
            out TApi[] newData,
            out Guid[] deletedData,
            Expression<Func<TEntity, bool>> additionalFilter = null);

        bool Exists(Guid id);

        void Write(TApi apiEntity);

        void Write(TApi[] apiEntities);

        void Remove(Guid id);

        void Remove(Guid[] ids);

        void Remove(TApi apiEntity);

        void Remove(TApi[] apiEntities);
    }
}
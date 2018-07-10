using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services
{
    public class EntityService<TApi, TEntity> : IEntityService<TApi, TEntity>
        where TApi : IApiBase
        where TEntity : IBaseEntity
    {
        private readonly IMapper<TApi, TEntity> mapper;

        public EntityService(IMongoRepository<TEntity> repository, IMapper<TApi, TEntity> mapper)
        {
            this.Repository = repository;
            this.mapper = mapper;
        }

        public IMongoRepository<TEntity> Repository { get; }

        public virtual async Task<TApi> ReadAsync(Guid id)
        {
            var entity = await this.Repository.ReadAsync(id).ConfigureAwait(false);
            return this.mapper.GetApiFromEntity(entity);
        }

        public virtual async Task<IList<TApi>> ReadAsync(int skip, int limit)
        {
            var entities = await this.Repository.ReadAsync(skip, limit).ConfigureAwait(false);
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<IList<TApi>> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = await this.Repository.ReadAsync(filter, skip, limit).ConfigureAwait(false);
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<IList<TApi>> ReadAsync(Expression<Func<TEntity, bool>> filter)
        {
            var entities = await this.Repository.ReadAsync(filter).ConfigureAwait(false);
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<IList<TApi>> ReadAllAsync()
        {
            var entities = await this.Repository.ReadAllAsync().ConfigureAwait(false);
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<ApiSync<TApi>> ReadSyncedDataAsync(long lastSync, Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var apiSync = new ApiSync<TApi>();
            var syncResult = await this.Repository.ReadSyncedDataAsync(lastSync, additionalFilter).ConfigureAwait(false);

            apiSync.LastSync = syncResult.LastSync;
            apiSync.Data = this.mapper.GetApiFromEntity(syncResult.NewData);
            apiSync.DeletedData = syncResult.DeletedData.Select(x => x.Id).ToArray();

            return apiSync;
        }

        public virtual bool Exists(Guid id)
        {
            return this.Repository.Exists(id);
        }

        public virtual async Task WriteAsync(TApi apiEntity)
        {
            var entity = this.mapper.GetEntityFromApi(apiEntity);
            await this.Repository.WriteAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task WriteAsync(IEnumerable<TApi> apiEntities)
        {
            foreach (var apiEntity in apiEntities)
            {
                await this.WriteAsync(apiEntity).ConfigureAwait(false);
            }
        }

        public virtual async Task RemoveAsync(Guid id)
        {
            await this.Repository.RemoveAsync(id).ConfigureAwait(false);
        }

        public virtual async Task RemoveAsync(IEnumerable<Guid> ids)
        {
            await this.Repository.RemoveAsync(ids).ConfigureAwait(false);
        }

        public virtual async Task RemoveAsync(TApi apiEntity)
        {
            await this.Repository.RemoveAsync(apiEntity.Id).ConfigureAwait(false);
        }

        public virtual async Task RemoveAsync(IEnumerable<TApi> apiEntities)
        {
            var entities = this.mapper.GetEntityFromApi(apiEntities);
            await this.Repository.RemoveAsync(entities).ConfigureAwait(false);
        }
    }
}
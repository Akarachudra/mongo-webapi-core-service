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

        public virtual void Write(TApi apiEntity)
        {
            var entity = this.mapper.GetEntityFromApi(apiEntity);
            this.Repository.Write(entity);
        }

        public virtual void Write(TApi[] apiEntities)
        {
            foreach (var apiEntity in apiEntities)
            {
                this.Write(apiEntity);
            }
        }

        public virtual void Remove(Guid id)
        {
            this.Repository.RemoveAsync(id);
        }

        public virtual void Remove(Guid[] ids)
        {
            this.Repository.RemoveAsync(ids);
        }

        public virtual void Remove(TApi apiEntity)
        {
            this.Repository.RemoveAsync(apiEntity.Id);
        }

        public virtual void Remove(TApi[] apiEntities)
        {
            var entities = this.mapper.GetEntityFromApi(apiEntities);
            this.Repository.RemoveAsync(entities);
        }
    }
}
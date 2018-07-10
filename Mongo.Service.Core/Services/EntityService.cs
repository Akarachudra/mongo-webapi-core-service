using System;
using System.Linq;
using System.Linq.Expressions;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services
{
    public class EntityService<TApi, TEntity> : IEntityService<TApi, TEntity>
        where TApi : IApiBase
        where TEntity : IBaseEntity
    {
        private readonly IMapper<TApi, TEntity> mapper;

        public EntityService(IMongoRepository<TEntity> storage, IMapper<TApi, TEntity> mapper)
        {
            this.Storage = storage;
            this.mapper = mapper;
        }

        public IMongoRepository<TEntity> Storage { get; }

        public virtual TApi Read(Guid id)
        {
            var entity = this.Storage.Read(id);
            return this.mapper.GetApiFromEntity(entity);
        }

        public virtual bool TryRead(Guid id, out TApi apiEntity)
        {
            TEntity entity;
            var result = this.Storage.TryRead(id, out entity);
            if (result)
            {
                apiEntity = this.mapper.GetApiFromEntity(entity);
                return true;
            }
            apiEntity = default(TApi);
            return false;
        }

        public virtual TApi[] Read(int skip, int limit)
        {
            var entities = this.Storage.Read(skip, limit);
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual TApi[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = this.Storage.Read(filter, skip, limit);
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual TApi[] Read(Expression<Func<TEntity, bool>> filter)
        {
            var entities = this.Storage.Read(filter);
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual TApi[] ReadAll()
        {
            var entities = this.Storage.ReadAll();
            return this.mapper.GetApiFromEntity(entities);
        }

        public virtual Guid[] ReadIds(Expression<Func<TEntity, bool>> filter)
        {
            return this.Storage.ReadIds(filter);
        }

        public virtual long ReadSyncedData(long lastSync, out TApi[] newData, out Guid[] deletedData,
                                           Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            TEntity[] newEntities;
            TEntity[] deletedEntities;

            var newSync = this.Storage.ReadSyncedData(lastSync, out newEntities, out deletedEntities, additionalFilter);

            newData = this.mapper.GetApiFromEntity(newEntities);
            deletedData = deletedEntities.Select(x => x.Id).ToArray();

            return newSync;
        }

        public virtual bool Exists(Guid id)
        {
            return this.Storage.Exists(id);
        }

        public virtual void Write(TApi apiEntity)
        {
            var entity = this.mapper.GetEntityFromApi(apiEntity);
            this.Storage.Write(entity);
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
            this.Storage.Remove(id);
        }

        public virtual void Remove(Guid[] ids)
        {
            this.Storage.Remove(ids);
        }

        public virtual void Remove(TApi apiEntity)
        {
            this.Storage.Remove(apiEntity.Id);
        }

        public virtual void Remove(TApi[] apiEntities)
        {
            var entities = this.mapper.GetEntityFromApi(apiEntities);
            this.Storage.Remove(entities);
        }
    }
}
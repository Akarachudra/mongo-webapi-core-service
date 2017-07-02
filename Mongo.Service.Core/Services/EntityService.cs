using System;
using System.Linq;
using System.Linq.Expressions;
using Mongo.Service.Core.Services.Converters;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;
using Mongo.Service.Types.Base;

namespace Mongo.Service.Core.Services
{
    public class EntityService<TApi, TEntity> : IEntityService<TApi, TEntity> where TApi : IApiBase where TEntity : IBaseEntity
    {
        private readonly IConverter<TApi, TEntity> converter;

        public EntityService(IEntityStorage<TEntity> storage, IConverter<TApi, TEntity> converter)
        {
            Storage = storage;
            this.converter = converter;
        }
        
        public IEntityStorage<TEntity> Storage { get; }
        
        public virtual TApi Read(Guid id)
        {
            var entity = Storage.Read(id);
            return converter.GetApiFromStorable(entity);
        }

        public virtual bool TryRead(Guid id, out TApi apiEntity)
        {
            TEntity entity;
            var result = Storage.TryRead(id, out entity);
            if (result)
            {
                apiEntity = converter.GetApiFromStorable(entity);
                return true;
            }
            apiEntity = default(TApi);
            return false;
        }

        public virtual TApi[] Read(int skip, int limit)
        {
            var entities = Storage.Read(skip, limit);
            return converter.GetApiFromStorable(entities);
        }

        public virtual TApi[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = Storage.Read(filter, skip, limit);
            return converter.GetApiFromStorable(entities);
        }

        public virtual TApi[] Read(Expression<Func<TEntity, bool>> filter)
        {
            var entities = Storage.Read(filter);
            return converter.GetApiFromStorable(entities);
        }

        public virtual TApi[] ReadAll()
        {
            var entities = Storage.ReadAll();
            return converter.GetApiFromStorable(entities);
        }

        public virtual Guid[] ReadIds(Expression<Func<TEntity, bool>> filter)
        {
            return Storage.ReadIds(filter);
        }

        public virtual long ReadSyncedData(long lastSync, out TApi[] newData, out Guid[] deletedData,
                                           Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            TEntity[] newEntities;
            TEntity[] deletedEntities;

            var newSync = Storage.ReadSyncedData(lastSync, out newEntities, out deletedEntities, additionalFilter);

            newData = converter.GetApiFromStorable(newEntities);
            deletedData = deletedEntities.Select(x => x.Id).ToArray();

            return newSync;
        }

        public virtual bool Exists(Guid id)
        {
            return Storage.Exists(id);
        }

        public virtual void Write(TApi apiEntity)
        {
            var entity = converter.GetStorableFromApi(apiEntity);
            Storage.Write(entity);
        }

        public virtual void Write(TApi[] apiEntities)
        {
            foreach (var apiEntity in apiEntities)
            {
                Write(apiEntity);
            }
        }

        public virtual void Remove(Guid id)
        {
            Storage.Remove(id);
        }

        public virtual void Remove(Guid[] ids)
        {
            Storage.Remove(ids);
        }

        public virtual void Remove(TApi apiEntity)
        {
            Storage.Remove(apiEntity.Id);
        }

        public virtual void Remove(TApi[] apiEntities)
        {
            var entities = converter.GetStorableFromApi(apiEntities);
            Storage.Remove(entities);
        }
    }
}
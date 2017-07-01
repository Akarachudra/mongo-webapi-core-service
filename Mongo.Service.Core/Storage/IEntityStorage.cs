using System;
using System.Linq.Expressions;
using Mongo.Service.Core.Storable.Base;

namespace Mongo.Service.Core.Storage
{
    public interface IEntityStorage<TEntity> where TEntity : IBaseEntity
    {
        TEntity Read(Guid id);
        TEntity[] Read(Expression<Func<TEntity, bool>> filter);    
        TEntity[] Read(int skip, int limit);
        TEntity[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit);
        bool TryRead(Guid id, out TEntity apiEntity);
        TEntity[] ReadAll();
        Guid[] ReadIds(Expression<Func<TEntity, bool>> filter);
        long ReadSyncedData(long lastSync, out TEntity[] newData, out TEntity[] deletedData,
                            Expression<Func<TEntity, bool>> additionalFilter = null);
        bool Exists(Guid id);
        void Write(TEntity entity);
        void Write(TEntity[] entities);
        void Remove(Guid id);
        void Remove(Guid[] ids);
        void Remove(TEntity entity);
        void Remove(TEntity[] entities);
        long Count();
        long Count(Expression<Func<TEntity, bool>> filter);
        long GetLastTick();       
    }
}
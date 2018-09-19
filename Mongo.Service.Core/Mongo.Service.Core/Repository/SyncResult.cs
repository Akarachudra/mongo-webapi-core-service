using System.Collections.Generic;
using Mongo.Service.Core.Entities.Base;

namespace Mongo.Service.Core.Repository
{
    public class SyncResult<TEntity>
        where TEntity : IBaseEntity
    {
        public long LastSync { get; set; }

        public IList<TEntity> NewData { get; set; }

        public IList<TEntity> DeletedData { get; set; }
    }
}
using System.Collections.Generic;
using Mongo.Service.Core.Storable.Base;

namespace Mongo.Service.Core.Storage
{
    public class SyncResult<TEntity>
        where TEntity : IBaseEntity
    {
        public long LastSync { get; set; }

        public IList<TEntity> NewData { get; set; }

        public IList<TEntity> DeletedData { get; set; }
    }
}
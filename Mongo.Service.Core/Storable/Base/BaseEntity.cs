using System;

namespace Mongo.Service.Core.Storable.Base
{
    public abstract class BaseEntity : IBaseEntity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastModified { get; set; }
        public long Ticks { get; set; }
    }
}
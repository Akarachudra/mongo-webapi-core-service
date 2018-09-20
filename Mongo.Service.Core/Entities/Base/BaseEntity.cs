using System;

namespace Mongo.Service.Core.Entities.Base
{
    public class BaseEntity : IBaseEntity
    {
        public Guid Id { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime LastModified { get; set; }

        public long Ticks { get; set; }
    }
}
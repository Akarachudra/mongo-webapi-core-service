using System;

namespace Mongo.Service.Core.Entities.Base
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }

        bool IsDeleted { get; set; }

        DateTime LastModified { get; set; }

        long Ticks { get; set; }
    }
}
using System;

namespace Mongo.Service.Core.Types.Base
{
    public interface IApiBase
    {
        Guid Id { get; set; }
    }
}
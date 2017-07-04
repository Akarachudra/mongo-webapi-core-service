using System;

namespace Mongo.Service.Core.Types.Base
{
    public abstract class ApiBase : IApiBase
    {
        public Guid Id { get; set; }
    }
}
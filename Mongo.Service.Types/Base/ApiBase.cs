using System;

namespace Mongo.Service.Types.Base
{
    public abstract class ApiBase : IApiBase
    {
        public Guid Id { get; set; }
    }
}
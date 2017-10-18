using System;
using System.Runtime.Serialization;

namespace Mongo.Service.Core.Types.Base
{
    [DataContract]
    public abstract class ApiBase : IApiBase
    {
        [DataMember]
        public Guid Id { get; set; }
    }
}
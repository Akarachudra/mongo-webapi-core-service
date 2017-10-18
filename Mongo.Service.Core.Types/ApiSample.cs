using System.Runtime.Serialization;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Types
{
    [DataContract]
    public class ApiSample : ApiBase
    {
        [DataMember]
        public string SomeData { get; set; }
    }
}
using System.Runtime.Serialization;
using Mongo.Service.Types.Base;

namespace Mongo.Service.Types
{
    [DataContract]
    public class ApiSample : ApiBase
    {
        public string SomeData { get; set; }
    }
}
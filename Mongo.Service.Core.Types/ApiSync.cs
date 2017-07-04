using System;
using System.Runtime.Serialization;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Types
{
    [DataContract]
    public class ApiSync<TApi> where TApi : IApiBase
    {
        private TApi[] data;
        private Guid[] deleted;

        [DataMember]
        public TApi[] Data
        {
            get { return data ?? (data = new TApi[0]); }
            set { data = value; }
        }

        [DataMember]
        public Guid[] DeletedData
        {
            get { return deleted ?? (deleted = new Guid[0]); }
            set { deleted = value; }
        }

        [DataMember]
        public long LastSync { get; set; }
    }
}
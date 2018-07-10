using System;
using System.Runtime.Serialization;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Types
{
    [DataContract]
    public class ApiSync<TApi>
        where TApi : IApiBase
    {
        private TApi[] data;
        private Guid[] deleted;

        [DataMember]
        public TApi[] Data
        {
            get => this.data ?? (this.data = new TApi[0]);
            set => this.data = value;
        }

        [DataMember]
        public Guid[] DeletedData
        {
            get => this.deleted ?? (this.deleted = new Guid[0]);
            set => this.deleted = value;
        }

        [DataMember]
        public long LastSync { get; set; }
    }
}
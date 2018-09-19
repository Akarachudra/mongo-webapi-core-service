using System;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Types
{
    public class ApiSync<TApi>
        where TApi : IApiBase
    {
        private TApi[] data;
        private Guid[] deleted;

        public TApi[] Data
        {
            get => this.data ?? (this.data = new TApi[0]);
            set => this.data = value;
        }

        public Guid[] DeletedData
        {
            get => this.deleted ?? (this.deleted = new Guid[0]);
            set => this.deleted = value;
        }

        public long LastSync { get; set; }
    }
}
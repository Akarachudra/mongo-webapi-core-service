using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core.Controllers
{
    public class SampleController : ApiController
    {
        private readonly IEntityService<ApiSample, SampleEntity> service;

        public SampleController(IEntityService<ApiSample, SampleEntity> service)
        {
            this.service = service;
        }

        public async Task<IEnumerable<ApiSample>> GetAllAsync()
        {
            return await this.service.ReadAllAsync().ConfigureAwait(false);
        }

        public async Task<ApiSample> GetAsync(Guid id)
        {
            return await this.service.ReadAsync(id).ConfigureAwait(false);
        }

        public ApiSync<ApiSample> Get(long lastSync)
        {
            ApiSample[] newData;
            Guid[] deletedIds;

            var newSync = this.service.ReadSyncedData(lastSync, out newData, out deletedIds);

            var apiSync = new ApiSync<ApiSample>
            {
                Data = newData,
                DeletedData = deletedIds,
                LastSync = newSync
            };
            return apiSync;
        }

        public void Post(ApiSample apiSample)
        {
            this.service.Write(apiSample);
        }
    }
}
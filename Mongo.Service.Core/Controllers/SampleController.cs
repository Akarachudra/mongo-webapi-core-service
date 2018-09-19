using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mongo.Service.Core.Entities;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly IEntityService<ApiSample, SampleEntity> service;

        public SampleController(IEntityService<ApiSample, SampleEntity> service)
        {
            this.service = service;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<ApiSample>> GetAllAsync()
        {
            return await this.service.ReadAllAsync().ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        public async Task<ApiSample> GetAsync(Guid id)
        {
            return await this.service.ReadAsync(id).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<ApiSync<ApiSample>> GetAsync(long lastSync)
        {
            var apiSync = await this.service.ReadSyncedDataAsync(lastSync).ConfigureAwait(false);
            return apiSync;
        }

        [HttpPost]
        public async Task PostAsync(ApiSample apiSample)
        {
            await this.service.WriteAsync(apiSample).ConfigureAwait(false);
        }
    }
}

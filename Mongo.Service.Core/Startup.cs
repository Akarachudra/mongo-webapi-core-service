using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mongo.Service.Core.Entities;
using Mongo.Service.Core.Repository;
using Mongo.Service.Core.Repository.Indexes;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IMongoStorage, MongoStorage>();
            services.AddSingleton<IMongoSettings, MongoSettings>();
            services.AddSingleton<IMongoRepository<SampleEntity>, MongoRepository<SampleEntity>>();
            services.AddSingleton<IIndexes<SampleEntity>, Indexes<SampleEntity>>();
            services.AddSingleton<IEntityService<ApiSample, SampleEntity>, EntityService<ApiSample, SampleEntity>>();
            services.AddSingleton<IMapper<ApiSample, SampleEntity>, Mapper<ApiSample, SampleEntity>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}

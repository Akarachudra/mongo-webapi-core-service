# Mongo.Service.Core
Mongo.Service.Core designed for the rapid implementation your own scalable RESTFull service based on ASP.NET WebApi and Owin Self Host.
It uses MongoDB as a data store. Timeline (reading new, deleted and changed data from client) implementation based on optimistic loop.

Some other features:
* TopShelf for run as Windows Service.
* Automapper for entities mapping.
* DI Container - Simple Injector.

## How to use

Add a new ApiType (your share this with client) to Mongo.Service.Core.Types project and inherit it from ApiBase:
```c#
namespace Mongo.Service.Core.Types
{
    // Inherited from ApiBase
    [DataContract]
    public class ApiSample : ApiBase
    {
        [DataMember]
        public string SomeData { get; set; }
    }
}
```

Add a new EntityType (it's stored in MongoDB) to Mongo.Service.Core:
```c#
namespace Mongo.Service.Core.Storable
{
    // Set collection name. Inherit class from BaseEntity
    [CollectionName("Sample")]
    public class SampleEntity : BaseEntity
    {
        public string SomeData { get; set; }
    }
}
```

Implement custom Automapper mapping configuration if default is not enough:
```c#
namespace Mongo.Service.Core.Services.Mapping
{
    public class Mapper<TApi, TEntity> : IMapper<TApi, TEntity> where TEntity : IBaseEntity where TApi : IApiBase
    {
        // Some another basic implementation
		
	// Override this
        protected virtual IMapper ConfigureApiToEntityMapper()
        {
            var toEntityMapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TApi, TEntity>());
            return toEntityMapperConfig.CreateMapper();
        }

	// And this
        protected virtual IMapper ConfigureEntityToApiMapper()
        {
            var toApiMapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TEntity, TApi>());
            return toApiMapperConfig.CreateMapper();
        }
		
	// Some another basic implementation
    }
}
```

Create new controller class responsible for your new ApiType:
```c#
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

        public async Task<ApiSync<ApiSample>> GetAsync(long lastSync)
        {
            var apiSync = await this.service.ReadSyncedDataAsync(lastSync).ConfigureAwait(false);
            return apiSync;
        }

        public async Task PostAsync(ApiSample apiSample)
        {
            await this.service.WriteAsync(apiSample).ConfigureAwait(false);
        }
    }
}
```

Configure DI container at Startup class:
```c#
private static void ConfigureContainer(Container container)
{
    container.RegisterSingleton<IMongoStorage, MongoStorage>();
    container.RegisterSingleton<IMongoSettings, MongoSettings>();
    container.RegisterSingleton<IEntityStorage<SampleEntity>, EntityStorage<SampleEntity>>();
    container.RegisterSingleton<IIndexes<SampleEntity>, Indexes<SampleEntity>>();
    container.RegisterSingleton<IEntityService<ApiSample, SampleEntity>, EntityService<ApiSample, SampleEntity>>();
    container.RegisterSingleton<IMapper<ApiSample, SampleEntity>, Mapper<ApiSample, SampleEntity>>();
}
```

You may need to other changes:
* Implement your custom indexes. Just create new class inherited from Indexes and override CreateCustomIndexes method.
* Implement your custom EntityService and override read/write methods. May be helpfull if you need write or read custom entity data.

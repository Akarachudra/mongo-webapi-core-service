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
using System.Runtime.Serialization;
using Mongo.Service.Core.Types.Base;

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
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;

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
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Types;

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
using System;
using System.Collections.Generic;
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

        public IEnumerable<ApiSample> GetAll()
        {
            return service.ReadAll();
        }
        
        public ApiSample Get(Guid id)
        {
            return service.Read(id);
        }

        public ApiSync<ApiSample> Get(long lastSync)
        {
            ApiSample[] newData;
            Guid[] deletedIds;
            
            // Synchronize client data with ticks. Client will get only new data
            var newSync = service.ReadSyncedData(lastSync, out newData, out deletedIds);

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
            service.Write(apiSample);
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
* Implement your custom indexes. Just create new class inherited from Indexes and override CreateIndexes method. But please, don't forget to call base method in overrided, otherwise - data synchronization will work incorrect.
* Implement your custom EntityService and override read/write methods. May be helpfull if you need write or read custom entity data.

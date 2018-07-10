using System;
using System.Linq;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Tests.Helpers;
using Mongo.Service.Core.Types;
using NUnit.Framework;

namespace Mongo.Service.Core.Tests
{
    [TestFixture]
    public class EntityServiceTests
    {
        private readonly IMongoStorage mongoStorage;
        private readonly IEntityService<ApiSample, SampleEntity> service;

        public EntityServiceTests()
        {
            this.mongoStorage = new MongoStorage(new MongoSettings());
            var storage = new MongoRepository<SampleEntity>(this.mongoStorage, new Indexes<SampleEntity>());
            var mapper = new Mapper<ApiSample, SampleEntity>();
            this.service = new EntityService<ApiSample, SampleEntity>(storage, mapper);
        }

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this.mongoStorage.ClearCollection<SampleEntity>();
        }

        [Test]
        public void CanWriteAndRead()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "test"
            };

            this.service.WriteAsync(apiEntity).Wait();
            var readedApiEntity = this.service.ReadAsync(apiEntity.Id).Result;

            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, readedApiEntity));
        }

        [Test]
        public void CanWriteArrayAndReadWithFilter()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData1"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData2"
                }
            };

            this.service.WriteAsync(apiEntities).Wait();

            var readedApiEntities = this.service.ReadAsync(x => x.SomeData == "testData1").Result;
            Assert.AreEqual(1, readedApiEntities.Count);
            Assert.AreEqual("testData1", readedApiEntities[0].SomeData);
        }

        [Test]
        public void ExistsIsCorrect()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "test"
            };

            this.service.WriteAsync(apiEntity).Wait();
            Assert.IsTrue(this.service.Exists(apiEntity.Id));
            Assert.IsFalse(this.service.Exists(Guid.NewGuid()));
        }

        [Test]
        public void CanReadAll()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData1"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData2"
                }
            };

            var anonymousBefore = apiEntities.Select(x => new { x.Id, x.SomeData });
            this.service.WriteAsync(apiEntities).Wait();
            var readedAllApiEntities = this.service.ReadAllAsync().Result;
            var anonymousAfter = readedAllApiEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousBefore, anonymousAfter);
        }

        [Test]
        public void CanReadWithSkipAndLimit()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "2"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                }
            };
            this.service.WriteAsync(apiEntities);

            var anonymousEntitiesBefore = apiEntities.Select(x => new { x.Id, x.SomeData }).Take(2);
            var readedEntities = this.service.ReadAsync(0, 2).Result;
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Where(x => x.SomeData == "3")
                .Select(x => new { x.Id, x.SomeData })
                .Skip(1)
                .Take(1);
            readedEntities = this.service.ReadAsync(x => x.SomeData == "3", 1, 1).Result;
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Select(x => new { x.Id, x.SomeData })
                .Skip(1)
                .Take(2);
            readedEntities = this.service.ReadAsync(1, 2).Result;
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public void CanRemoveEntities()
        {
            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };
            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };
            var apiEntity3 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            this.service.WriteAsync(apiEntity1).Wait();
            this.service.RemoveAsync(apiEntity1).Wait();
            var readedEntities = this.service.ReadAsync(x => !x.IsDeleted).Result;
            Assert.AreEqual(0, readedEntities.Count);

            this.service.WriteAsync(new[] { apiEntity2, apiEntity3 }).Wait();
            this.service.RemoveAsync(new[] { apiEntity2, apiEntity3 }).Wait();
            readedEntities = this.service.ReadAsync(x => !x.IsDeleted).Result;
            Assert.AreEqual(0, readedEntities.Count);
        }

        [Test]
        public void CanReadSyncedData()
        {
            var apiSync = this.service.ReadSyncedDataAsync(0).Result;

            Assert.AreEqual(0, apiSync.LastSync);

            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            this.service.WriteAsync(apiEntity1);

            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            this.service.WriteAsync(apiEntity2);

            apiSync = this.service.ReadSyncedDataAsync(apiSync.LastSync).Result;

            Assert.AreEqual(2, apiSync.Data.Length);
            Assert.AreEqual(2, apiSync.LastSync);

            var previousSync = apiSync.LastSync;
            apiSync = this.service.ReadSyncedDataAsync(apiSync.LastSync).Result;

            Assert.AreEqual(previousSync, apiSync.LastSync);

            this.service.RemoveAsync(apiEntity2);
            apiSync = this.service.ReadSyncedDataAsync(apiSync.LastSync).Result;
            Assert.AreEqual(1, apiSync.DeletedData.Length);
            Assert.AreEqual(3, apiSync.LastSync);
        }

        [Test]
        public void CanReadSyncedDataWithFilter()
        {
            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            this.service.WriteAsync(apiEntity1);

            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };
            this.service.WriteAsync(apiEntity2);

            var apiSync = this.service.ReadSyncedDataAsync(0, x => x.SomeData == "2").Result;

            Assert.AreEqual(1, apiSync.Data.Length);
            Assert.AreEqual(2, apiSync.LastSync);
        }
    }
}
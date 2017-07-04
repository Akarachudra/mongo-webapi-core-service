using System;
using System.Linq;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Services.Converters;
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
            mongoStorage = new MongoStorage(new MongoSettings());
            var storage = new EntityStorage<SampleEntity>(mongoStorage, new Indexes<SampleEntity>());
            var converter = new SampleConverter();
            service = new EntityService<ApiSample, SampleEntity>(storage, converter);
        }

        [SetUp]
        public void RunBeforeAnyTest()
        {
            mongoStorage.DropCollection<SampleEntity>();
        }

        [Test]
        public void CanWriteAndRead()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "test"
            };

            service.Write(apiEntity);
            var readedApiEntity = service.Read(apiEntity.Id);

            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, readedApiEntity));
        }

        [Test]
        public void TryReadIsCorrect()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "test"
            };

            service.Write(apiEntity);
            ApiSample resultApiEntity;
            var readResult = service.TryRead(apiEntity.Id, out resultApiEntity);
            Assert.IsTrue(readResult);
            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, resultApiEntity));

            readResult = service.TryRead(Guid.NewGuid(), out resultApiEntity);
            Assert.IsFalse(readResult);
            Assert.AreEqual(default(ApiSample), resultApiEntity);
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

            service.Write(apiEntities);

            var readedApiEntities = service.Read(x => x.SomeData == "testData1");
            Assert.AreEqual(1, readedApiEntities.Length);
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

            service.Write(apiEntity);
            Assert.IsTrue(service.Exists(apiEntity.Id));
            Assert.IsFalse(service.Exists(Guid.NewGuid()));
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
            service.Write(apiEntities);
            var readedAllApiEntities = service.ReadAll();
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
            service.Write(apiEntities);

            var anonymousEntitiesBefore = apiEntities.Select(x => new { x.Id, x.SomeData }).Take(2);
            var readedEntities = service.Read(0, 2);
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Where(x => x.SomeData == "3")
                                                 .Select(x => new { x.Id, x.SomeData })
                                                 .Skip(1)
                                                 .Take(1);
            readedEntities = service.Read(x => x.SomeData == "3", 1, 1);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Select(x => new { x.Id, x.SomeData })
                                                 .Skip(1)
                                                 .Take(2);
            readedEntities = service.Read(1, 2);
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

            service.Write(apiEntity1);
            service.Remove(apiEntity1);
            var readedEntities = service.Read(x => !x.IsDeleted);
            Assert.AreEqual(0, readedEntities.Length);

            service.Write(new[] { apiEntity2, apiEntity3 });
            service.Remove(new[] { apiEntity2, apiEntity3 });
            readedEntities = service.Read(x => !x.IsDeleted);
            Assert.AreEqual(0, readedEntities.Length);
        }

        [Test]
        public void CanReadIdsOnly()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid()
                },
                new ApiSample
                {
                    Id = Guid.NewGuid()
                },
                new ApiSample
                {
                    Id = Guid.NewGuid()
                },
                new ApiSample
                {
                    Id = Guid.NewGuid()
                }
            };

            service.Write(apiEntities);
            var idsBefore = apiEntities.Select(x => x.Id);
            var idsAfter = service.ReadIds(x => !x.IsDeleted);
            CollectionAssert.AreEquivalent(idsBefore, idsAfter);
        }

        [Test]
        public void CanReadSyncedData()
        {
            ApiSample[] apiEntities;
            Guid[] deletedIds;
            var sync = service.ReadSyncedData(0, out apiEntities, out deletedIds);

            Assert.AreEqual(0, sync);

            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            service.Write(apiEntity1);

            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            service.Write(apiEntity2);

            sync = service.ReadSyncedData(sync, out apiEntities, out deletedIds);

            Assert.AreEqual(2, apiEntities.Length);
            Assert.AreEqual(2, sync);

            var previousSync = sync;
            sync = service.ReadSyncedData(sync, out apiEntities, out deletedIds);

            Assert.AreEqual(previousSync, sync);

            service.Remove(apiEntity2);
            sync = service.ReadSyncedData(sync, out apiEntities, out deletedIds);
            Assert.AreEqual(1, deletedIds.Length);
            Assert.AreEqual(3, sync);
        }

        [Test]
        public void CanReadSyncedDataWithFilter()
        {
            ApiSample[] apiEntities;
            Guid[] deletedIds;
            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            service.Write(apiEntity1);

            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };
            service.Write(apiEntity2);

            var sync = service.ReadSyncedData(0, out apiEntities, out deletedIds, x => x.SomeData == "2");

            Assert.AreEqual(1, apiEntities.Length);
            Assert.AreEqual(2, sync);
        }
    }
}
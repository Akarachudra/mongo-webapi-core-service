using System;
using System.Linq;
using Mongo.Service.Core.Controllers;
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
    public class SampleControllerTests
    {
        private readonly IMongoStorage mongoStorage;
        private readonly IEntityService<ApiSample, SampleEntity> service;

        public SampleControllerTests()
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
        public void CanGetAll()
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
                }
            };

            var idsBefore = apiEntities.Select(x => x.Id);
            var sampleController = new SampleController(this.service);
            var resultEntities = sampleController.GetAllAsync().Result.ToArray();
            Assert.AreEqual(0, resultEntities.Length);

            this.service.WriteAsync(apiEntities).Wait();
            var resultIds = sampleController.GetAllAsync().Result.Select(x => x.Id).ToArray();
            CollectionAssert.AreEquivalent(idsBefore, resultIds);
        }

        [Test]
        public void CanGetById()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid()
            };

            this.service.WriteAsync(apiEntity).Wait();
            var sampleController = new SampleController(this.service);
            var resultApiEntity = sampleController.GetAsync(apiEntity.Id).Result;
            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, resultApiEntity));
        }

        [Test]
        public void CanPostApiEntity()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid()
            };

            var sampleController = new SampleController(this.service);
            sampleController.Post(apiEntity);

            var readedEtity = this.service.ReadAsync(apiEntity.Id).Result;
            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, readedEtity));
        }

        [Test]
        public void CanGetWithSync()
        {
            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid()
            };

            var sampleController = new SampleController(this.service);
            var apiSync = sampleController.GetAsync(-1).Result;
            Assert.AreEqual(0, apiSync.LastSync);
            Assert.AreEqual(0, apiSync.Data.Length);
            Assert.AreEqual(0, apiSync.DeletedData.Length);

            this.service.WriteAsync(apiEntity1).Wait();
            apiSync = sampleController.GetAsync(apiSync.LastSync).Result;
            Assert.AreEqual(1, apiSync.LastSync);
            Assert.AreEqual(1, apiSync.Data.Length);
            Assert.AreEqual(apiEntity1.Id, apiSync.Data[0].Id);
            Assert.AreEqual(0, apiSync.DeletedData.Length);

            this.service.WriteAsync(apiEntity2).Wait();
            this.service.RemoveAsync(apiEntity1).Wait();
            apiSync = sampleController.GetAsync(apiSync.LastSync).Result;
            Assert.AreEqual(3, apiSync.LastSync);
            Assert.AreEqual(1, apiSync.Data.Length);
            Assert.AreEqual(apiEntity2.Id, apiSync.Data[0].Id);
            Assert.AreEqual(1, apiSync.DeletedData.Length);
            Assert.AreEqual(apiEntity1.Id, apiSync.DeletedData[0]);
        }
    }
}
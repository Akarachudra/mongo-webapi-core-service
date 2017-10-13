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
            mongoStorage = new MongoStorage(new MongoSettings());
            var storage = new EntityStorage<SampleEntity>(mongoStorage, new Indexes<SampleEntity>());
            var mapper = new Mapper<ApiSample, SampleEntity>();
            service = new EntityService<ApiSample, SampleEntity>(storage, mapper);
        }

        [SetUp]
        public void RunBeforeAnyTest()
        {
            mongoStorage.ClearCollection<SampleEntity>();
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
            var sampleController = new SampleController(service);
            var resultEntities = sampleController.GetAll().ToArray();
            Assert.AreEqual(0, resultEntities.Length);
            
            service.Write(apiEntities);
            var resultIds = sampleController.GetAll().Select(x => x.Id).ToArray();
            CollectionAssert.AreEquivalent(idsBefore, resultIds);
        }

        [Test]
        public void CanGetById()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            
            service.Write(apiEntity);
            var sampleController = new SampleController(service);
            var resultApiEntity = sampleController.Get(apiEntity.Id);
            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, resultApiEntity));
        }

        [Test]
        public void CanPostApiEntity()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            
            var sampleController = new SampleController(service);
            sampleController.Post(apiEntity);

            var readedEtity = service.Read(apiEntity.Id);
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
            
            var sampleController = new SampleController(service);
            var apiSync = sampleController.Get(-1);
            Assert.AreEqual(0, apiSync.LastSync);
            Assert.AreEqual(0, apiSync.Data.Length);
            Assert.AreEqual(0, apiSync.DeletedData.Length);
            
            service.Write(apiEntity1);
            apiSync = sampleController.Get(apiSync.LastSync);
            Assert.AreEqual(1, apiSync.LastSync);
            Assert.AreEqual(1, apiSync.Data.Length);
            Assert.AreEqual(apiEntity1.Id, apiSync.Data[0].Id);
            Assert.AreEqual(0, apiSync.DeletedData.Length);
            
            service.Write(apiEntity2);
            service.Remove(apiEntity1);
            apiSync = sampleController.Get(apiSync.LastSync);
            Assert.AreEqual(3, apiSync.LastSync);
            Assert.AreEqual(1, apiSync.Data.Length);
            Assert.AreEqual(apiEntity2.Id, apiSync.Data[0].Id);
            Assert.AreEqual(1, apiSync.DeletedData.Length);
            Assert.AreEqual(apiEntity1.Id, apiSync.DeletedData[0]);
        }
    }
}
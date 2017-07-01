using System;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Storable.System;
using Mongo.Service.Core.Storage;
using NUnit.Framework;

namespace Mongo.Service.Core.Tests
{
    [TestFixture]
    public class EntityStorageTests
    {
        private readonly IMongoStorage mongoStorage;
        private readonly IEntityStorage<SampleEntity> storage;

        public EntityStorageTests()
        {
            mongoStorage = new MongoStorage(new MongoSettings());
            storage = new EntityStorage<SampleEntity>(mongoStorage, new Indexes<SampleEntity>());
        }

        [SetUp]
        public void RunBeforeAnyTest()
        {
            mongoStorage.DropCollection<CounterEntity>();
            mongoStorage.DropCollection<SampleEntity>();
        }

        [Test]
        public void CanWriteAndReadEntityById()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            storage.Write(entity);

            var readedEntity = storage.Read(entity.Id);

            Assert.AreEqual(entity.Id, readedEntity.Id);
            Assert.AreEqual(entity.SomeData, readedEntity.SomeData);
        }

        [Test]
        public void CanAutoFillLastModifiedDateTime()
        {
            var dateTimeBefore = DateTime.UtcNow;
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            storage.Write(entity);

            var dateTimeAfter = DateTime.UtcNow;

            var readedEntity = storage.Read(entity.Id);

            Assert.IsTrue((dateTimeBefore <= readedEntity.LastModified) && (readedEntity.LastModified <= dateTimeAfter));
        }

        [Test]
        public void CanWriteArrayAndReadWithFilter()
        {
            var entities = new[]
            {
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData2"
                }
            };

            storage.Write(entities);

            var readedEntities = storage.Read(x => x.IsDeleted == false);
            Assert.AreEqual(2, readedEntities.Length);

            readedEntities = storage.Read(x => x.SomeData == "testData1");
            Assert.AreEqual(1, readedEntities.Length);
            Assert.AreEqual("testData1", readedEntities[0].SomeData);
        }

        [Test]
        public void CanAutoFillIdIfItsEmpty()
        {
            var entity = new SampleEntity
            {
                SomeData = "testData"
            };

            storage.Write(entity);

            var readedEntities = storage.Read(x => x.SomeData == "testData");
            Assert.IsTrue(readedEntities[0].Id != default(Guid));
        }

        [Test]
        public void TryReadIsCorrect()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };
            
            storage.Write(entity);

            SampleEntity resultEntity;
            var readResult = storage.TryRead(entity.Id, out resultEntity);
            Assert.IsTrue(readResult);
            Assert.AreEqual("testData", resultEntity.SomeData);
            
            readResult = storage.TryRead(Guid.NewGuid(), out resultEntity);
            Assert.IsFalse(readResult);
            Assert.AreEqual(default(SampleEntity), resultEntity);
        }
    }
}
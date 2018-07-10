using System;
using System.Linq;
using System.Linq.Expressions;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Storage;
using NUnit.Framework;

namespace Mongo.Service.Core.Tests
{
    [TestFixture]
    public class MongoRepositoryTests
    {
        private readonly IMongoStorage mongoStorage;
        private IMongoRepository<SampleEntity> repository;

        public MongoRepositoryTests()
        {
            this.mongoStorage = new MongoStorage(new MongoSettings());
        }

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this.mongoStorage.ClearCollection<SampleEntity>();
            this.repository = new MongoRepository<SampleEntity>(this.mongoStorage, new Indexes<SampleEntity>());
        }

        [Test]
        public void CanWriteAndReadEntityById()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            this.repository.Write(entity);

            var readedEntity = this.repository.ReadAsync(entity.Id).Result;

            Assert.AreEqual(entity.Id, readedEntity.Id);
            Assert.AreEqual(entity.SomeData, readedEntity.SomeData);
        }

        [Test]
        public void CanAutoFillLastModifiedDateTime()
        {
            var dateTimeBefore = DateTime.UtcNow.AddSeconds(-1);
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            this.repository.Write(entity);

            var dateTimeAfter = DateTime.UtcNow.AddSeconds(1);

            var readedEntity = this.repository.ReadAsync(entity.Id).Result;

            Assert.IsTrue(dateTimeBefore <= readedEntity.LastModified && readedEntity.LastModified <= dateTimeAfter);
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

            this.repository.Write(entities);

            var readedEntities = this.repository.ReadAsync(x => x.IsDeleted == false).Result;
            Assert.AreEqual(2, readedEntities.Count);

            readedEntities = this.repository.ReadAsync(x => x.SomeData == "testData1").Result;
            Assert.AreEqual(1, readedEntities.Count);
            Assert.AreEqual("testData1", readedEntities[0].SomeData);
        }

        [Test]
        public void CanAutoFillIdIfItsEmpty()
        {
            var entity = new SampleEntity
            {
                SomeData = "testData"
            };

            this.repository.Write(entity);

            var readedEntities = this.repository.ReadAsync(x => x.SomeData == "testData").Result;
            Assert.IsTrue(readedEntities[0].Id != default(Guid));
        }

        [Test]
        public void CanReadWithSkipAndLimit()
        {
            var entities = new[]
            {
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "2"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                }
            };
            this.repository.Write(entities);

            var anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData }).Take(2);
            var readedEntities = this.repository.ReadAsync(0, 2).Result;
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Where(x => x.SomeData == "3")
                .Select(x => new { x.Id, x.SomeData })
                .Skip(1)
                .Take(1);
            readedEntities = this.repository.ReadAsync(x => x.SomeData == "3", 1, 1).Result;
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData })
                .Skip(1)
                .Take(2);
            readedEntities = this.repository.ReadAsync(1, 2).Result;
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public void CanRemoveEntities()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };
            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            this.repository.Write(entity1);
            this.repository.Remove(entity1);

            var readedEntity = this.repository.ReadAsync(entity1.Id).Result;
            Assert.IsTrue(readedEntity.IsDeleted);

            var entity3 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            this.repository.Write(new[] { entity2, entity3 });
            this.repository.Remove(new[] { entity2, entity3 });
            readedEntity = this.repository.ReadAsync(entity2.Id).Result;
            Assert.IsTrue(readedEntity.IsDeleted);

            readedEntity = this.repository.ReadAsync(entity3.Id).Result;
            Assert.IsTrue(readedEntity.IsDeleted);
        }

        [Test]
        public void CheckWriteIsNotRestoreDeletedEntity()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            this.repository.Write(entity);
            this.repository.Remove(entity);

            this.repository.Write(entity);
            var readedEntity = this.repository.ReadAsync(entity.Id).Result;
            Assert.IsTrue(readedEntity.IsDeleted);
        }

        [Test]
        public void ExistsIsCorrect()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid()
            };

            this.repository.Write(entity);
            Assert.IsTrue(this.repository.Exists(entity.Id));
            Assert.IsFalse(this.repository.Exists(Guid.NewGuid()));
        }

        [Test]
        public void CanReadAll()
        {
            var entities = new[]
            {
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "2"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                }
            };

            this.repository.Write(entities);
            var anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData });
            var readedEntities = this.repository.ReadAllAsync().Result;
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public void CountIsCorrect()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };

            this.repository.Write(entity1);
            Assert.AreEqual(1, this.repository.Count());

            this.repository.Write(entity2);
            Assert.AreEqual(2, this.repository.Count());

            Assert.AreEqual(1, this.repository.Count(x => x.SomeData == "2"));
        }

        [Test]
        public void CanAutoincrementLastTick()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };

            Assert.AreEqual(0, this.repository.GetLastTick());

            this.repository.Write(entity1);
            var readedEntity1 = this.repository.ReadAsync(entity1.Id).Result;
            Assert.AreEqual(1, this.repository.GetLastTick());
            Assert.AreEqual(1, readedEntity1.Ticks);

            this.repository.Write(entity2);
            var readedEntity2 = this.repository.ReadAsync(entity2.Id).Result;
            Assert.AreEqual(2, this.repository.GetLastTick());
            Assert.AreEqual(2, readedEntity2.Ticks);
        }

        [Test]
        public void CanReadSyncedData()
        {
            SampleEntity[] entities;
            SampleEntity[] deletedEntities;
            var sync = this.repository.ReadSyncedData(0, out entities, out deletedEntities);

            Assert.AreEqual(0, sync);

            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid()
            };
            this.repository.Write(entity1);

            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid()
            };
            this.repository.Write(entity2);

            sync = this.repository.ReadSyncedData(sync, out entities, out deletedEntities);

            Assert.AreEqual(2, entities.Length);
            Assert.AreEqual(2, sync);

            var previousSync = sync;
            sync = this.repository.ReadSyncedData(sync, out entities, out deletedEntities);

            Assert.AreEqual(previousSync, sync);

            this.repository.Remove(entity2);
            sync = this.repository.ReadSyncedData(sync, out entities, out deletedEntities);
            Assert.AreEqual(1, deletedEntities.Length);
            Assert.AreEqual(3, sync);
        }

        [Test]
        public void CanReadSyncedDataWithFilter()
        {
            SampleEntity[] entities;
            SampleEntity[] deletedEntities;
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            this.repository.Write(entity1);

            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };
            this.repository.Write(entity2);

            var sync = this.repository.ReadSyncedData(0, out entities, out deletedEntities, x => x.SomeData == "2");

            Assert.AreEqual(1, entities.Length);
            Assert.AreEqual(2, sync);
        }

        [Test]
        public void CanUpdateTicksOnly()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid()
            };
            this.repository.Write(entity);
            var readedEntity = this.repository.ReadAsync(entity.Id).Result;
            var ticksBefore = readedEntity.Ticks;
            this.repository.UpdateTicks(entity.Id);
            readedEntity = this.repository.ReadAsync(entity.Id).Result;
            Assert.AreEqual(ticksBefore + 1, readedEntity.Ticks);
        }

        [Test]
        public void CanUpdateEntityFieldsWithoutTicks()
        {
            const string dataAfter = "data after";
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "data before"
            };
            this.repository.Write(entity);
            var readedEntity = this.repository.ReadAsync(entity.Id).Result;
            var ticksBefore = readedEntity.Ticks;
            var updater = this.repository.Updater;
            var updateDefinition = updater.Set(x => x.SomeData, dataAfter);
            this.repository.Update(x => x.Id == entity.Id, updateDefinition);
            readedEntity = this.repository.ReadAsync(entity.Id).Result;
            Assert.AreEqual(ticksBefore, readedEntity.Ticks);
            Assert.AreEqual(dataAfter, readedEntity.SomeData);
        }

        [Test]
        public void CanUpdateEntityFieldsWitTicks()
        {
            const string dataAfter = "data after";
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "data before"
            };
            this.repository.Write(entity);
            var readedEntity = this.repository.ReadAsync(entity.Id).Result;
            var ticksBefore = readedEntity.Ticks;
            var updater = this.repository.Updater;
            var updateDefinition = updater.Set(x => x.SomeData, dataAfter);
            this.repository.UpdateWithTicks(x => x.Id == entity.Id, updateDefinition);
            readedEntity = this.repository.ReadAsync(entity.Id).Result;
            Assert.AreEqual(ticksBefore + 1, readedEntity.Ticks);
            Assert.AreEqual(dataAfter, readedEntity.SomeData);
        }
    }
}
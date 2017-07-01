﻿using System;
using System.Linq;
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
            storage.Write(entities);

            var anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData }).Take(2);
            var readedEntities = storage.Read(0, 2);
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Where(x => x.SomeData == "3")
                                              .Select(x => new { x.Id, x.SomeData })
                                              .Skip(1)
                                              .Take(1);
            readedEntities = storage.Read(x => x.SomeData == "3", 1, 1);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData })
                                              .Skip(1)
                                              .Take(2);
            readedEntities = storage.Read(1, 2);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public void CanRemoveEntities()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                IsDeleted = false,
                SomeData = "testData"
            };
            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                IsDeleted = false,
                SomeData = "testData"
            };

            storage.Write(entity1);
            storage.Remove(entity1);

            var readedEntity = storage.Read(entity1.Id);
            Assert.IsTrue(readedEntity.IsDeleted);

            storage.Write(entity1);
            storage.Remove(entity1.Id);
            readedEntity = storage.Read(entity1.Id);
            Assert.IsTrue(readedEntity.IsDeleted);

            storage.Write(entity1);
            storage.Write(entity2);
            storage.Remove(new[] { entity1.Id, entity2.Id });
            var readedEntities = storage.Read(x => x.SomeData == "testData");
            Assert.IsTrue(readedEntities[0].IsDeleted);
            Assert.IsTrue(readedEntities[1].IsDeleted);

            storage.Write(entity1);
            storage.Write(entity2);
            storage.Remove(new[] { entity1, entity2 });
            readedEntities = storage.Read(x => x.SomeData == "testData");
            Assert.IsTrue(readedEntities[0].IsDeleted);
            Assert.IsTrue(readedEntities[1].IsDeleted);
        }

        [Test]
        public void ExistsIsCorrect()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid()
            };

            storage.Write(entity);
            Assert.IsTrue(storage.Exists(entity.Id));
            Assert.IsFalse(storage.Exists(Guid.NewGuid()));
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

            storage.Write(entities);
            var anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData });
            var readedEntities = storage.ReadAll();
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public void CanReadIdsOnly()
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
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                }
            };
            
            storage.Write(entities);
            var idsBefore = entities.Select(x => x.Id);
            var idsAfer = storage.ReadIds(x => x.SomeData == "1");
            CollectionAssert.AreEquivalent(idsBefore, idsAfer);
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
            
            storage.Write(entity1);
            Assert.AreEqual(1, storage.Count());
            
            storage.Write(entity2);
            Assert.AreEqual(2, storage.Count());

            Assert.AreEqual(1, storage.Count(x => x.SomeData == "2"));
        }
    }
}
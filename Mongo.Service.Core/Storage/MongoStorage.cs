using System;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storable.System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public class MongoStorage : IMongoStorage
    {
        private readonly IMongoDatabase database;

        public MongoStorage(IMongoSettings settings)
        {
            var mongoDataBaseName = settings.MongoDatabaseName;

            var mongoClientSettings = new MongoClientSettings
            {
                Servers = settings.MongoServers,
                WriteConcern = WriteConcern.W1,
                ReadPreference = ReadPreference.Primary,
                GuidRepresentation = GuidRepresentation.Standard
            };

            var mongoReplicaSetName = settings.MongoReplicaSetName;
            if (!string.IsNullOrEmpty(mongoReplicaSetName))
            {
                mongoClientSettings.ReplicaSetName = mongoReplicaSetName;
            }

            var mongoUserName = settings.MongoUserName;
            if (!string.IsNullOrEmpty(mongoUserName))
            {
                mongoClientSettings.Credentials = new[]
                {
                    MongoCredential.CreateCredential(mongoDataBaseName,
                        mongoUserName,
                        settings.MongoPassword)
                };
            }

            var client = new MongoClient(mongoClientSettings);

            database = client.GetDatabase(mongoDataBaseName);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>(out string collectionName) where TEntity : IBaseEntity
        {
            collectionName = GetCollectionName(typeof(TEntity));
            return database.GetCollection<TEntity>(collectionName);
        }

        public void DropCollection<T>()
        {
            database.DropCollection(GetCollectionName(typeof(T)));
        }

        public IMongoCollection<CounterEntity> GetSyncCollection()
        {
            return database.GetCollection<CounterEntity>(GetCollectionName(typeof(CounterEntity)));
        }

        private static string GetCollectionName(Type type)
        {
            foreach (var attr in type.GetCustomAttributes(false))
            {
                var attribute = attr as CollectionNameAttribute;
                if (attribute != null)
                {
                    var collectionName = attribute.Name;
                    if (string.IsNullOrEmpty(collectionName))
                    {
                        throw new ArgumentException(
                            $"There is empty collection name at {typeof(CollectionNameAttribute).Name} in {type.Name}");
                    }
                    return collectionName;
                }
            }
            throw new ArgumentException(
                $"There is no {typeof(CollectionNameAttribute).Name} attribute at {type.Name}");
        }
    }
}
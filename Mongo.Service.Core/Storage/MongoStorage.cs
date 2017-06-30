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
    }
}
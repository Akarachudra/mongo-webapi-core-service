using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public class MongoSettings : IMongoSettings
    {
        public MongoSettings()
        {
            MongoDatabaseName = "MongoServiceCore";
            MongoServers = new[] { new MongoServerAddress("localhost", 27017) };
        }

        public MongoServerAddress[] MongoServers { get; set; }
        public string MongoDatabaseName { get; set; }
        public string MongoUserName { get; set; }
        public string MongoPassword { get; set; }
        public string MongoReplicaSetName { get; set; }
    }
}
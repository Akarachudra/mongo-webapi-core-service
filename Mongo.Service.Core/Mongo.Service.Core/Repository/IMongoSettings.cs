using MongoDB.Driver;

namespace Mongo.Service.Core.Repository
{
    public interface IMongoSettings
    {
        MongoServerAddress[] MongoServers { get; set; }

        string MongoDatabaseName { get; set; }

        string MongoUserName { get; set; }

        string MongoPassword { get; set; }

        string MongoReplicaSetName { get; set; }
    }
}
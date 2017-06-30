namespace Mongo.Service.Core.WebApp
{
    public class ServiceSettings : IServiceSettings
    {
        public int Port { get; set; }

        public ServiceSettings()
        {
            Port = 12512;
        }
    }
}
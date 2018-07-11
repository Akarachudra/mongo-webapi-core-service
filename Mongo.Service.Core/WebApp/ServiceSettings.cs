namespace Mongo.Service.Core.WebApp
{
    public class ServiceSettings : IServiceSettings
    {
        public ServiceSettings()
        {
            this.Port = 12512;
        }

        public int Port { get; set; }
    }
}
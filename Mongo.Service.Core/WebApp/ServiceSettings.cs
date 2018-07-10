namespace Mongo.Service.Core.WebApp
{
    public class ServiceSettings : IServiceSettings
    {
        public int Port { get; set; }

        public ServiceSettings()
        {
            this.Port = 12512;
        }
    }
}
using Mongo.Service.Core.WebApp;
using Topshelf;

namespace Mongo.Service.Core
{
    internal class EntryPoint
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<IApiService>(s =>
                {
                    s.ConstructUsing(settings => new ApiService(new ServiceSettings()));
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });

                x.RunAsLocalSystem();
                x.SetServiceName("Mongo.Service.Core");
                x.SetDisplayName("Mongo.Service.Core");
                x.SetDescription("Mongo.Service.Core");
            });
        }
    }
}
namespace Mongo.Service.Core.WebApp
{
    public class ApiService : IApiService
    {
        private readonly IServiceSettings settings;

        public ApiService(IServiceSettings settings)
        {
            this.settings = settings;
        }
        
        public void Start()
        {
            Microsoft.Owin.Hosting.WebApp.Start<Startup>($"http://+:{settings.Port}/");
        }

        public void Stop()
        {
        }
    }
}
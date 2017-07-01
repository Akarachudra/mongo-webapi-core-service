﻿using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

[assembly: OwinStartup(typeof(Mongo.Service.Core.WebApp.Startup))]

namespace Mongo.Service.Core.WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var container = new Container();
            ConfigureContainer(container);
            container.Verify();

            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Services.Add(typeof(IExceptionLogger), new ExceptionLogger());
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            appBuilder.UseWebApi(config);
        }

        private static void ConfigureContainer(Container container)
        {
            
        }
    }
}
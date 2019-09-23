using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Catalyst.Module.Twitter
{
    public class TwitterModule : Autofac.Module
    {
        private readonly string _siteDirectory;
        private readonly string _apiBindingAddress;
        private readonly string[] _controllerModules;
        private IContainer _container;

        public TwitterModule(string siteDirectory, string apiBindingAddress, List<string> controllerModules)
        {
            _siteDirectory = siteDirectory;
            _apiBindingAddress = apiBindingAddress;
            _controllerModules = controllerModules.ToArray();
        }

        protected override void Load(ContainerBuilder builder)
        {
            var host = WebHost.CreateDefaultBuilder()
                .UseContentRoot(_siteDirectory)
                .ConfigureServices(serviceCollection => ConfigureServices(serviceCollection, builder))
                .Configure(Configure)
                .UseUrls(_apiBindingAddress)
                .UseSerilog()
                .Build();

            builder.RegisterInstance(host).As<IWebHost>();
            builder.RegisterBuildCallback(async container =>
            {
                _container = container;
                var logger = _container.Resolve<ILogger>();

                try
                {
                    await host.StartAsync();
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error loading API");
                }
            });

            base.Load(builder);
        }

        public void ConfigureServices(IServiceCollection services, ContainerBuilder containerBuilder)
        {
            services.AddCors(c => { c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin()); });

            var mvcBuilder = services.AddMvc();

            foreach (var controllerModule in _controllerModules)
            {
                mvcBuilder.AddApplicationPart(Assembly.Load(controllerModule));
            }

            mvcBuilder.AddControllersAsServices();

            containerBuilder.Populate(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices = new AutofacServiceProvider(_container);
            app.UseDeveloperExceptionPage();
            app.UseCors(options => options.AllowAnyOrigin());

            //if (true)
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
            //    {
            //        HotModuleReplacement = true
            //    });
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Meow/Error");
            //}

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    "spa-fallback",
                    new {controller = "Home", action = "Index"});
            });
        }
    }
}
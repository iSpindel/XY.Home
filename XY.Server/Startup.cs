using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using XY.Server.Extensions;
using XY.Server.Settings;
using XY.SignalHub.Hubs;

namespace XY.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MapConfiguration();
        }

        public IConfiguration Configuration;

        private void MapConfiguration()
        {
            MapBrokerHostSettings();
            MapClientSettings();
        }

        private void MapBrokerHostSettings()
        {
            BrokerHostSettings brokerHostSettings = new BrokerHostSettings();
            Configuration.GetSection(nameof(BrokerHostSettings)).Bind(brokerHostSettings);
            AppSettingsProvider.BrokerHostSettings = brokerHostSettings;
        }

        private void MapClientSettings()
        {
            ClientSettings clientSettings = new ClientSettings();
            Configuration.GetSection(nameof(ClientSettings)).Bind(clientSettings);
            AppSettingsProvider.ClientSettings = clientSettings;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
#if DEBUG
                options.EnableDetailedErrors = true;
#endif
            });

            services.AddMqttClientHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                /*endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });*/

                endpoints.MapHub<MqttBridgeHub>("/bridge");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "../XY.App";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}

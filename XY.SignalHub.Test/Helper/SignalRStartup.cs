
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace XY.SignalHub.Test
{
    public class SignalRStartup<T>
        where T : Hub
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseRouting()
                .UseEndpoints(config =>
                {
                    config.MapHub<T>("/test");
                });
        }
    }
}
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using XY.SignalHub.Test;

public static class SignalRConnectionBuilder
{
    public static (IHubContext<THub, TClient>, HubConnection) CreateTestConnection<THub, TClient>()
        where THub : Hub<TClient>
        where TClient : class
    {
        var webHostBuilder = new WebHostBuilder()
             .UseStartup<SignalRStartup<THub>>();
        var server = new TestServer(webHostBuilder);
        var hubContext = server.Services.GetService(typeof(IHubContext<THub, TClient>)) as IHubContext<THub, TClient>;
        return (hubContext, new HubConnectionBuilder()
            .WithUrl(
                new Uri(server.BaseAddress, "/test"),
                options => options.HttpMessageHandlerFactory = _ => server.CreateHandler())
            .Build());
    }
}
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;
using XY.SignalHub.Clients;
using XY.SignalHub.Hubs;
using XY.SignalHub.Messages.MQTTBridge;

namespace XY.SignalHub.Test
{
    public class MQTTBridgeHubTest
    {

        public static readonly string TestDeviceId = "0xD3V";
        public static readonly int TestTimeout = 1000;

        [Fact]
        public async Task SubscribeWithResponse()
        {
            var (_, client) = SignalRConnectionBuilder.CreateTestConnection<MqttBridgeHub, IMqttBridgeClient>();

            await client.StartAsync();
            var result = await client.InvokeAsync<SubscribedMessage>("Subscribe", new SubscribeMessage()
            {
                DeviceId = TestDeviceId
            });

            await client.StopAsync();

            Assert.Equal(TestDeviceId, result.DeviceId);
        }

        [Fact]
        public async Task UnsubscribeWithResponse()
        {
            var (_, client) = SignalRConnectionBuilder.CreateTestConnection<MqttBridgeHub, IMqttBridgeClient>();

            await client.StartAsync();
            var result = await client.InvokeAsync<UnsubscribedMessage>("Unsubscribe", new UnsubscribeMessage()
            {
                DeviceId = TestDeviceId
            });

            await client.StopAsync();

            Assert.Equal(TestDeviceId, result.DeviceId);
        }


        [Fact]
        public async Task SubscribeWithMessage()
        {
            var (server, client) = SignalRConnectionBuilder.CreateTestConnection<MqttBridgeHub, IMqttBridgeClient>();

            string message = null;
            var msgTokenSource = new CancellationTokenSource(TestTimeout);

            client.On<string>("ReceiveMessage", msg =>
            {
                message = msg;
                msgTokenSource.Cancel();
            });

            await client.StartAsync();
            await client.InvokeAsync<SubscribedMessage>("Subscribe", new SubscribeMessage()
            {
                DeviceId = TestDeviceId
            });

            var testMessage = "test";

            await server.Clients.Group(TestDeviceId).ReceiveMessage(testMessage);
            //await hubContext.Clients.All.ReceiveMessage(testMessage);

            await CancelableDelay(msgTokenSource);

            await client.StopAsync();

            Assert.Equal(testMessage, message);
        }

        private static async Task CancelableDelay(CancellationTokenSource tokenSource) {
            try {
                await Task.Delay(TestTimeout, tokenSource.Token);
            } catch(TaskCanceledException) {}
        }
    }
}

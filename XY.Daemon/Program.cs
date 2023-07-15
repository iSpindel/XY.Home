using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;

namespace XY.Daemon
{
    public static class Program
    {
        public static readonly string DAEMON_DIMMER_TIMER = "cmnd/daemon/dimmer/timer";

        public static async Task Main(string[] _)
        {
            // Create a new MQTT client.
            var factory = new MqttFactory();
            var mqttClient = factory.CreateManagedMqttClient();

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("XY.Daemon")
                    .WithTcpServer("yulirox-pi")
                    .WithCredentials("minion", "IPreferNotTo")
                    .WithCleanSession()
                    .Build())
                .Build();

            /*await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("cmnd/dimmer/power").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("cmnd/dimmer/dimmer").Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("stat/dimmer/RESULT").Build());*/

            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(DAEMON_DIMMER_TIMER).Build());

            /*mqttClient.UseApplicationMessageReceivedHandler(msg =>
            {
                var payloadString = Encoding.UTF8.GetString(msg.ApplicationMessage.Payload);
                Console.WriteLine($"{msg.ClientId}: {msg.ApplicationMessage.QualityOfServiceLevel} {msg.ApplicationMessage.Retain} {msg.ApplicationMessage.Topic} {payloadString}");
            });*/

            mqttClient.ApplicationMessageReceivedHandler = new MessageHandler(mqttClient);

            await mqttClient.StartAsync(options);

            /*var tcs = new TaskCompletionSource();
            mqttClient.UseConnectedHandler((e) =>
            {
                tcs.SetResult();
            });

            await tcs.Task;*/

            //var tokenSource = new CancellationTokenSource(50000);
            //mqttClient.PingAsync(tokenSource.Token);

            // StartAsync returns immediately, as it starts a new thread using Task.Run,
            // and so the calling thread needs to wait.
            Console.ReadLine();
        }
    }

    internal class MessageHandler : IMqttApplicationMessageReceivedHandler
    {
        private IManagedMqttClient mqttClient;

        public MessageHandler(IManagedMqttClient mqttClient)
        {
            this.mqttClient = mqttClient;
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            if (eventArgs.ProcessingFailed)
                return Task.CompletedTask;

            if (eventArgs.ApplicationMessage.Topic != Program.DAEMON_DIMMER_TIMER)
                return Task.CompletedTask;

            try
            {
                var message = JsonSerializer.Deserialize<TimerPayload>(eventArgs.ApplicationMessage.Payload);
                Task.Run(async () =>
                {
                    Console.WriteLine($"Waiting for timer {message.Duration}");
                    await Task.Delay(message.Duration * 1000);
                    Console.WriteLine($"Sending command to dimmer {message.Value}");
                    var dimmerMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("cmnd/dimmer/power")
                        .WithPayload(message.Value)
                        .WithExactlyOnceQoS()
                        .WithRetainFlag(false)
                        .Build();
                    await mqttClient.PublishAsync(dimmerMessage, CancellationToken.None);
                });
            }
            catch (JsonException)
            {
            }
            catch (NotSupportedException)
            {
            }

            return Task.CompletedTask;
        }
    }

    internal class TimerPayload
    {
        public int Duration { get; set; }
        public string Value { get; set; }

    }
}

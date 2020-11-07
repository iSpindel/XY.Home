
using System;
using MQTTnet.Client.Options;

namespace XY.Server.Options
{
    public class AspCoreMqttClientOptionBuilder : MqttClientOptionsBuilder
    {
        public IServiceProvider ServiceProvider { get; }

        public AspCoreMqttClientOptionBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
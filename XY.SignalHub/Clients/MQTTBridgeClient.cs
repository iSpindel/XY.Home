using System.Threading.Tasks;
using XY.SignalHub.Messages.MQTTBridge;

namespace XY.SignalHub.Clients
{
    public interface IMqttBridgeClient
    {
        Task ReceiveMessage(string message);
    }
}

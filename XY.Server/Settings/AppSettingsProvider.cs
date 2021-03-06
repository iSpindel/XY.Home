namespace XY.Server.Settings
{
    public class AppSettingsProvider
    {
        public static BrokerHostSettings BrokerHostSettings;
        public static ClientSettings ClientSettings;
    }

    public class BrokerHostSettings
    {
        public string Host { set; get; }
        public int Port { set; get; }
    }

     public class ClientSettings
    {
        public string Id { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
    }
}
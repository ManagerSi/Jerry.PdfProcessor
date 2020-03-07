using System;

namespace Jerry.Model
{
    public class RabbitMqConnection
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ExchangeName { get; set; }
        public string VirtualHost { get; set; }
        public bool IsUseSsl { get; set; }
        public ushort PerFetchCount { get; set; }
        public string QueueName { get; set; }
        public int MaxExecutingCommands { get; set; }



    }
}

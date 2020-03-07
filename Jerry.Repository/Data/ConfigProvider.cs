using Jerry.Model;
using Microsoft.Extensions.Configuration;
using System;

namespace Jerry.Repository.Data
{
    public class ConfigProvider : IConfigProvider
    {
        private readonly IConfiguration _configuration;
        public ConfigProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public RabbitMqConnection GetRabbitMqConnection()
        {
            return new RabbitMqConnection()
            {
                HostName = _configuration.GetValue<string>("RabbitmqSettings:HostName"),
                Port = _configuration.GetValue<int>("RabbitmqSettings:Port"),
                UserName = _configuration.GetValue<string>("RabbitmqSettings:UserName"),
                Password = _configuration.GetValue<string>("RabbitmqSettings:Password"),
                VirtualHost = _configuration.GetValue<string>("RabbitmqSettings:VirtualHost"),
                PerFetchCount = _configuration.GetValue<ushort>("RabbitmqSettings:perFetchCount"),
                ExchangeName = _configuration.GetValue<string>("RabbitmqSettings:ExchangeName"),
                QueueName = _configuration.GetValue<string>("RabbitmqSettings:QueueName"),
                MaxExecutingCommands = _configuration.GetValue<int>("RabbitmqSettings:MaxExecutingCommands"),
               
            };
        }
    }
}

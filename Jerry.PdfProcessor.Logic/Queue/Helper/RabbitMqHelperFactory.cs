using System;
using System.Collections.Generic;
using System.Text;
using Jerry.Repository.Data;
using Microsoft.Extensions.Configuration;

namespace Jerry.PdfProcessor.Logic.Queue.Helper
{
    public class RabbitMqHelperFactory
    {
        public static IConfigProvider _ConfigProvider;
        static RabbitMqHelperFactory()
        {
            var builder = new ConfigurationBuilder().
                SetBasePath(AppDomain.CurrentDomain.BaseDirectory).
                AddJsonFile("appsettings.json", false, true);

            _ConfigProvider=new ConfigProvider( builder.Build());
        }
        public static RabbitMqHelper Default => new RabbitMqHelper(_ConfigProvider);
    }
}

using Jerry.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.Repository.Data
{
    public interface IConfigProvider
    {
        RabbitMqConnection GetRabbitMqConnection();
    }
}

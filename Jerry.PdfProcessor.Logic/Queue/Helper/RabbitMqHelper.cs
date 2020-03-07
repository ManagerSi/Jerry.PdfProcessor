using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jerry.Common.Interface;
using Jerry.Model;
using Jerry.Repository.Data;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jerry.PdfProcessor.Logic.Queue.Helper
{
    public class RabbitMqHelper
    {
        //private readonly string HostName = "localhost";
        //private readonly int Port = 5672;
        //private readonly string UserName = "jerry";
        //private readonly string Password = "123456";
        //private readonly string ExchangeName = "123456";


        private IConnection _connection;
        private IModel _channel;

        private readonly RabbitMqConnection _rabbitMqConnection;
        //private readonly ILogManager _logManager;

        public RabbitMqHelper(IConfigProvider configProvider)
        {
            _rabbitMqConnection = configProvider.GetRabbitMqConnection();

            //创建连接工厂
            var connectionFactory = new ConnectionFactory //创建连接工厂对象
            {
                HostName = _rabbitMqConnection.HostName, //IP地址
                Port = _rabbitMqConnection.Port, //端口号
                UserName = _rabbitMqConnection.UserName, //用户账号
                Password = _rabbitMqConnection.Password //用户密码
            };
            //创建连接
            _connection = connectionFactory.CreateConnection();
            //创建通道
            _channel = _connection.CreateModel();
            //声明交换机
            _channel.ExchangeDeclare(_rabbitMqConnection.ExchangeName,ExchangeType.Topic);
        }

        public void SendMsg<T>(string queName, T msg) where T : class
        {
            //声明队列
            _channel.QueueDeclare(queName, true, false, false, null);

            //绑定交换机
            _channel.QueueBind(queName, _rabbitMqConnection.ExchangeName, queName);
            var messageProperties = _channel.CreateBasicProperties();
            messageProperties.Persistent = true; //是否持久化
            _channel.BasicQos(0, 1, true); //mq单次读取msg数量


            byte[] body;
            if (typeof(T) == typeof(string))
            {
                var Msg = Convert.ToString(msg);
                body = Encoding.UTF8.GetBytes(Msg);
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                using (MemoryStream ms = new MemoryStream())
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    serializer.Serialize(sw, msg);
                    body = ms.ToArray();
                    sw.Flush();
                }
            }

            //_channel.BasicPublish(
            //    "",
            //    queName ,
            //    basicProperties: messageProperties,
            //    mandatory: true,
            //    body: body
            //);

            var address = new PublicationAddress(exchangeType: ExchangeType.Topic, exchangeName: _rabbitMqConnection.ExchangeName,  queName);
            _channel.BasicPublish(address, messageProperties, body);

            _channel.WaitForConfirms(TimeSpan.FromMilliseconds(500));
        }


        public void Receive(string queName, Action<BasicDeliverEventArgs> received)
        {
            //事件基本消费者
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, eventArgs) =>
            {
                string msg = Encoding.UTF8.GetString(eventArgs.Body);
                
                //消息处理
                received(eventArgs);
                
                //返回消息确认
                _channel.BasicAck(eventArgs.DeliveryTag, true);
            };

            //启动消费者，设置为手动应答
            _channel.BasicConsume(queName, false, consumer);
        }

    }
}

using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;

namespace Jerry.PdfProcessor.Test
{
    public class RabbitMqDemo
    {
        private readonly string HostName = "localhost";
        private readonly int Port = 5672;
        private readonly string UserName = "jerry";
        private readonly string Password = "123456";

        private IConnectionFactory connFactory;
        private IConnectionFactory GetConnFactory()
        {
            {
                if (connFactory == null)
                {
                    connFactory = new ConnectionFactory //创建连接工厂对象
                    {
                        HostName = this.HostName, //IP地址
                        Port = Port, //端口号
                        UserName = UserName, //用户账号
                        Password = Password //用户密码
                    };
                }
                return connFactory;
            }
        }

        #region 简单队列/直接声明队列并发消息
        public void TryToWriteMsg_Simple(string queueName)
        {
            using (IConnection con = GetConnFactory().CreateConnection())//创建连接对象
            {
                using (IModel channel = con.CreateModel())//创建连接会话对象
                {
                    //声明一个队列
                    channel.QueueDeclare(
                        queue: queueName,//消息队列名称
                        durable: false,//是否缓存
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                    while (true)
                    {
                        Console.WriteLine("消息内容:");
                        String message = Console.ReadLine();
                        //消息内容
                        byte[] body = Encoding.UTF8.GetBytes(message);
                        //发送消息
                        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                        Console.WriteLine("成功发送消息:" + message);
                    }
                }
            }
        }

        public void TryToReadMsg_Simple(string queueName)
        {
            
            using (IConnection conn = GetConnFactory().CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {

                    //声明一个队列
                    channel.QueueDeclare(
                        queue: queueName,//消息队列名称
                        durable: false,//是否缓存
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                    //告诉Rabbit每次只能向消费者发送一条信息,再消费者未确认之前,不再向他发送信息
                    channel.BasicQos(0, 1, false);
                    //创建消费者对象
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        byte[] message = ea.Body;//接收到的消息
                        Console.WriteLine("接收到信息为:" + Encoding.UTF8.GetString(message));
                        //返回消息确认
                        channel.BasicAck(ea.DeliveryTag, true);
                    };
                    //消费者开启监听
                    channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                    Console.ReadKey();
                }
            }
        }
        #endregion 简单队列

        #region 发布订阅模式(fanout) 接收者/　路由模式(direct),定义不同的routingKey / 通配符模式(topic)

        public void TryToWriteMsg_fanout(string exchangeName)
        {
           
            using (IConnection conn = GetConnFactory().CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName,ExchangeType.Fanout);
                    while (true)
                    {
                        Console.WriteLine("请输入消息内容：");
                        string msg = Console.ReadLine();
                        byte[] body = Encoding.UTF8.GetBytes(msg);

                        channel.BasicPublish(exchange:exchangeName,"",null,body);

                        Console.WriteLine($"发送成功：{msg}");
                    }
                }
            }
        }
        /// <summary>
        /// 发布订阅模式(fanout) 接收者/　路由模式(direct),定义不同的routingKey / 通配符模式(topic)
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        public void TryToReadMsg_fanout(string exchangeName, string queueName)
        {
            using (IConnection conn = GetConnFactory().CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //声明交换机
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
                    //声明队列
                    channel.QueueDeclare(queue:queueName, durable: false, false, false, null);
                    //绑定交换机
                    channel.QueueBind(queue: queueName, exchange: exchangeName, "", null);
                    //手动确认
                    channel.BasicQos(0,1,false);
                    //定义消费者
                    var consumer = new EventingBasicConsumer(channel);
                    //接收事件
                    consumer.Received += (model, eventArgs) =>
                    {
                        byte[] body = eventArgs.Body;
                        Console.WriteLine($"收到的消息为：{Encoding.UTF8.GetString(body)}");

                        //返回消息确认
                        channel.BasicAck(eventArgs.DeliveryTag, true);
                    };
                }
            }
        }
        #endregion
    }
}

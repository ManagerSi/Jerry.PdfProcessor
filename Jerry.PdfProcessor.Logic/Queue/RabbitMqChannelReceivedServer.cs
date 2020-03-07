using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jerry.Common.Interface;
using Jerry.Model;
using Jerry.PdfProcessor.Logic.CommandHandle;
using Jerry.PdfProcessor.Logic.Queue.Helper;
using Jerry.Repository.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jerry.PdfProcessor.Logic.Queue
{
    public class RabbitMqChannelReceivedServer:IChannelReceivedServer
    {
        private readonly ILogManager _logger;
        private readonly ICommandHandleFactory _commandHandleFactory;
        private readonly RabbitMqConnection _rabbitMqConnection;

        private readonly SemaphoreSlim _semaphore;
        //private readonly string HostName = "localhost";
        //private readonly int Port = 5672;
        //private readonly string UserName = "jerry";
        //private readonly string Password = "123456";
        //private readonly string QueueName = "QueueName";



        public RabbitMqChannelReceivedServer(ILogManager logger, IConfigProvider configProvider, ICommandHandleFactory handleFactory)
        {
            _logger = logger;
            _rabbitMqConnection = configProvider?.GetRabbitMqConnection();
            _commandHandleFactory = handleFactory;
            _semaphore = new SemaphoreSlim(_rabbitMqConnection.MaxExecutingCommands);//并发数
            _logger.Info($"{nameof(RabbitMqChannelReceivedServer)} init");
        }
        public void Start(CancellationToken cancellationToken)
        {
           _logger.Info($"{nameof(RabbitMqChannelReceivedServer)} Start");

           RabbitMqHelperFactory.Default.Receive(_rabbitMqConnection.QueueName, MessageReceived);
         
        }

        private void MessageReceived(BasicDeliverEventArgs eventArgs)
        {
            ICommandHandle handle;
            try
            {
                //获取commandtype
                string commandType = null;
                if (!string.IsNullOrEmpty(eventArgs.BasicProperties.Type))
                {
                    if (eventArgs.BasicProperties.Headers?["Type"] !=null)
                    {
                        using(MemoryStream ms = new MemoryStream((byte[])eventArgs.BasicProperties.Headers["Type"]))
                        using (StreamReader sr = new StreamReader(ms))
                        {
                            commandType = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    commandType = eventArgs.BasicProperties.Type;
                }
                //获取handle
                handle = _commandHandleFactory.CreadCommandHandle(commandType);
                if (handle!=null)
                {
                    Task task=new Task(async () =>
                    {
                        try
                        {
                            var resBuffer = await handle.HandleAsync(eventArgs.Body).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            //throw;
                        }
                        finally
                        {
                            //处理结束释放
                            _semaphore.Release();
                        }
                        
                    });

                    _semaphore.Wait();
                    task.Start();
                    Task.WaitAll(task);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            _logger.Info($"received msg:{eventArgs}");
        }


        public void Stop()
        {
            _logger.Info($"{nameof(RabbitMqChannelReceivedServer)} Stop");
        }
        public void Dispose()
        {
            _logger.Info($"{nameof(RabbitMqChannelReceivedServer)} Dispose");
        }
    }
}

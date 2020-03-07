using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Jerry.Common.Interface;
using Jerry.Model;
using Jerry.Repository.Data;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Jerry.PdfProcessor.Logic.Queue
{
    public class RabbitMqChannelWriteClient:IChannelWriteClient
    {
        //private readonly string HostName = "localhost";
        //private readonly int Port = 5672;
        //private readonly string UserName = "jerry";
        //private readonly string Password = "123456";
        //private readonly string VirtualHost = "/";
        //private readonly bool IsUseSsl = false;
        //private readonly ushort perFetchCount = 1;//mq单次读取msg数量
        //private readonly string QueueName = "QueueName";//mq单次读取msg数量

        private IConnection _connection;
        private IModel _model;
        private bool _isConnected;
        private bool _isDisconnecting;
        private readonly ILogManager _logManager;
        private readonly RabbitMqConnection _rabbitMqConnection;
        private Lazy<X509Certificate2> _lazyCert;

        public RabbitMqChannelWriteClient(ILogManager logManager, IConfigProvider configProvider)
        {
            _logManager = logManager;
            _rabbitMqConnection = configProvider.GetRabbitMqConnection();
        }

        public void Dispose()
        {
            Disconnect();
        }

        public void Write<T>(T message, string routingKey, Dictionary<string, object> header)
        {
            try
            {
                _model.ConfirmSelect(); //write后要受到mq的确认通知


                var messageProperties = new BasicProperties
                {
                    Headers = header,
                    ContentType = "text/plain",
                    DeliveryMode = 1,
                    Type = (header!=null && header.ContainsKey("Type"))? header["Type"].ToString():null
                };
                messageProperties.Persistent = true;

                byte[] body;
                if (typeof(T) == typeof(string))
                {
                    var msg = Convert.ToString(message);
                    body = Encoding.UTF8.GetBytes(msg);
                }
                else
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using(MemoryStream ms = new MemoryStream())
                    using (StreamWriter sw = new StreamWriter(ms))
                    {
                        serializer.Serialize(sw,message);
                        body = ms.ToArray();
                        sw.Flush();
                    }
                }

                _model.BasicPublish(
                    "",
                    string.IsNullOrEmpty(routingKey) ? _rabbitMqConnection.QueueName : routingKey,
                    basicProperties: messageProperties,
                    mandatory: true,
                    body: body
                );

                _model.WaitForConfirms(TimeSpan.FromMilliseconds(500));
            }
            catch (Exception ex)
            {
                _logManager.Error("got error when write to {}",ex);
            }
        }

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = _rabbitMqConnection.HostName, //IP地址
                Port = _rabbitMqConnection.Port, //端口号
                UserName = _rabbitMqConnection.UserName, //用户账号
                Password = _rabbitMqConnection.Password, //用户密码
                VirtualHost = _rabbitMqConnection.VirtualHost,
            };
            factory = SetFactoryCertificate(factory);

            _connection = factory.CreateConnection();
            _connection.ConnectionRecoveryError += _connection_ConnectionRecoveryError;
            _connection.ConnectionShutdown += _connection_ConnectionShutdown;
            _connection.RecoverySucceeded += _connection_RecoverySucceeded;

            _model = _connection.CreateModel();
            var messageProperties = _model.CreateBasicProperties();
            messageProperties.Persistent = true; //是否持久化
            _model.BasicQos(0, _rabbitMqConnection.PerFetchCount, true); //mq单次读取msg数量
            //_model.ExchangeDeclare();
            _model.QueueDeclare(_rabbitMqConnection.QueueName, true, false, false, null);

            _model.BasicReturn += _model_BasicReturn;

            _isConnected = true;
            _logManager.Info($"writeclient connect to {_rabbitMqConnection.QueueName}");
        }

        private void _model_BasicReturn(object sender, RabbitMQ.Client.Events.BasicReturnEventArgs e)
        {
            _logManager.Info($"writeclient failed to insert data to {_rabbitMqConnection.QueueName}");
            Disconnect(); ;
        }

        private void _connection_RecoverySucceeded(object sender, EventArgs e)
        {
            _logManager.Error($"writeclient got error,  reconnected to {_rabbitMqConnection.QueueName}");
        }

        private void _connection_ConnectionRecoveryError(object sender, RabbitMQ.Client.Events.ConnectionRecoveryErrorEventArgs e)
        {
            _logManager.Error($"writeclient got error, reconnecting to {_rabbitMqConnection.QueueName}");
        }

        private void _connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (_isDisconnecting)
            {
                _logManager.Error($"writeclient got error, lost to {_rabbitMqConnection.QueueName}");
            }
        }

        #region Certificate
        private ConnectionFactory SetFactoryCertificate(ConnectionFactory factory)
        {

            factory.Ssl = _rabbitMqConnection.IsUseSsl
                ? new SslOption()
                {
                    Enabled = true,
                    ServerName = factory.HostName,
                    AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors|SslPolicyErrors.RemoteCertificateNameMismatch,
                    CertificateSelectionCallback = ConfigOnCertificateSelection,
                    Version =  SslProtocols.Tls12
                }
                : new SslOption()
                {
                    Enabled = false
                };
            factory.AuthMechanisms = _rabbitMqConnection.IsUseSsl
                ? new List<AuthMechanismFactory>() {new ExternalMechanismFactory()}
                : ConnectionFactory.DefaultAuthMechanisms; 

            return factory;
        }

        private X509Certificate ConfigOnCertificateSelection(object sender, string targethost, X509CertificateCollection localcertificates, X509Certificate remotecertificate, string[] acceptableissuers)
        {
            _lazyCert = _lazyCert ?? new Lazy<X509Certificate2>(CertFactory);
            return _lazyCert.Value;
        }

        private X509Certificate2 CertFactory()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var cerFindType = X509FindType.FindBySubjectName;
                var cerFindValue = "Jerry.PdfProcessor";
                var certificateCol = store.Certificates.Find(cerFindType, cerFindValue, true);
                store.Close();
                return certificateCol[0];
            }
        }
        #endregion
        public void Disconnect()
        {
            if (_isConnected)
            {
                _isDisconnecting = true;
                if (_model != null)
                {
                    _model.Dispose();
                    _model = null;
                }

                if (_connection !=null)
                {
                    _connection.Dispose();
                    _connection = null;
                }

                _isDisconnecting = false;
                _isConnected = false;

                _logManager.Info($"writeclient disconnect to {_rabbitMqConnection.QueueName}");
            }
        }
    }
}

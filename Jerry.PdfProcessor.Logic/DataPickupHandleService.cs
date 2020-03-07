using Jerry.Common.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jerry.Model;
using Jerry.PdfProcessor.Logic.Queue;
using Newtonsoft.Json;

namespace Jerry.PdfProcessor.Logic
{
    public class DataPickupHandleService:IDisposable
    {
        private readonly ILogManager _logger;
        private IChannelWriteClient _channelWriteClient;

        public DataPickupHandleService(ILogManager logger, IChannelWriteClient channelWriteClient)
        {
            _logger = logger;
            _channelWriteClient = channelWriteClient;
            _logger.Info($"{nameof(DataPickupHandleService)} init.");
        }

        /// <summary>
        /// triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void Start(CancellationToken cancellationToken)
        {
            try
            {
                _logger.Info($"{nameof(DataPickupHandleService)} StartAsync.");

                _channelWriteClient.Connect();

                Dictionary<string, object> hearder = new Dictionary<string, object>
                {
                    {"ProgramCode", "PdfProcessor"},
                    {"Type", CommandHandleType.SampleCommandHandle.ToString()},
                };

                for (int i = 0; i < 10; i++)
                {
                    if (i>4 && i<=8)
                    {
                        hearder["Type"] = CommandHandleType.Pdf1CommandHandle.ToString();
                    }
                    if (i > 8)
                    {
                        hearder["Type"] = "invalidType";
                    }
                    var request = new BaseRequest()
                    {
                        Header =  new RequestHeader()
                        {
                            RequestId = new Guid(),
                            Options = new Dictionary<string, string>() { { i.ToString(), i.ToString() } }
                        },
                        ProgramCode = "PdfProcessor"
                    };

                    string msg = JsonConvert.SerializeObject(request);
                    _channelWriteClient.Write(msg, "", hearder);
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(DataPickupHandleService)} StartAsync. occor error",ex);
                if (_channelWriteClient != null)
                {
                    _channelWriteClient.Disconnect();
                    _channelWriteClient = null;
                }

                throw;
            }
        }

        public void Stop()
        {
            _logger.Info($"{nameof(DataPickupHandleService)} StopAsync.");
            _channelWriteClient.Disconnect();
        }

        public void Dispose()
        {
            _logger.Info($"{nameof(DataPickupHandleService)} Dispose.");
            _channelWriteClient.Dispose();
        }
    }
}

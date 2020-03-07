using Jerry.Common.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jerry.PdfProcessor.Logic.Queue;

namespace Jerry.PdfProcessor.Logic
{
    public class PdfGenerateHandleService:IDisposable
    {
        private readonly ILogManager _logger;
        private IChannelReceivedServer _channelReceived;

        public PdfGenerateHandleService(ILogManager logger, IChannelReceivedServer channelReceived)
        {
            _logger = logger;
            _channelReceived = channelReceived;
            _logger.Info($"{nameof(PdfGenerateHandleService)} init.");
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
                _logger.Info($"{nameof(PdfGenerateHandleService)} StartAsync.");

                _channelReceived.Start(cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(PdfGenerateHandleService)} StartAsync. occor error",ex);
                if (_channelReceived != null)
                {
                   
                }

                throw;
            }
        }

        public void Stop()
        {
            _logger.Info($"{nameof(PdfGenerateHandleService)} StopAsync.");
            _channelReceived.Stop();
        }

        public void Dispose()
        {
            _logger.Info($"{nameof(PdfGenerateHandleService)} Dispose.");
            _channelReceived.Dispose();
        }
    }
}

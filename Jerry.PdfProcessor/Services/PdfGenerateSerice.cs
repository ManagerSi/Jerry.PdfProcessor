using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jerry.Common.Interface;
using Jerry.PdfProcessor.Logic;
using Jerry.PdfProcessor.Logic.Queue;
using Microsoft.Extensions.Hosting;

namespace Jerry.PdfProcessor.Services
{
    public class PdfGenerateSerice:IHostedService, IDisposable
    {
        private readonly ILogManager _logger;
        private PdfGenerateHandleService _pdfGenerateHandle;
        public PdfGenerateSerice(ILogManager logger, IChannelReceivedServer channelReceived)
        {
            _logger = logger;
            _pdfGenerateHandle = new PdfGenerateHandleService(logger,channelReceived);
            _logger.Info($"{nameof(PdfGenerateSerice)} init.");
        }

        /// <summary>
        /// triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"{nameof(PdfGenerateSerice)} StartAsync.");
            _pdfGenerateHandle.Start(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"{nameof(PdfGenerateSerice)} StopAsync.");
            _pdfGenerateHandle.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.Info($"{nameof(PdfGenerateSerice)} Dispose.");
            _pdfGenerateHandle.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jerry.Common.Interface;
using Jerry.PdfProcessor.Logic;
using Jerry.PdfProcessor.Logic.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Jerry.PdfProcessor.Services
{
    public class DataPickupService:IHostedService, IDisposable
    {
        private readonly ILogManager _logger;
        private DataPickupHandleService _dataPickupHandle;

        public DataPickupService(ILogManager logger, IChannelWriteClient channelWriteClient)
        {
            _logger = logger;
            _dataPickupHandle = new DataPickupHandleService(logger, channelWriteClient);

            _logger.Info($"{nameof(DataPickupService)} init.");
        }

        /// <summary>
        /// triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"{nameof(DataPickupService)} StartAsync.");
            try
            {
                return KeepTryingToConnect(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(DataPickupService)} StartAsync. occor error", ex);
                if (_dataPickupHandle != null)
                {
                    lock (this)
                    {
                        _dataPickupHandle.Dispose();
                        _dataPickupHandle = null;
                    }
                }

                throw;
            }
        }

        private async Task KeepTryingToConnect(CancellationToken cancellationToken)
        {
            var isConnected = false;
            while (!isConnected && !cancellationToken.IsCancellationRequested)
            {
                _logger.Info($"{nameof(DataPickupService)} KeepTryingToConnect.");

                lock (this)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            _dataPickupHandle.Start(cancellationToken);
                            isConnected = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"{nameof(DataPickupService)} KeepTryingToConnect. occor error", ex);
                        }
                    }
                }

                if (isConnected)
                {
                    _logger.Info($"{nameof(DataPickupService)} Connected.");
                }
                else
                {
                    _logger.Error($"{nameof(DataPickupService)} failed to Connected. occor error");

                    await Task.Delay(TimeSpan.FromSeconds(60));
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"{nameof(DataPickupService)} StopAsync.");
            try
            {
                lock (this)
                {
                    _dataPickupHandle.Stop();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(DataPickupService)} StopAsync. occor error",ex);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.Info($"{nameof(DataPickupService)} Dispose.");
        }
    }
}

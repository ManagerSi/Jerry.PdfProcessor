using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jerry.PdfProcessor.Logic.Queue
{
    public interface IChannelReceivedServer :IDisposable
    {
        void Start(CancellationToken cancellationToken);
        void Stop();
    }
}

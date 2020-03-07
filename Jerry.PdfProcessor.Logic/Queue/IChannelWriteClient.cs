using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.PdfProcessor.Logic.Queue
{
    public interface IChannelWriteClient:IDisposable
    {
        void Write<T>(T message, string routingKey, Dictionary<string, object> header);
        void Connect();
        void Disconnect();

    }
}

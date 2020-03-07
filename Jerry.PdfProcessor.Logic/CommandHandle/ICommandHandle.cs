using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jerry.PdfProcessor.Logic.CommandHandle
{
    public interface ICommandHandle
    {
        Task<byte[]> HandleAsync(byte[] requestBuffer);
    }
}

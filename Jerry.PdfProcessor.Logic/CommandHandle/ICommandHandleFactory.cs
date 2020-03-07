using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Jerry.PdfProcessor.Logic.CommandHandle
{
    public interface ICommandHandleFactory
    {
        ICommandHandle CreadCommandHandle(string commandTypd);
    }
}

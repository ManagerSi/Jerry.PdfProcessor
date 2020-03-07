using System;
using System.Collections.Generic;
using System.Text;

namespace Jerry.Common.Interface
{
    public interface ILogManager
    {
        void Info(object msg, Exception exp = null);
        void Debug(object msg, Exception exp = null);
        void Error(object msg, Exception exp = null);
        void Warn(object msg, Exception exp = null);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Jerry.Common.Interface;
using NLog;

namespace Jerry.Common.Imp
{
    public class NLogManager:ILogManager
    {

        private static readonly Logger _logger = NLog.LogManager.GetLogger("DefaultLogger"); 
        //private static readonly Logger log = NLog.LogManager.GetLogger("RequestLogger"); //LogManager.GetLogger("");

        public  void Error(object msg, Exception exp = null)
        {
            if (exp == null)
                _logger.Error("#" + msg);
            else
                _logger.Error(exp, msg.ToString());
        }

        public  void Debug(object msg, Exception exp = null)
        {
            if (exp == null)
                _logger.Debug("#" + msg);
            else
                _logger.Debug("#" + msg , exp.ToString());
        }

        public  void Info(object msg, Exception exp = null)
        {
            if (exp == null)
                _logger.Info("#" + msg);
            else
                _logger.Info(exp, "#" + msg);
        }


        public  void Warn(object msg, Exception exp = null)
        {
            if (exp == null)
                _logger.Warn("#" + msg);
            else
                _logger.Warn(exp, "#" + msg);
        }
    }
}

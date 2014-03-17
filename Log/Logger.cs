using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log
{
    public class Logger
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
        private static readonly Logger interlog = new Logger();

        public static Logger GetLogger()
        {
            return interlog;
        }

        public void Info(string msg)
        {
            log.Info(msg);
        }
    }
}

namespace CommonLib
{
    public class Logger
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
        private static readonly Logger interlog = new Logger();

        private Logger()
        {
            System.IO.FileInfo file = new System.IO.FileInfo("../../../CommonLib/Log.config");

            if(file.Exists)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(file);
            }
            else
            {
                System.Console.WriteLine("配置文件不存在");
            }

            //log4net.Config.XmlConfigurator.ConfigureAndWatch();
        }

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

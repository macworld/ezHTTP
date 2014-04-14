using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Log
{
    /// <summary>
    /// 日志类定义，使用该日志类可以允许不同使用者输出到不同终端
    /// 当前包含的终端：
    /// 1. 使用者独有的Windows Form窗口，推荐调试使用
    /// 2. 向特定窗口程序发送日志消息
    /// 3. 其它log4net标准输出终端
    /// </summary>
    public class Logger
    {
        /// <summary>
        ///  默认的唯一构造函数
        /// </summary>
        /// <param name="loggerName">
        /// 日志记录器Logger的名称
        /// </param>
        public  Logger(string loggerName)
        {
            LoggerName = loggerName;
            InterLogger m_interlogger = InterLogger.GetIngerLogger();
        }

        /// <summary>
        /// 日志记录器Logger的名称，作为属性保存
        /// </summary>
        public string LoggerName
        {
            get { return m_loggerName; }
            set { m_loggerName = value; }
        }

        /// <summary>
        ///  输出Debug级别的日志消息
        /// </summary>
        /// <param name="debug">
        /// 要输出的日志消息
        /// </param>
        public void Debug(string debug)
        {
            InterLogger.Debug(m_loggerName, debug);
        }

        /// <summary>
        ///  输出Info级别的日志消息
        /// </summary>
        /// <param name="info">
        /// 要输出的日志消息
        /// </param>
        public void Info(string info)
        {
            InterLogger.Info(m_loggerName, info);
        }

        /// <summary>
        ///  输出Warn级别的日志消息
        /// </summary>
        /// <param name="warn">
        /// 要输出的日志消息
        /// </param>
        public void Warn(string warn)
        {
            InterLogger.Warn(m_loggerName, warn);
        }

        /// <summary>
        ///  输出Error级别的日志消息
        /// </summary>
        /// <param name="error">
        /// 要输出的日志消息
        /// </param>
        public void Error(string error)
        {
            InterLogger.Error(m_loggerName, error);
        }

        /// <summary>
        ///  输出Fatal级别的日志消息
        /// </summary>
        /// <param name="fatal">
        /// 要输出的日志消息
        /// </param>
        public void Fatal(string fatal)
        {
            InterLogger.Fatal(m_loggerName, fatal);
        }

        /// <summary>
        /// 属性对应的私有成员
        /// </summary>
        private string m_loggerName;
    }

    #region Logger类内部使用的实际用于日志输出的InterLogger类
    /// <summary>
    /// Logger类内部使用的实际用于日志输出的InterLogger类，static属性
    /// </summary>
    public class InterLogger
    {
        /// <summary>
        /// InterLogger类的唯一不可见实例，只能通过GetLogger获得
        /// </summary>
        private static readonly InterLogger m_interlogger = new InterLogger();

        /// <summary>
        /// 构造类时，检查配置文件并按配置文件配置
        /// </summary>
        private  InterLogger()
        {
            System.IO.FileInfo file = new System.IO.FileInfo("../../Log.config");

            if (file.Exists)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(file);
            }
            else
            {
                System.Console.WriteLine("配置文件不存在");
            }
        }

        /// <summary>
        ///  外部获得InterLogger类的唯一内部实例时使用的方法
        /// </summary>
        /// <returns>
        ///  InterLogger类的唯一内部实例
        /// </returns>
        public static InterLogger GetIngerLogger()
        {
            return m_interlogger;
        }

        /// <summary>
        ///  输出Debug级别的日志消息
        /// </summary>
        /// <param name="logger">
        /// 配置文件中，logger的name属性值
        /// </param>
        /// <param name="debug">
        /// 要输出的Debug级别日志消息
        /// </param>
        public static void Debug(string logger, string debug)
        {
            log4net.LogManager.GetLogger(logger).Debug(debug);
        }

        /// <summary>
        ///  输出Info级别的日志消息
        /// </summary>
        /// <param name="logger">
        /// 配置文件中，logger的name属性值
        /// </param>
        /// <param name="info">
        /// 要输出的Info级别日志消息
        /// </param>
        public static void Info(string logger, string info)
        {
            log4net.LogManager.GetLogger(logger).Info(info);
        }

        /// <summary>
        ///  输出Warn级别的日志消息
        /// </summary>
        /// <param name="logger">
        /// 配置文件中，logger的name属性值
        /// </param>
        /// <param name="warn">
        /// 要输出的Warn级别日志消息
        /// </param>
        public static void Warn(string logger, string warn)
        {
            log4net.LogManager.GetLogger(logger).Warn(warn);
        }

        /// <summary>
        ///  输出Error级别的日志消息
        /// </summary>
        /// <param name="logger">
        /// 配置文件中，logger的name属性值
        /// </param>
        /// <param name="error">
        /// 要输出的Error级别日志消息
        /// </param>
        public static void Error(string logger, string error)
        {
            log4net.LogManager.GetLogger(logger).Error(error);
        }

        /// <summary>
        ///  输出Fatal级别的日志消息
        /// </summary>
        /// <param name="logger">
        /// 配置文件中，logger的name属性值
        /// </param>
        /// <param name="fatal">
        /// 要输出的Fatal级别日志消息
        /// </param>
        public static void Fatal(string logger, string fatal)
        {
            log4net.LogManager.GetLogger(logger).Fatal(fatal);
        }
    }
    #endregion Logger类内部使用的实际用于日志输出的InterLogger类
}

    

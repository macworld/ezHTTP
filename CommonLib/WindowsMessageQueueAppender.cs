using System;
using System.Runtime.InteropServices;

namespace CommonLib
{
    public class WindowsMessageQueueAppender : log4net.Appender.AppenderSkeleton
    {
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsMessageQueueAppender" /> class.
        /// </summary>
        /// <remarks>
        /// The default constructor initializes all fields to their default values.
        /// </remarks>
        public WindowsMessageQueueAppender()
        {
        }

        #endregion Public Instance Constructors

        #region Public Instance Properties

        /// <summary>
        ///  窗口消息目的窗口的窗口类名，用于查找目的窗口
        ///  如果为空，则只通过窗口名查找
        /// </summary>
        public string WindowClass
        {
            get { return m_windowClass; }
            set { m_windowClass = value; }
        }

        /// <summary>
        /// 窗口消息目的窗口的窗口名 ，用于查找目的窗口
        /// </summary>
        public string WindowName
        {
            get { return m_windowName; }
            set { m_windowName = value; }
        }

        #endregion Public Instance Properties

        #region Implementation of IOptionHandler

        /// <summary>
        ///  实现IOptionHandler的部分方法
        /// </summary>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (this.WindowClass == "")
            {
                this.WindowClass = null; // "" 和null是不一样的
            }

            if (this.WindowName == null || this.WindowName == "")
            {
                throw new ArgumentNullException("要查找的窗口窗口名不能为空.");
            }
        }

        #endregion

        #region Override implementation of AppenderSkeleton

        /// <summary>
        /// 用于添加一条日志事件
        /// </summary>
        /// <param name="loggingEvent">
        ///  日志事件
        /// </param>
        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            try
            {
                this.Send(loggingEvent);
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("无法通过消息队列向目的窗口发送日志消息", ex, log4net.Core.ErrorCode.WriteFailure);
            }
        }

        /// <summary>
        ///  向目的窗口发送一条日志消息
        /// </summary>
        /// <param name="loggingEvent">
        ///  日志事件
        /// </param>
        protected void Send(log4net.Core.LoggingEvent loggingEvent)
        {
            COPYDATASTRUCT data;

            string msg = RenderLoggingEvent(loggingEvent);

            data.cbData = msg.Length * 2;
            data.dwData = (IntPtr)loggingEvent.Level.Value;
            data.Message = msg;


            SendMessage(FindWindow(this.WindowClass, this.WindowName), WM_COPYDATA, new IntPtr(0), ref data);
        }

        #region 发送窗口消息用到的数据结构和API函数
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Message;
        }

        //声明 API 函数 
        [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessage(int hWnd, int msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [System.Runtime.InteropServices.DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        //定义消息常数 
        public const int WM_COPYDATA = 0x004A; // 数据拷贝消息

        #endregion 发送窗口消息用到的数据结构和API函数

        /// <summary>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </summary>
        /// <value><c>true</c></value>
        /// <remarks>
        /// <para>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </para>
        /// </remarks>
        override protected bool RequiresLayout
        {
            get { return true; }
        }

        #endregion Override implementation of AppenderSkeleton

        // 私有变量
        private string m_windowClass;
        private string m_windowName;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Timers;
using CommonLib;
using Microsoft.Win32;

namespace ezHttp
{
    public partial class MainWindow : Window
    {
        Timer timer = new Timer();
        public const int LOG_DEBUG = 30000; // 日志调试级别
        public const int LOG_INFO = 40000; // 日志通知级别
        public const int LOG_WARN = 60000; // 日志警告级别
        public const int LOG_ERROR = 70000; // 日志错误级别
        public const int LOG_FATAL = 110000; // 日志致命错误级别

        public const int WM_COPYDATA = 0x004A; // 数据拷贝消息
        /// <summary>
        /// 传递数据的数据结构
        /// </summary>
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Message;
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            (PresentationSource.FromVisual(this) as HwndSource).AddHook(new HwndSourceHook(this.WndProc));
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == WM_COPYDATA)
            {

                COPYDATASTRUCT data = (COPYDATASTRUCT)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                TextBlock tempBlock = new TextBlock();
                
                string str = "";
                for (int i = 0; i < data.cbData / 2; i++)
                    str += data.Message[i];
                int type = (int)data.dwData;

                switch(type)
                {
                    case LOG_DEBUG:
                        tempBlock.Foreground = new SolidColorBrush(Color.FromRgb(50,50,50));
                        break;
                    case LOG_ERROR:
                        tempBlock.Foreground = new SolidColorBrush(Color.FromRgb(224, 119, 119));
                        break;
                    case LOG_FATAL:
                        tempBlock.Foreground = new SolidColorBrush(Color.FromRgb(224, 119, 119));
                        break;
                    case LOG_INFO:
                        tempBlock.Foreground = new SolidColorBrush(Color.FromRgb(138, 201, 238));
                        break;
                    case LOG_WARN:
                        tempBlock.Foreground = new SolidColorBrush(Color.FromRgb(224, 194, 19));
                        break;
                    default:
                        break;
                }
                tempBlock.Text = str;
                tempBlock.TextWrapping =TextWrapping.Wrap;
                tempBlock.Height = 20;
                listbox_log.Items.Add(tempBlock);
                //when no item was selected,scorll the latest into view
                if(listbox_log.SelectedItem==null)
                {
                    listbox_log.ScrollIntoView(tempBlock);
                }
                //when there was too many log in the screen
                if(listbox_log.Items.Count>500)
                {
                    listbox_log.Items.RemoveAt(0);
                }
              
            }
            return hwnd;
        }


        private void text_clear_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure to clear the log", "Remind", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                listbox_log.Items.Clear();
            }
        }

        private void text_clear_MouseEnter(object sender, MouseEventArgs e)
        {
            text_clear.FontWeight = FontWeights.Bold;
        }

        private void text_clear_MouseLeave(object sender, MouseEventArgs e)
        {
            text_clear.FontWeight = FontWeights.Normal;
        }

        private void text_view_log_MouseEnter(object sender, MouseEventArgs e)
        {
            text_view_log.FontWeight = FontWeights.Bold;
        }

        private void text_view_log_MouseLeave(object sender, MouseEventArgs e)
        {
            text_view_log.FontWeight = FontWeights.Normal;
        }

        private void text_view_log_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            System.Diagnostics.Process.Start("NOTEPAD.EXE", "log.txt");
 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
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
using System.Windows.Threading;
using CommonLib;

namespace ezHttp
{
    /// <summary>
    /// MainWindow 的主体交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Color clicked_color = Color.FromRgb(0, 0, 0);
        Color default_color = Color.FromRgb(124, 124, 124);
        Color start_color = Color.FromRgb(112,173,71);
        Color stop_color = Color.FromRgb(243, 153, 62);
        bool isSettingsChanged = false;
        ChartViewModel StateCharts = new ChartViewModel();
        private ChartInfo cpuinfo,bufferinfo,coninfo;
        private PerformanceCounter cpuCounter,memCounter;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = StateCharts;

            cpuinfo = StateCharts.CpuInfo.First();
            bufferinfo = StateCharts.FileBufferInfo.First();
            coninfo = StateCharts.ConnectionInfo.First();
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            OnHomeClicked(null, null);
            //inital the settings
            InitSettings();
            //add hook to receive log message
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            DispatcherTimer stateTimer = new DispatcherTimer();
            stateTimer.Interval = TimeSpan.FromSeconds(0.5);
            stateTimer.Tick += new EventHandler(StatetimerTick);//更新状态信息
            stateTimer.Start();
        }

        private void StatetimerTick(object sender, EventArgs e)
        {
            cpuinfo.Number = (int)cpuCounter.NextValue();
            if (MainLogic.Started)
            {
                coninfo.Number = (100*MainLogic.socketServer.ConnetionNum)/MainLogic.socketServer.MaxConnections;
                bufferinfo.Number = MainLogic.fileBuffer.BufferUsage;
            }
            else
            {
                coninfo.Number = 0;
                bufferinfo.Number = 0;
            }

        } 
        private void ResetTag()
        {
            text_home.Foreground = new SolidColorBrush(default_color);
            text_home.FontWeight = FontWeights.Normal;
            text_log.Foreground = new SolidColorBrush(default_color);
            text_log.FontWeight = FontWeights.Normal;
            text_settings.Foreground = new SolidColorBrush(default_color);
            text_settings.FontWeight = FontWeights.Normal;
            content_home.Visibility = Visibility.Hidden;
            content_settings.Visibility = Visibility.Hidden;
            content_log.Visibility = Visibility.Hidden;
        }

        private void OnHomeClicked(object sender, MouseButtonEventArgs e)
        {

            ResetTag();
            text_home.Foreground = new SolidColorBrush(clicked_color);
            text_home.FontWeight = FontWeights.Bold;
            content_home.Visibility = Visibility.Visible;
        }

        private void OnLogClicked(object sender, MouseButtonEventArgs e)
        {

            ResetTag();
            text_log.Foreground = new SolidColorBrush(clicked_color);
            text_log.FontWeight = FontWeights.Bold;
            content_log.Visibility = Visibility.Visible;
        }

        private void OnSettingsClicked(object sender, MouseButtonEventArgs e)
        {
            ResetTag();
            text_settings.Foreground = new SolidColorBrush(clicked_color);
            text_settings.FontWeight = FontWeights.Bold;
            content_settings.Visibility = Visibility.Visible;
        }

        private void OnStartClicked(object sender, MouseButtonEventArgs e)
        {

            if (MainLogic.Started)
            {
                MainLogic.StopService();
                text_start.Foreground = new SolidColorBrush(start_color);
                text_start.Text = "Start";
            }
            else
            {
                if (MainLogic.StartService())
                {
                    text_start.Foreground = new SolidColorBrush(stop_color);
                    text_start.Text = "Stop";
                }
            }
        }


        private void text_home_MouseEnter(object sender, MouseEventArgs e)
        {
          
            text_home.TextDecorations = TextDecorations.Underline;

        }

        private void text_home_MouseLeave(object sender, MouseEventArgs e)
        {
            text_home.TextDecorations = null;
        }

        private void text_log_MouseEnter(object sender, MouseEventArgs e)
        {
            text_log.TextDecorations = TextDecorations.Underline;
        }

        private void text_log_MouseLeave(object sender, MouseEventArgs e)
        {
            text_log.TextDecorations = null;
        }

        private void text_settings_MouseEnter(object sender, MouseEventArgs e)
        {
            text_settings.TextDecorations = TextDecorations.Underline;
        }

        private void text_settings_MouseLeave(object sender, MouseEventArgs e)
        {
            text_settings.TextDecorations = null;
        }

        private void text_start_MouseEnter(object sender, MouseEventArgs e)
        {
            text_start.FontWeight = FontWeights.Bold;
        }

        private void text_start_MouseLeave(object sender, MouseEventArgs e)
        {
            text_start.FontWeight = FontWeights.Normal;
        }

    }
}

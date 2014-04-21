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
using System.Windows.Media.Animation;
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
        Color stop_color = Color.FromRgb(0xcc, 0x66, 62);
        bool isSettingsChanged = false;
        ChartViewModel StateCharts = new ChartViewModel();
        private ChartInfo cpuinfo,bufferinfo,coninfo;
        private PerformanceCounter cpuCounter;
        private bool cpuWarn = false, fileWarn = false, connectWarn = false;
        private int fadeCount = -1;
        private Storyboard aboutShow, aboutFade;
        private bool isAboutShowing = false, isAboutFading = false,isAboutMouseLeave = true;
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

            aboutFade =  (Storyboard)this.Resources["FadeAbout"];
            aboutShow = (Storyboard)this.Resources["ShowAbout"];
            aboutShow.Completed += OnAboutShowComplete;
            aboutFade.Completed += ChangeAboutName;


            OnHomeClicked(null, null);
            //inital the settings
            InitSettings();
            //add hook to receive log message
            Loaded += MainWindow_Loaded;
            DispatcherTimer stateTimer = new DispatcherTimer();
            stateTimer.Interval = TimeSpan.FromSeconds(0.5);
            stateTimer.Tick += StatetimerTick;//更新状态信息
            stateTimer.Start();
        }

        private void UpdateStateText(string state, SolidColorBrush color)
        {
            Storyboard showState;
            showState = (Storyboard)this.Resources["ShowState"];
            StatusText.Foreground = color;
            StatusText.Text = state;
            showState.Begin();
            fadeCount = 6;
        }

        private void ButtonTurnGreen()
        {
            Storyboard tg;
            tg = (Storyboard)this.Resources["turnGreen"];
            tg.Begin();
            start_rectangle.Stroke = new SolidColorBrush(start_color);
        }

        private void ButtonTurnOrange()
        {
            Storyboard to;
            to = (Storyboard)this.Resources["turnOrange"];
            to.Begin();
            start_rectangle.Stroke = new SolidColorBrush(stop_color);
        }

        private void FadeState()
        {
            Storyboard fadeState;
            fadeState = (Storyboard)this.Resources["FadeState"];
            fadeState.Begin();
            fadeCount = -1;
        }

        private void StatetimerTick(object sender, EventArgs e)
        {
            Logger log = new Logger("AppLogger");
            cpuinfo.Number = (int)cpuCounter.NextValue();


            if (fadeCount > 0) fadeCount--;
            else if (fadeCount == 0)  FadeState();

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

            if (cpuinfo.Number > 80 && cpuWarn == false)
            {
                log.Warn("Warning: High CPU Usage !");
                UpdateStateText("Warning: High CPU Usage !", new SolidColorBrush(stop_color));
                cpuWarn = true;
            }
            if (cpuinfo.Number < 40 && cpuWarn == true)
            {
                cpuWarn = false;
            }

            if (bufferinfo.Number > 80 && fileWarn == false)
            {
                log.Warn("Warning: High FileBuffer Usage !");
                UpdateStateText("Warning: High FileBuffer Usage !", new SolidColorBrush(stop_color));
                fileWarn = true;
            }
            if (bufferinfo.Number < 40 && fileWarn == true)
            {
                fileWarn = false;
            }

            if (coninfo.Number > 80 && connectWarn == false)
            {
                log.Warn("Warning: High Connection Pool Usage !");
                UpdateStateText("Warning: High Connection Pool Usage !", new SolidColorBrush(stop_color));
                connectWarn = true;
            }
            if (coninfo.Number < 40 && connectWarn == true)
            {
                connectWarn = false;
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
                ButtonTurnGreen();
                UpdateStateText("EasyWebServer Stoped", new SolidColorBrush(stop_color));
            }
            else
            {
                if (MainLogic.StartService())
                {
                    UpdateStateText("EasyWebServer Started",new SolidColorBrush(start_color));
                    text_start.Foreground = new SolidColorBrush(stop_color);
                    text_start.Text = "Stop";
                    ButtonTurnOrange();
                }
                else
                {
                    UpdateStateText("Start failed, please get more information on log", new SolidColorBrush(Color.FromRgb(0xaa, 0, 0)));
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
            Storyboard tg, to;
            tg = (Storyboard) this.Resources["turnGreen"];
            to = (Storyboard) this.Resources["turnOrange"];
            if (MainLogic.Started) to.Begin();
            else tg.Begin();
        }

        private void text_start_MouseLeave(object sender, MouseEventArgs e)
        {
            Storyboard tg, to;
            tg = (Storyboard)this.Resources["turnGreen"];
            to = (Storyboard)this.Resources["turnOrange"];
            if (MainLogic.Started) to.Stop();
            else tg.Stop();
        }

        private void ChangeAboutName(object sender, EventArgs e)
        {
            Random rnd = new Random();
            switch (rnd.Next(0, 5))
            {

                case 0:
                    Author.Text = "Macworld";
                    AboutBak.Fill = new SolidColorBrush(Color.FromRgb(81,0,49));
                    break;
                case 1:
                    Author.Text = "Wu jin";
                    AboutBak.Fill = new SolidColorBrush(Color.FromRgb(37, 94, 145));
                    break;
                case 2:
                    Author.Text = "Yuan XinChen";
                    AboutBak.Fill = new SolidColorBrush(Color.FromRgb(0xcc, 0x66, 00));
                    break;
                case 3:
                    Author.Text = "Zhao WeiFeng";
                    AboutBak.Fill = new SolidColorBrush(Color.FromRgb(0x33, 0x49, 0x5e));
                    break;
                case 4:
                    Author.Text = "Zhuang Jia";
                    AboutBak.Fill = new SolidColorBrush(Color.FromRgb(9, 13, 99));
                    break;
            }
            isAboutFading = false;
        }

        private void OnAboutShowComplete(object sender, EventArgs e)
        {
            isAboutShowing = false;
            if (isAboutMouseLeave && isAboutFading == false) aboutFade.Begin();
        }
        private void AboutButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            isAboutMouseLeave = false;
            if (isAboutFading || isAboutShowing) return;
            aboutShow.Begin();
            isAboutShowing = true;
        }
        private void AboutButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            isAboutMouseLeave = true;
            if (isAboutShowing || isAboutFading) return;
            aboutFade.Begin();
            isAboutFading = true;
        }

        private void DragBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	if (e.ChangedButton == MouseButton.Left)
        this.DragMove();
        }

        private void Minimize_btn_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	WindowState = WindowState.Minimized;
        }

        private void Close_btn_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.Close();
        }
    }
}

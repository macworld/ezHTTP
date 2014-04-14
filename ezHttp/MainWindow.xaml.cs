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
using CommonLib;

namespace wpfAppTest
{
    /// <summary>
    /// MainWindow 的主体交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Color clicked_color = Color.FromRgb(72, 163, 233);
        Color default_color = Color.FromRgb(124, 124, 124);
        Color start_color = Color.FromRgb(57, 184, 255);
        Color stop_color = Color.FromRgb(243, 153, 62);

        bool isInitialed = false;
        bool isSettingsChanged = false;
        public MainWindow()
        {
            InitializeComponent();
            OnHomeClicked(null, null);
            //inital the settings
            InitSettings();
            //add hook to receive log message
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }


        private void OnHomeClicked(object sender, MouseButtonEventArgs e)
        {
            text_home.Foreground = new SolidColorBrush(clicked_color);
            text_log.Foreground = new SolidColorBrush(default_color);
            text_settings.Foreground = new SolidColorBrush(default_color);
            content_home.Visibility = Visibility.Visible;
            content_settings.Visibility = Visibility.Hidden;
            content_log.Visibility = Visibility.Hidden;
        }

        private void OnLogClicked(object sender, MouseButtonEventArgs e)
        {
            text_log.Foreground = new SolidColorBrush(clicked_color);
            text_home.Foreground = new SolidColorBrush(default_color);
            text_settings.Foreground = new SolidColorBrush(default_color);
            content_home.Visibility = Visibility.Hidden;
            content_settings.Visibility = Visibility.Hidden;
            content_log.Visibility = Visibility.Visible;
        }

        private void OnSettingsClicked(object sender, MouseButtonEventArgs e)
        {
            text_settings.Foreground = new SolidColorBrush(clicked_color);
            text_home.Foreground = new SolidColorBrush(default_color);
            text_log.Foreground = new SolidColorBrush(default_color);
            content_home.Visibility = Visibility.Hidden;
            content_settings.Visibility = Visibility.Visible;
            content_log.Visibility = Visibility.Hidden;
        }

        private void OnStartClicked(object sender, MouseButtonEventArgs e)
        {
            if (text_start.Foreground.ToString() == new SolidColorBrush(start_color).ToString())
            {
                text_start.Foreground = new SolidColorBrush(stop_color);
                text_start.Text = "Stop";
                if (!isInitialed)
                {
                    MainLogic.StartService();
                    isInitialed = true;
                }
                else
                {
                    MainLogic.RestartService();
                }

            }
            else
            {
                text_start.Foreground = new SolidColorBrush(start_color);
                text_start.Text = "Start";
                MainLogic.StopService();
            }

        }


        private void text_home_MouseEnter(object sender, MouseEventArgs e)
        {
            text_home.FontWeight = FontWeights.Bold;

        }

        private void text_home_MouseLeave(object sender, MouseEventArgs e)
        {
            text_home.FontWeight = FontWeights.Normal;
        }

        private void text_log_MouseEnter(object sender, MouseEventArgs e)
        {
            text_log.FontWeight = FontWeights.Bold;
        }

        private void text_log_MouseLeave(object sender, MouseEventArgs e)
        {
            text_log.FontWeight = FontWeights.Normal;
        }

        private void text_settings_MouseEnter(object sender, MouseEventArgs e)
        {
            text_settings.FontWeight = FontWeights.Bold;
        }

        private void text_settings_MouseLeave(object sender, MouseEventArgs e)
        {
            text_settings.FontWeight = FontWeights.Normal;
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

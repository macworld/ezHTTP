using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace ezHttp
{
    /// <summary>
    /// this file was the logical part of settings
    /// </summary>
    public partial class MainWindow : Window
    {
        Color apply_color = Color.FromRgb(17, 17, 17);
        Color remind_success_color = Color.FromRgb(50, 50, 150);
        Color remind_error_color = Color.FromRgb(247, 59, 59);
        private void InitSettings()
        {
            textbox_serverDirectory.Text = FileManager.Properties.FileManagerSettings.Default.ServerDirectory;
            textbox_filebuffer.Text = FileManager.Properties.FileManagerSettings.Default.PageSize *
                FileManager.Properties.FileManagerSettings.Default.MaxPageNum / 1024 / 1024 + "MB";
            textbox_listenport.Text = Convert.ToString(SocketManager.Properties.SocketManager.Default.ListenPort);
            textbox_maxconnection.Text = Convert.ToString(SocketManager.Properties.SocketManager.Default.MaxConnections);
            textbox_homedic.Text = Convert.ToString(HttpParser.Properties.HttpParserSettings.Default.WelcomeFilePath);
            checkbox_ipv6.IsChecked = SocketManager.Properties.SocketManager.Default.IPv6;
            SetUnChanged();
        }
        /// <summary>
        /// set the apply text to be unchanged
        /// </summary>
        private void SetUnChanged()
        {
            isSettingsChanged = false;
            text_apply.Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180));
        }

        private void text_apply_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isSettingsChanged)
            {
                text_apply.FontWeight = FontWeights.Bold;
            }
        }

        private void text_apply_MouseLeave(object sender, MouseEventArgs e)
        {
            text_apply.FontWeight = FontWeights.Normal;
        }

        private void text_deafult_MouseEnter(object sender, MouseEventArgs e)
        {
            text_deafult.FontWeight = FontWeights.Bold;
        }

        private void text_deafult_MouseLeave(object sender, MouseEventArgs e)
        {
            text_deafult.FontWeight = FontWeights.Normal;
        }

        private void text_apply_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isSettingsChanged)
            {
                return;
            }
            string ServerDirectory = textbox_serverDirectory.Text;
            string FileBuffer = textbox_filebuffer.Text;
            string ListenPort = textbox_listenport.Text;
            string MaxConnection = textbox_maxconnection.Text;
            string HomeDic = textbox_homedic.Text;
            text_remind.Text = "";
            //detect wheather the input is effective
            if(!DetectDirectory(ServerDirectory))
            {
                return;
            }
            if(!DetectFileBuffer(ref FileBuffer))
            {
                return;
            }
            if(!DetectPortNum(ListenPort))
            {
                return;
            }
            if(!DetectHomeDic(ServerDirectory,HomeDic))
            {
                return;
            }
            if(!DetectMaxConnection(MaxConnection))
            {
                return;
            }
            //apply the changes, user need to restart the whole application to make changes effective
            FileManager.Properties.FileManagerSettings.Default.ServerDirectory = ServerDirectory;

            int maxPageNum = Convert.ToInt32(FileBuffer) * 1024 * 1024 /
                FileManager.Properties.FileManagerSettings.Default.PageSize;
            FileManager.Properties.FileManagerSettings.Default.MaxPageNum = maxPageNum;
            FileManager.Properties.FileManagerSettings.Default.Save();

            SocketManager.Properties.SocketManager.Default.ListenPort = Convert.ToInt32(ListenPort);
            SocketManager.Properties.SocketManager.Default.MaxConnections = Convert.ToInt32(MaxConnection);
            SocketManager.Properties.SocketManager.Default.IPv6 = Convert.ToBoolean(checkbox_ipv6.IsChecked);
            SocketManager.Properties.SocketManager.Default.Save();

            HttpParser.Properties.HttpParserSettings.Default.WelcomeFilePath = HomeDic;
            HttpParser.Properties.HttpParserSettings.Default.Save();
            text_remind.Text = "Settings have been saved, restart service to take effect!";
            text_remind.Foreground = new SolidColorBrush(remind_success_color);
            SetUnChanged();
        }

        private void text_deafult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure to reset your settings?", "Remind", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                FileManager.Properties.FileManagerSettings.Default.Reset();
                SocketManager.Properties.SocketManager.Default.Reset();
                InitSettings();
            }
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isSettingsChanged = true;
            text_apply.Foreground = new SolidColorBrush(apply_color);
        }

        private void checkbox_ipv6_Checked(object sender, RoutedEventArgs e)
        {
            isSettingsChanged = true;
            text_apply.Foreground = new SolidColorBrush(apply_color);
        }

        private bool DetectDirectory(string ServerDirectory)
        {
            if (!Directory.Exists(ServerDirectory))
            {
                text_remind.Text = "Please Use A ServerDirectory Which Is Existed";
                text_remind.Foreground = new SolidColorBrush(remind_error_color);
                SetUnChanged();
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool DetectFileBuffer(ref string FileBuffer)
        {
            if (FileBuffer.Length >= 2)
            {
                if (FileBuffer.Substring(FileBuffer.Length - 2) == "MB")
                {
                    FileBuffer = FileBuffer.Substring(0, FileBuffer.Length - 2);
                }
                if (Convert.ToInt32(FileBuffer) >= 4096)
                {
                    text_remind.Text = "The size of file buffer should be less than 4096.";
                    text_remind.Foreground = new SolidColorBrush(remind_error_color);
                    SetUnChanged();
                    return false;
                }
            }
            return true;
        }

        private bool DetectPortNum(string ListenPort)
        {
            if (Convert.ToInt32(ListenPort) < 1024 || Convert.ToInt32(ListenPort) > 65535)
            {
                text_remind.Text = "The value of ListenPort Should between 1023 and 65536(not include).";
                text_remind.Foreground = new SolidColorBrush(remind_error_color);
                SetUnChanged();
                return false;
            }
            return true;
        }

        private bool DetectHomeDic(string ServerDirectory,string HomeDic)
        {
            string filepath=ServerDirectory+HomeDic;
            if(!File.Exists(filepath))
            {
                text_remind.Text = "You should enter an existed file as home page.";
                text_remind.Foreground = new SolidColorBrush(remind_error_color);
                SetUnChanged();
                return false;
            }
            return true;
        }

        private bool DetectMaxConnection(string MaxConnection)
        {
            if (Convert.ToInt32(MaxConnection) < 100)
            {
                text_remind.Text = "The Max Connection should be bigger than 100.";
                text_remind.Foreground = new SolidColorBrush(remind_error_color);
                SetUnChanged();
                return false;
            }
            return true;
        }
        

        private void textbox_filebuffer_GotFocus(object sender, RoutedEventArgs e)
        {
            textbox_filebuffer.Text = textbox_filebuffer.Text.Substring(0,textbox_filebuffer.Text.Length - 2);
        }

        private void textbox_filebuffer_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textbox_filebuffer.Text.Length >= 2 & textbox_filebuffer.Text.Substring(textbox_filebuffer.Text.Length-2)!="MB")
            {
                textbox_filebuffer.Text += "MB";
            }
            
        }
        //to ensure user can only enter number
        private void textbox_KeyDown_number(object sender, KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;
            //屏蔽非法按键，只能输入整数
            if (((e.Key >= Key.D0 && e.Key <= Key.D9)) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }

}

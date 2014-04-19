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
            if (!Directory.Exists(ServerDirectory))
            {
                text_remind.Text = "Please Use A ServerDirectory Which Is Existed";
                SetUnChanged();
                return;
            }
            if (Convert.ToInt32(ListenPort) < 1024 || Convert.ToInt32(ListenPort) > 65535)
            {
                text_remind.Text = "The value of ListenPort Should between 1023 and 65536(not include).";
                SetUnChanged();
                return;
            }
            FileBuffer = FileBuffer.Substring(0, FileBuffer.Length - 2);
            if (Convert.ToInt32(FileBuffer) >= 4096)
            {
                text_remind.Text = "The size of file buffer is too big.";
                SetUnChanged();
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
    }

}

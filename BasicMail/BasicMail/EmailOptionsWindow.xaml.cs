using BasicMail.Generic;
using BasicMailSharedClasses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BasicMail
{
    public partial class EmailOptionsWindow : Window
    {
        String initUsername;

        public EmailOptionsWindow()
        {
            InitializeComponent();

            this.DataContext = Globals.mainView;

            initUsername = Globals.mainView.Username != null ? Globals.mainView.Username : "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            emailOptionsSubmenu_listBox.SelectedIndex = 0;
        }

        private void emailOptionsSubmenu_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ListBoxItem)((ListBox)sender).SelectedItem).Tag.ToString())
            {
                case "SMTP/IMAP":
                    smtpImapPage_grid.Visibility = Visibility.Visible;
                    userInfoPage_grid.Visibility = Visibility.Hidden;
                    break;

                default:
                    smtpImapPage_grid.Visibility = Visibility.Hidden;
                    userInfoPage_grid.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            Globals.mainView.SMTPAddress = smtpAddress_textBox.Text.Trim();
            Globals.mainView.SMTPPort = Convert.ToInt32(smtpPort_textBox.Text.Trim());
            Globals.mainView.IMAPAddress = imapAddress_textBox.Text.Trim();
            Globals.mainView.IMAPPort = Convert.ToInt32(imapPort_textBox.Text.Trim());

            appConfig.AppSettings.Settings["SMTPAddress"].Value = smtpAddress_textBox.Text.Trim();
            appConfig.AppSettings.Settings["SMTPPort"].Value = smtpPort_textBox.Text.Trim();
            appConfig.AppSettings.Settings["IMAPAddress"].Value = imapAddress_textBox.Text.Trim();
            appConfig.AppSettings.Settings["IMAPPort"].Value = imapPort_textBox.Text.Trim();

            if (Globals.mainView.LoggedIn
                && initUsername.Trim() != Globals.mainView.Username.Trim())
            {
                Globals.mainView.LoggedIn = false;
                Globals.mainView.DisplayLoginPanel = true;

                appConfig.AppSettings.Settings["Username"].Value = userName_textBox.Text.Trim();
                ((MainWindow)this.Owner).password_passwordBox.Password = password_passwordBox.Password;

                _ = ((MainWindow)this.Owner).AttemptLoginAsync();
            }
            else
            {
                Common.SaveConfig(appConfig);
            }

            this.Close();
        }
    }
}

using BasicMailSharedClasses;
using BasicMailStylization;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static BasicMailSharedPublicStructs.PublicStructs;

namespace BasicMail.ViewModels.Main
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ObservableCollection<Email> emails = new ObservableCollection<Email>();
        private ObservableCollection<ActionLogEntry> actions = new ObservableCollection<ActionLogEntry>();
        private Guid appInstance = Guid.NewGuid();
        private Boolean sendingEmail = false;
        private Boolean loggingIn = false;
        private Boolean loggedIn = false;
        private Boolean displayLoginPanel = true;
        private Boolean displayLoginElements = true;
        private SecureString? appPassword;
        private String composeErrorMessage = "";
        private String replyErrorMessage = "";
        private String header = "";
        private String sender = "";
        private String date = "";
        private String body = "";

        public ObservableCollection<Email> Emails
        {
            get
            {
                return emails;
            }

            set
            {
                emails = value;
                OnPropertyChanged("Emails");
            }
        }

        public ObservableCollection<ActionLogEntry> Actions
        {
            get
            {
                return actions;
            }

            set
            {
                actions = value;

                OnPropertyChanged("Actions");
            }
        }

        public Guid AppInstance
        {
            get
            {
                return appInstance;
            }
        }
        public Boolean IsEmailSendingInUse
        {
            get
            {
                return !sendingEmail && loggedIn;
            }
        }

        public Boolean IsAuthenticationInfoInUse
        {
            get
            {
                return !sendingEmail && !loggingIn;
            }
        }

        public Boolean SendingEmail
        {
            get
            {
                return sendingEmail;
            }

            set
            {
                sendingEmail = value;
                OnPropertyChanged("IsEmailSendingInUse");
                OnPropertyChanged("IsAuthenticationInfoInUse");
            }
        }

        public Boolean LoggingIn
        {
            get
            {
                return loggingIn;
            }

            set
            {
                loggingIn = value;
                OnPropertyChanged("LoggingIn");
                OnPropertyChanged("IsAuthenticationInfoInUse");
            }
        }

        public Boolean LoggedIn
        {
            get
            {
                return loggedIn;
            }

            set
            {
                loggedIn = value;
                OnPropertyChanged("LoggedIn");
                OnPropertyChanged("IsEmailSendingInUse");
            }
        }

        public Boolean DisplayLoginPanel
        {
            get
            {
                return displayLoginPanel;
            }

            set
            {
                displayLoginPanel = value;
                OnPropertyChanged("DisplayLoginPanel");
            }
        }

        public Boolean DisplayLoginElements
        {
            get
            {
                return displayLoginElements;
            }

            set
            {
                displayLoginElements = value;
                OnPropertyChanged("DisplayLoginElements");
            }
        }

        public String LogsPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"Logs\";
            }
        }

        public String ComposeErrorMessage
        {
            get
            {
                return composeErrorMessage;
            }
            set
            {
                composeErrorMessage = value;
                OnPropertyChanged("ComposeErrorMessage");
            }
        }

        public String ReplyErrorMessage
        {
            get
            {
                return replyErrorMessage;
            }
            set
            {
                replyErrorMessage = value;
                OnPropertyChanged("ReplyErrorMessage");
            }
        }

        public String Username
        {
            get
            {
                return ConfigurationManager.AppSettings["Username"]!;
            }

            set
            {
                ConfigurationManager.AppSettings["Username"] = value;
                OnPropertyChanged("Username");
            }
        }

        public SecureString? AppPassword
        {
            get
            {
                return appPassword;
            }

            set
            {
                appPassword = value;
            }
        }

        public String Header
        {
            get
            {
                return header;
            }

            set
            {
                header = value;
                OnPropertyChanged("Header");
            }
        }

        public String Sender
        {
            get
            {
                return sender;
            }

            set
            {
                sender = value;
                OnPropertyChanged("Sender");
            }
        }

        public String Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
                OnPropertyChanged("Date");
            }
        }

        public String Body
        {
            get
            {
                return body;
            }

            set
            {
                body = value;
                OnPropertyChanged("Body");
            }
        }

        public String SMTPAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["SMTPAddress"]!;
            }

            set
            {
                ConfigurationManager.AppSettings["SMTPAddress"] = value;
            }
        }

        public String IMAPAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["IMAPAddress"]!;
            }

            set
            {
                ConfigurationManager.AppSettings["IMAPAddress"] = value;
            }
        }

        public Int32 SMTPPort
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]!);
            }

            set
            {
                ConfigurationManager.AppSettings["SMTPPort"] = value.ToString();
            }
        }

        public Int32 IMAPPort
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["IMAPPort"]!);
            }

            set
            {
                ConfigurationManager.AppSettings["IMAPPort"] = value.ToString();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] String name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

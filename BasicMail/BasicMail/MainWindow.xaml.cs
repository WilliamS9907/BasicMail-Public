using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MimeKit;
using BasicMail.Generic;
using System.Configuration;
using static BasicMailSharedPublicStructs.PublicStructs;
using BasicMailAPI;
using BasicMailSharedClasses;
using System.IO;
using System;
using System.Drawing;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using System.Globalization;

namespace BasicMail
{
    public partial class MainWindow : Window
    {
        Int32 maxSendAttempts = 3;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = Globals.mainView;

            if (!Directory.Exists(Globals.mainView.LogsPath))
            {
                Directory.CreateDirectory(Globals.mainView.LogsPath);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Globals.mainView.Actions.CollectionChanged += (sender, e) => _ = Common.WriteActionLogToFileAsync(Globals.mainView.LogsPath,
                                                                                                                    Globals.mainView.AppInstance,
                                                                                                                    Globals.mainView.Actions);
        }

        #region User Authorization
        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            _ = AttemptLoginAsync();
        }

        private void userName_textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CheckForKeystrokeLoginAttempt(e);
        }

        private void password_passwordBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            CheckForKeystrokeLoginAttempt(e);
        }

        private void CheckForKeystrokeLoginAttempt(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _ = AttemptLoginAsync();
            }
        }

        public async Task AttemptLoginAsync()
        {
            try
            {
                Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                Globals.mainView.LoggedIn = false;
                Globals.mainView.DisplayLoginPanel = true;
                inbox_listBox.IsEnabled = false;

                if (inbox_listBox.SelectedItem != null)
                {
                    inbox_listBox.SelectedItem = null;
                    ResetInboxSelectionFrameworkElements();
                }

                Globals.mainView.ComposeErrorMessage = "";
                Globals.mainView.ReplyErrorMessage = "";

                ResetReplyFrameworkElements();
                ResetCompositionPanelFrameworkElements(composePanelOuter_border.Visibility);

                Globals.mainView.Username = userName_textBox.Text;
                Globals.mainView.AppPassword = password_passwordBox.SecurePassword;

                Globals.mainView.LoggingIn = true;
                Globals.mainView.DisplayLoginElements = false;

                Globals.mainView.Emails = await AuthAndMailboxHandler.LoginAsync(Globals.mainView.Username, Globals.mainView.AppPassword,
                                                                                       Globals.mainView.IMAPAddress, Globals.mainView.IMAPPort,
                                                                                       authenticationFeedback_textBlock,
                                                                                       this);

                inbox_listBox.IsEnabled = true;
                Globals.mainView.LoggingIn = false;
                Globals.mainView.LoggedIn = true;
                Globals.mainView.DisplayLoginPanel = false;

                this.Dispatcher.Invoke(() =>
                {
                    appConfig.AppSettings.Settings["Username"].Value = userName_textBox.Text.Trim();
                });

                Common.SaveConfig(appConfig);
            }
            catch (Exception ex)
            {
                authError_textBlock.Text = "Auth failed, see action log for details (Options -> Action Log)...";

                Globals.mainView.Actions.Add(Common.GetActionLogEntry("Auth Failure", 
                                                                      ex.Message + "\n" + ex.StackTrace));
                
                Globals.mainView.LoggingIn = false;
                Globals.mainView.LoggedIn = false;
                Globals.mainView.DisplayLoginElements = true;
            }
        }

        private void refresh_button_Click(object sender, RoutedEventArgs e)
        {
            _ = AttemptLoginAsync();
        }
        #endregion User Authorization

        #region Send Reply
        private void reply_button_Click(object sender, RoutedEventArgs e)
        {
            _ = ReplyAsync();
        }

        private async Task ReplyAsync()
        {
            Boolean sendingFailed = false;
            Exception reportedException = new Exception();
            Email? inboxSelection = inbox_listBox.SelectedItem != null ? (Email)inbox_listBox.SelectedItem! : null;

            reply_button.Content = "Replying...";
            Globals.mainView.SendingEmail = true;

            for (int i = 0; i < maxSendAttempts; i++)
            {
                try
                {
                    if (inboxSelection != null)
                    {
                        await this.Dispatcher.Invoke(async () =>
                        {
                            MimeMessage reply;

                            Globals.mainView.ReplyErrorMessage = "";

                            reply = EmailHandler.CreateReply(((Email)inboxSelection).originalEmail!,
                                                             sendReplyInput_textBox.Text,
                                                             (Boolean)replyToAll_checkBox.IsChecked!);

                            await CallAPIToSendEmail(reply);

                            Globals.mainView.Actions.Add(Common.GetActionLogEntry(((Email)inboxSelection).originalEmail!.Subject, "N/A", Globals.mainView.Username,
                                                                                  ((Email)inboxSelection).originalEmail!.From.ToString(),
                                                                                  ((Email)inboxSelection).originalEmail!.Cc != null ? ((Email)inboxSelection).originalEmail!.Cc.ToString() : "",
                                                                                  ((Email)inboxSelection).originalEmail!.Bcc != null ? ((Email)inboxSelection).originalEmail!.Bcc.ToString() : "",
                                                                                  ((Email)inboxSelection).originalEmail!.Subject, sendReplyInput_textBox.Text, EmailStatus.Sent));

                            ResetReplyFrameworkElements();

                            replyError_textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                            Globals.mainView.ReplyErrorMessage = "Email succesfully sent!";
                        });
                    }
                    else
                    {
                        Globals.mainView.Actions.Add(Common.GetActionLogEntry("ORIGINAL EMAIL INVALID", "No email selected for reply", Globals.mainView.Username,
                                                                              "ORIGINAL EMAIL INVALID", "ORIGINAL EMAIL INVALID",
                                                                              "ORIGINAL EMAIL INVALID", "ORIGINAL EMAIL INVALID", sendReplyInput_textBox.Text, EmailStatus.Failed));

                        replyError_textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                        Globals.mainView.ReplyErrorMessage = "No email selected, could not send reply...";
                    }
                    break;
                }
                catch (Exception ex)
                {
                    HandleRetryReport(reply_button, i);

                    await Task.Delay(1000);

                    if (i == 2)
                    {
                        reportedException = ex;
                        sendingFailed = true;
                    }
                }
            }

            if (sendingFailed)
            {
                HandleSendingError(reportedException, true);
            }

            reply_button.Content = "Reply";
            Globals.mainView.SendingEmail = false;
        }

        private void ResetReplyFrameworkElements()
        {
            Globals.mainView.ReplyErrorMessage = "";
            sendReplyInput_textBox.Text = "";
            replyToAll_checkBox.IsChecked = false;
        }
        #endregion Send Reply

        #region Send New
        private async Task SendNewEmail()
        {
            Boolean sendingFailed = false;
            Exception reportedException = new Exception();

            composeSend_button.Content = "Sending...";
            Globals.mainView.SendingEmail = true;

            for (int i = 0; i < maxSendAttempts; i++)
            {
                try
                {
                    MimeMessage email;

                    Globals.mainView.ComposeErrorMessage = "";

                    email = EmailHandler.CreateNewEmail(composeSubject_textBox.Text,
                                                        composeBody_textBox.Text,
                                                        composeTo_textBox.Text,
                                                        composeCc_textBox.Text,
                                                        composeBcc_textBox.Text);

                    await CallAPIToSendEmail(email);

                    Globals.mainView.Actions.Add(Common.GetActionLogEntry(composeSubject_textBox.Text, "N/A", Globals.mainView.Username,
                                                                          composeTo_textBox.Text, composeCc_textBox.Text,
                                                                          composeBcc_textBox.Text, composeSubject_textBox.Text,
                                                                          composeBody_textBox.Text, EmailStatus.Sent));

                    ResetCompositionPanelFrameworkElements();

                    composeError_textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    Globals.mainView.ComposeErrorMessage = "Email succesfully sent!";
                    break;
                }
                catch (Exception ex)
                {
                    HandleRetryReport(composeSend_button, i);

                    await Task.Delay(1000);

                    if (i == 2)
                    {
                        reportedException = ex;
                        sendingFailed = true;
                    }
                }
            }

            if (sendingFailed)
            {
                HandleSendingError(reportedException);
            }

            Globals.mainView.SendingEmail = false;
            composeSend_button.Content = "Send";
        }
        #endregion Send New

        #region Send Email Common
        private async Task CallAPIToSendEmail(MimeMessage email)
        {
            await EmailHandler.SendEmail(email,
                                         Globals.mainView.SMTPAddress,
                                         Globals.mainView.SMTPPort,
                                         Globals.mainView.Username,
                                         Globals.mainView.AppPassword!);

            Globals.mainView.SendingEmail = false;
        }

        private void HandleRetryReport(Button mainButtonControl, Int32 numberOfAttempts)
        {
            mainButtonControl.Content = "Retrying (" + (numberOfAttempts + 1) + ")...";
        }

        private void HandleSendingError(Exception ex, Boolean reply = false)
        {
            if (reply)
            {
                MimeMessage originalEmail = ((Email)inbox_listBox.SelectedItem).originalEmail!;

                Globals.mainView.Actions.Add(Common.GetActionLogEntry(originalEmail.Subject, ex.Message + "\n" + ex.StackTrace, Globals.mainView.Username,
                                                                      originalEmail.From.ToString(), originalEmail.Cc != null ? originalEmail.Cc.ToString() : "",
                                                                      originalEmail.Bcc != null ? originalEmail.Bcc.ToString() : "",
                                                                      originalEmail.Subject, sendReplyInput_textBox.Text));

                replyError_textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                Globals.mainView.ReplyErrorMessage = "Message failed to send, see action log for details (Options -> Action Log)...";
            }
            else
            {
                Globals.mainView.Actions.Add(Common.GetActionLogEntry(String.IsNullOrEmpty(composeSubject_textBox.Text) ? "NO TITLE" : composeSubject_textBox.Text,
                                                                      ex.Message + "\n" + ex.StackTrace, Globals.mainView.Username,
                                                                      composeTo_textBox.Text, composeCc_textBox.Text,
                                                                      composeBcc_textBox.Text, composeSubject_textBox.Text,
                                                                      composeBody_textBox.Text));

                composeError_textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                Globals.mainView.ComposeErrorMessage = "Message failed to send, see action log for details (Options -> Action Log)...";
            }
        }
        #endregion Send Email Common

        #region Mail Display Handling
        private void inbox_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inbox_listBox.SelectedItem != null)
            {
                Email email = (Email)inbox_listBox.SelectedItem;

                Globals.mainView.Sender = email.Sender! ?? "Sender NULL";
                Globals.mainView.Date = email.Date!?? "Date NULL";
                Globals.mainView.Body = GetEmailBody(email.Body!);

                Globals.mainView.Header = email.Subject! ?? "Subject NULL";
            }
            else
            {
                ResetInboxSelectionFrameworkElements();
            }
        }

        private void ResetInboxSelectionFrameworkElements()
        {
            Globals.mainView.Sender = "";
            Globals.mainView.Date = "";
            Globals.mainView.Body = "";
            Globals.mainView.Header = "";
        }

        private String GetEmailBody(String body)
        {
            if (String.IsNullOrEmpty(body)
                || String.IsNullOrWhiteSpace(body))
            {
                return "INTERNAL ERROR: Unable to process body. This means the element is either empty and/or some applet with no text return.";
            }

            return body;
        }
        #endregion Mail Display Handling

        #region New Email Composition Handling & Framework Elements
        private void composeTitleBarMinimizeMaximize_button_Click(object sender, RoutedEventArgs e)
        {
            switch (composePanelOuter_border.ActualWidth == 100)
            {
                case true:
                    MaximizeCompositionPanel();
                    break;

                case false:
                    MinimizeCompositionPanel();
                    break;
            }
        }

        private void MaximizeCompositionPanel()
        {
            composePanelOuter_border.Height = 325;
            composePanelOuter_border.Width = 490;
            composeTitleBarHeader_textBlock.Width = 400;
            composeTitleBarMinimizeMaximize_button.Content = "_";
        }

        private void MinimizeCompositionPanel()
        {
            composePanelOuter_border.Height = composeTitleBar_border.ActualHeight;
            composePanelOuter_border.Width = 100;
            composeTitleBarHeader_textBlock.Width = 25;
            composeTitleBarMinimizeMaximize_button.Content = "🗖";
        }

        private void composeTitleBarClose_button_Click(object sender, RoutedEventArgs e)
        {
            ResetCompositionPanelFrameworkElements(Visibility.Hidden);
        }

        private void compose_button_Click(object sender, RoutedEventArgs e)
        {
            ResetCompositionPanelFrameworkElements();
        }

        private void ResetCompositionPanelFrameworkElements(Visibility vis = Visibility.Visible)
        {
            composePanelOuter_border.Visibility = vis;
            composeTo_textBox.Text = "";
            composeCc_textBox.Text = "";
            composeBcc_textBox.Text = "";
            composeSubject_textBox.Text = "";
            composeBody_textBox.Text = "";

            MaximizeCompositionPanel();
        }

        private void composeSend_button_Click(object sender, RoutedEventArgs e)
        {
            _ = SendNewEmail();
        }
        #endregion New Email Composition Handling & Framework Elements

        #region Toolbar
        private void emailOptions_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            EmailOptionsWindow emWin = new EmailOptionsWindow()
            {
                Owner = this
            };

            emWin.ShowDialog();
        }

        private void actionLog_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ActionLogWindow alWin = new ActionLogWindow()
            {
                Owner = this
            };

            alWin.ShowDialog();
        }
        #endregion Toolbar
    }
}
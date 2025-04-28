using BasicMailAPI.Generic;
using MailKit.Search;
using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static BasicMailSharedPublicStructs.PublicStructs;
using MailKit.Net.Imap;
using System.Collections.ObjectModel;
using BasicMailSharedClasses;

namespace BasicMailAPI
{
    public static class AuthAndMailboxHandler
    {
        static Int32 emailsPerPageLimit = 50;

        #region Authorization
        public static async Task<ObservableCollection<Email>> LoginAsync(String username, SecureString password, 
                                                                         String imapAddress, Int32 imapPort, 
                                                                         TextBlock statusTextBlock = null!,
                                                                         Window statusTextBlockOwner = null!)
        {
            ObservableCollection<Email> emails = new ObservableCollection<Email>();

            await Task.Factory.StartNew(() =>
            {
                using (ImapClient imap = new ImapClient())
                {
                    imap.AuthenticationMechanisms.Remove("XOAUTH2");
                    ReportLoginStatus("Connecting...", statusTextBlock, statusTextBlockOwner);
                    imap.Connect(imapAddress, imapPort, true);
                    ReportLoginStatus("Authenticating...", statusTextBlock, statusTextBlockOwner);
                    imap.Authenticate(username, Common.SecureStringToString(password));

                    ReportLoginStatus("Opening Mailboxes...", statusTextBlock, statusTextBlockOwner);
                    emails = GetInbox(imap.Inbox, statusTextBlock, statusTextBlockOwner);

                    ReportLoginStatus("Getting Mailbox Address...", statusTextBlock, statusTextBlockOwner);
                    Globals.usersMailboxAddress = GetUsersMailboxAddress(username, emails);
                }
            }, TaskCreationOptions.LongRunning);

            return emails;
        }
        #endregion Authorization

        #region Inbox and GetUserMailboxAddress
        private static ObservableCollection<Email> GetInbox(IMailFolder inbox, TextBlock statusTextBlock,
                                                            Window statusTextBlockOwner)
        {
            ObservableCollection<Email> emails = new ObservableCollection<Email>();
            Int32 emailCount = 0;

            inbox.Open(FolderAccess.ReadOnly);

            SearchResults rawHtmlEmails = inbox.Search(SearchOptions.All, SearchQuery.All);

            foreach (UniqueId rawEmailId in rawHtmlEmails.UniqueIds.Reverse())
            {
                emailCount++;

                ReportLoginStatus("Getting mail " + emailCount + " out of " + emailsPerPageLimit + "...", 
                                  statusTextBlock, statusTextBlockOwner);

                MimeMessage message = inbox.GetMessage(rawEmailId);


                emails.Add(new Email
                {
                    Subject = message.Subject,
                    Body = message.TextBody,
                    Date = message.Date.ToString(),
                    Sender = message.From.ToString(),
                    originalEmail = message
                });

                if (emailCount >= emailsPerPageLimit)
                {
                    break;
                }
            }

            return emails;
        }

        private static MailboxAddress GetUsersMailboxAddress(String username, ObservableCollection<Email> emails)
        {
            MailboxAddress senderMailbox = new MailboxAddress("", username);
            List<String> to;
            List<String> cc;
            List<String> bcc;

            foreach (Email email in emails)
            {
                to = email.originalEmail!.Headers[HeaderId.To] != null ? email.originalEmail!.Headers[HeaderId.To].Trim().Split(',').ToList()
                                                                         : null!;
                cc = email.originalEmail!.Headers[HeaderId.Cc] != null ? email.originalEmail!.Headers[HeaderId.Cc].Trim().Split(',').ToList()
                                                                         : null!;
                bcc = email.originalEmail!.Headers[HeaderId.Bcc] != null ? email.originalEmail!.Headers[HeaderId.Bcc].Trim().Split(',').ToList()
                                                                           : null!;

                if (to != null
                    && (senderMailbox = ValidateMailboxAddressFromStringList(to, username)) != null)
                {
                    return senderMailbox!;
                }
                else if (cc != null
                         && (senderMailbox = ValidateMailboxAddressFromStringList(cc, username)) != null)
                {
                    return senderMailbox!;
                }
                else if (bcc != null
                         && (senderMailbox = ValidateMailboxAddressFromStringList(bcc, username)) != null)
                {
                    return senderMailbox!;
                }
            }

            return senderMailbox!;
        }

        private static MailboxAddress ValidateMailboxAddressFromStringList(List<String> addresses, String username)
        {
            return MailboxAddress.Parse(addresses.Where(address => address.Trim().Contains(username, StringComparison.OrdinalIgnoreCase))
                                                 .FirstOrDefault());
        }
        #endregion Inbox and GetUserMailboxAddress

        private static void ReportLoginStatus(String status,
                               TextBlock statusTextBlock,
                               Window statusTextBlockOwner)
        {
            if (statusTextBlock != null
                && statusTextBlockOwner != null)
            {
                statusTextBlockOwner.Dispatcher.Invoke(() =>
                {
                    statusTextBlock.Text = status;
                });
            }
        }
    }
}

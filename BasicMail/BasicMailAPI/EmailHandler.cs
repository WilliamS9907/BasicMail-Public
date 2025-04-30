
using BasicMailAPI.Generic;
using BasicMailSharedClasses;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.IO;
using System.Security;
using System.Windows;

namespace BasicMailAPI
{
    public static class EmailHandler
    {
        #region Send Email
        public static async Task SendEmail(MimeMessage email, String smtpAddress, Int32 smtpPort, 
                                           String username, SecureString password)
        {
            await Task.Run(() =>
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Connect(smtpAddress, smtpPort, false);

                    smtp.Authenticate(username, Common.SecureStringToString(password));

                    smtp.Send(email);

                    smtp.Disconnect(true);
                }
            });
        }
        #endregion Send Email

        #region Send Reply
        public static MimeMessage CreateReply(MimeMessage originalEmail, String sendReplyInput, Boolean replyToAll)
        {
            MimeMessage reply = new MimeMessage
            {
                Subject = originalEmail.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase) ? "Re: " + originalEmail.Subject
                                                                                                        : originalEmail.Subject,
                Body = GetBodyOfReply(originalEmail, sendReplyInput),
                InReplyTo = originalEmail.MessageId,
                Sender = Globals.usersMailboxAddress
            };

            if (originalEmail.ReplyTo.Count > 0)
            {
                reply.To.AddRange(originalEmail.ReplyTo);
            }
            else if (originalEmail.From.Count > 0)
            {
                reply.To.AddRange(originalEmail.From);
            }
            else if (originalEmail.Sender != null)
            {
                reply.To.Add(originalEmail.Sender);
            }

            if (replyToAll)
            {
                reply.To.AddRange(originalEmail.To);
                reply.Cc.AddRange(originalEmail.Cc);
            }

            foreach (String referenceId in originalEmail.References)
            {
                reply.References.Add(referenceId);
            }

            reply.References.Add(originalEmail.MessageId);

            return reply;
        }

        private static TextPart GetBodyOfReply(MimeMessage originalEmail, String sendReplyInput)
        {
            using (StringWriter bodyBuilder = new StringWriter())
            {
                MailboxAddress? sender = originalEmail.Sender ?? originalEmail.From.Mailboxes.FirstOrDefault();

                bodyBuilder.WriteLine(sendReplyInput);

                bodyBuilder.WriteLine("On {0}, {1} wrote:",
                                      originalEmail.Date.ToString("f"),
                                      !String.IsNullOrEmpty(sender!.Name) ? sender.Name : sender.Address);

                using (StringReader reader = new StringReader(originalEmail.TextBody))
                {
                    String inputForReply;

                    while ((inputForReply = reader.ReadLine()!) != null)
                    {
                        bodyBuilder.Write("> ");
                        bodyBuilder.WriteLine(inputForReply);
                    }
                }

                return new TextPart("plain")
                {
                    Text = bodyBuilder.ToString()
                };
            }
        }
        #endregion Send Reply

        #region New Email
        public static MimeMessage CreateNewEmail(String subject, String body, String to, String cc, String bcc)
        {
            MimeMessage email = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart("plain")
                {
                    Text = body
                },
                Sender = Globals.usersMailboxAddress
            };

            email.To.Add(new MailboxAddress("", to));

            if (!String.IsNullOrEmpty(cc)
                && !String.IsNullOrWhiteSpace(cc))
            {
                email.Cc.AddRange(ConvertStringListToMailboxAddresses(cc.Split(',').ToList()));
            }

            if (!String.IsNullOrEmpty(bcc)
                && !String.IsNullOrWhiteSpace(bcc))
            {
                email.Bcc.AddRange(ConvertStringListToMailboxAddresses(bcc.Split(',').ToList()));
            }

            return email;
        }

        private static List<MailboxAddress> ConvertStringListToMailboxAddresses(List<String> rawMailboxAddresses)
        {
            List<MailboxAddress> convertedMailboxAddresses = new List<MailboxAddress>();

            foreach (String mailboxAddress in rawMailboxAddresses)
            {
                convertedMailboxAddresses.Add(new MailboxAddress("", mailboxAddress.Trim()));
            }

            return convertedMailboxAddresses;
        }
        #endregion New Email
    }
}

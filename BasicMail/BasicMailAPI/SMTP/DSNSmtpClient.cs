using MailKit;
using MailKit.Net.Smtp;
using MimeKit;


//This is unneeded as Gmail API handles DSN for us.
//I implemented this very, very bare bones DSN SMTP client to be expanded upon
//in the case it is requested that I add DSN handling functionality for external email servers

public class DSNSmtpClient : SmtpClient
{
    public DSNSmtpClient()
    {
    }

    protected override string GetEnvelopeId(MimeMessage message)
    {
        return message.MessageId;
    }

    protected override DeliveryStatusNotification? GetDeliveryStatusNotifications(MimeMessage message, MailboxAddress mailbox)
    {
        return DeliveryStatusNotification.Failure;
    }
}
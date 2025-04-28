Libraries: Mailkit, Mimekit, Newtonsoft, BasicMailAPI (in-house), BasicMailSharedClasses (in-house)
Language: C#
Framework/Frontend: WPF

For Composing:
Cc and Bcc both use commas as a delimiter for multiple emails.

Example:
william@cabbage4sage.com, cfcmodwill@gmail.com

The To field can only take a single email.

BasicMail is a basic email client that loads the newest 50 emails (doesn't check for statuses such as unread, read, etc.)
from a mailbox address and allows for replying/composing new emails based off of a pre-determined SMTP/IMAP server.

Nothing special to build, just launch it and build through VS. (I use Enterprise, but any version should be fine)

For Gmail:
1. Navigate to https://support.google.com/accounts/answer/185833/
2. Click on "Create and manage your app passwords"
3. Log in and create an app password for BasicMail
4. Launch BasicMail
5. Sign in with email and generated app password

NOTE: Gmail was used to design, program and test this application. You can change the SMTP/IMAP and it *should* work fine, 
but just be aware that it follows Google's specific formatting standards for Mailbox Addresses, Messages, et. al.

To change SMTP/IMAP:
1. Launch BasicMail
2. Click on Options
3. Click on Email
4. Change SMTP & IMAP Address/Port to your specifications
5. Click Save

Passwords *are not* stored in app config. Only SMTP/IMAP Server/Address and email are stored. I wasn't comfortable storing passwords
locally without access to a form of OAuth-type hashable key or programming my own solution for keyword hashing/salting using my SQL server.

The former requires that I have access to the Google API which, unfortunately, I don't and the latter would've taken a significant chunk
of time. I prioritized getting the deliverable in on time so I hope not storing passwords isn't an issue as I would rather not risk 
user security if I don't have time to write a solution.

If you'd like me to write a solution storing passwords let me know and I can do so.

To access errors/stored actions:
1. Click Options
2. Click Action Log

The action log defaults to the current app instances log. Each new app instance creates a new log. By clicking Open Log you can load in a previous
instances action log to review information such as successfully sent emails, unsuccessfully sent emails, auth failures and all data
related to those/other catches.


I didn't go too crazy on the front-end due to time constraints but I can also whip up a full custom-suite frameworked
frontend if you'd like me to!

Let me know if you have any questions! 



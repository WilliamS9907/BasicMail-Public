using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicMailAPI.Generic
{
    public class Globals
    {
        public static MailboxAddress? usersMailboxAddress { get; set; }
        public static IMailFolder? inbox { get; set; }
    }
}

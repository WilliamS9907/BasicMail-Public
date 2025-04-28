
using MimeKit;
using System.Text.Json.Serialization;

namespace BasicMailSharedPublicStructs
{
    public static class PublicStructs
    {
        [Serializable]
        public enum EmailStatus
        {
            Sent,
            Failed
        }

        [Serializable]
        public struct Email
        {
            public String? Subject { get; set; }
            public String? Body { get; set; }
            public String? Sender { get; set; }
            public String? Date { get; set; }
            public MimeMessage? originalEmail { get; set; }
        }

        [Serializable]
        public struct ActionLogEntry
        {
            public String title { get; set; }
            public String from { get; set; }
            public String to { get; set; }
            public String cc { get; set; }
            public String bcc { get; set; }
            public String subject { get; set; }
            public String body { get; set; }
            public EmailStatus status { get; set; }
            public String stack { get; set; }
        }
    }
}

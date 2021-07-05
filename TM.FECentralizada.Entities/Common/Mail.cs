using System.Collections.Generic;

namespace TM.FECentralizada.Entities.Common
{
    public class Mail
    {
        public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<Attachment> ListAttachment { get; set; }
        public string Subject { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string Host { get; set; }
    }
}

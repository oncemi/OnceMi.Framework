using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace OnceMi.Framework.Util.Api.Email
{
    class MailMessageBuilder
    {
        private string MailFrom { get; set; }

        private string Subject { get; set; }

        private List<string> SendTo { get; set; } = new List<string>();

        private List<string> CcTo { get; set; } = new List<string>();

        private string Body { get; set; }

        private bool IsBodyHtml { get; set; }

        public MailMessageBuilder(string mailFrom, string subject, string sendto, string body, bool isBodyHtml, string ccto = "")
        {
            this.MailFrom = mailFrom;

            this.Subject = subject;

            this.SendTo.Add(sendto);

            this.Body = body;

            this.IsBodyHtml = isBodyHtml;

            if (!string.IsNullOrEmpty(ccto))
            {
                this.CcTo.Add(ccto);
            }
        }

        public MailMessageBuilder(string mailFrom, string subject, List<string> sendto, string body, bool isBodyHtml, List<string> ccto = null)
        {
            this.MailFrom = mailFrom;

            this.Subject = subject;

            this.SendTo = sendto;

            this.Body = body;

            this.IsBodyHtml = isBodyHtml;

            this.CcTo = ccto;
        }

        public MailMessage Message()
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(MailFrom, "EasyLinker");
            foreach (var item in SendTo)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                msg.To.Add(item);
            }

            foreach (var item in CcTo)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                msg.CC.Add(item);
            }

            msg.Subject = Subject;//邮件标题   
            msg.Body = Body;//邮件内容   
            msg.BodyEncoding = Encoding.UTF8;//邮件内容编码   
            msg.IsBodyHtml = IsBodyHtml;//是否是HTML邮件   
            msg.Priority = MailPriority.High;//邮件优先级
            return msg;
        }
    }
}

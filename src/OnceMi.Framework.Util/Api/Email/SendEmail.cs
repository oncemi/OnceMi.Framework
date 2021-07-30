using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Util.Api.Email
{
    public class SendEmail
    {
        /// <summary>
        /// 邮件主机
        /// </summary>
        private string Host { get; set; }

        /// <summary>
        /// 邮件地址
        /// </summary>
        private string Address { get; set; }

        /// <summary>
        /// 邮件密码
        /// </summary>
        private string Passwd { get; set; }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="host">邮件主机</param>
        /// <param name="address">邮件地址</param>
        /// <param name="passwd">邮件密码</param>
        public SendEmail(string host, string address, string passwd)
        {
            this.Host = host;
            this.Address = address;
            this.Passwd = passwd;
        }

        /// <summary>
        /// 发送HTML邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="sendto">发送给</param>
        /// <param name="body">邮件主体</param>
        /// <param name="ccto">抄送 可选</param>
        /// <returns></returns>
        public bool SendHtmlMail(string subject, string sendto, string body, string ccto = "")
        {
            if (string.IsNullOrEmpty(subject))
            {
                return false;
            }
            if (string.IsNullOrEmpty(sendto))
            {
                return false;
            }
            if (string.IsNullOrEmpty(body))
            {
                return false;
            }
            //邮件发送客户端
            MailClient mailClient = new MailClient(Host, Address, Passwd);
            //创建邮件对象
            MailMessageBuilder buildMsg = new MailMessageBuilder(Address, subject, sendto, body, true);
            MailMessage msg = buildMsg.Message();
            return mailClient.Send(msg);
        }

        /// <summary>
        /// 发送普通文本邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="sendto">发送给</param>
        /// <param name="text">邮件主体</param>
        /// <param name="ccto">抄送 可选</param>
        /// <returns></returns>
        public bool SendTextMail(string subject, string sendto, string text, string ccto = "")
        {
            if (string.IsNullOrEmpty(subject))
            {
                return false;
            }
            if (string.IsNullOrEmpty(sendto))
            {
                return false;
            }
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            //邮件发送客户端
            MailClient mailClient = new MailClient(Host, Address, Passwd);
            //创建邮件对象
            MailMessageBuilder buildMsg = new MailMessageBuilder(Address, subject, sendto, text, false);
            MailMessage msg = buildMsg.Message();
            return mailClient.Send(msg);
        }
    }
}

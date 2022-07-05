using System;
using System.Net.Mail;

namespace OnceMi.Framework.Util.Api.Email
{
    class MailClient
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

        public MailClient(string host, string address, string passwd)
        {
            this.Host = host;
            this.Address = address;
            this.Passwd = passwd;
        }

        /// <summary>
        /// 邮件发送客户端
        /// </summary>
        private SmtpClient Client
        {
            get
            {
                SmtpClient client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式    
                client.Host = Host;//邮件服务器
                client.UseDefaultCredentials = true;
                client.Credentials = new System.Net.NetworkCredential(Address, Passwd);//用户名、密码
                return client;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Send(MailMessage msg)
        {
            if (msg == null)
            {
                return false;
            }
            try
            {
                SmtpClient client = Client;
                client.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Mail Send failed. Error:{ex.Message}");
            }
        }
    }
}

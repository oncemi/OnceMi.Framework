using System;
using System.Collections.Generic;
using System.Text;

namespace OnceMi.Framework.Util.Http
{
    public class CookieFormat
    {
        public static string Encode(string userid, string content)
        {
            if (string.IsNullOrEmpty(userid)
                || string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException("Cookie model cannot null.");
            }
            return $"USER:{userid};CONTENT:{content}";
        }

        public static string Encode(CookieModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("Cookie model cannot null.");
            }
            return Encode(model.UserId, model.Content);
        }

        public static CookieModel Decode(string cookie)
        {
            if (string.IsNullOrEmpty(cookie))
            {
                throw new Exception("Cookie string cannot null.");
            }

            string[] items = cookie.Split(';');
            if (items.Length != 2)
                throw new Exception("Unknow state cookie format!");
            string[] userItems = items[0].Split(':');
            if (userItems.Length != 2)
                throw new Exception("Unknow state cookie userid format!");
            string[] passwordItem = items[1].Split(':');
            if (passwordItem.Length != 2)
                throw new Exception("Unknow state cookie password format!");
            string userid = userItems[1];
            string password = passwordItem[1];

            return new CookieModel()
            {
                UserId = userid,
                Content = password
            };
        }
    }

    public class CookieModel
    {
        public string UserId { get; set; }

        public string Content { get; set; }
    }
}

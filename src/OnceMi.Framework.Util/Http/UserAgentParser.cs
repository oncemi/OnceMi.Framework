using Microsoft.AspNetCore.Http;
using OnceMi.Framework.Util.Json;
using System.Net;
using UAParser;

namespace OnceMi.Framework.Util.Http
{
    public class UserAgentParser
    {
        private readonly HttpContext _context;
        private readonly ClientInfo _client = null;

        public UserAgentParser(HttpContext context)
        {
            _context = context;
            string userAgent = _context.Request.Headers["User-Agent"];
            if (!string.IsNullOrEmpty(userAgent))
            {
                var uaParser = Parser.GetDefault();
                _client = uaParser.Parse(userAgent);
            }
        }

        /// <summary>
        /// 获取客户端操作系统版本
        /// </summary>
        /// <returns></returns>
        public string GetOS()
        {
            if (_client == null) return "Other";
            return _client.OS.ToString();
        }

        /// <summary>
        /// 获取浏览器名称
        /// </summary>
        /// <returns></returns>
        public string GetBrowser()
        {
            if (_client == null) return "Other";
            return _client.UA.ToString();
        }

        public string GetDevice()
        {
            if (_client == null) return "Other";
            return _client.Device.ToString();
        }

        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <returns></returns>
        public string GetRequestIpAddress()
        {
            try
            {
                string remoteIpAddress = null;
                //反向代理
                if (_context.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    remoteIpAddress = _context.Request.Headers["X-Forwarded-For"];
                }
                else if (_context.Request.Headers.ContainsKey("X-Real-IP"))
                {
                    remoteIpAddress = _context.Request.Headers["X-Real-IP"];
                }
                else
                {
                    remoteIpAddress = _context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                }
                //判断是否为IP地址
                if (!IPAddress.TryParse(remoteIpAddress, out _))
                {
                    return "0.0.0.0";
                }
                return remoteIpAddress;
            }
            catch
            {
                return "0.0.0.0";
            }
        }
    }
}

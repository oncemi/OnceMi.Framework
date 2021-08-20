using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Microsoft.AspNetCore.Http;
using OnceMi.Framework.Util.Json;
using System.Net;

namespace OnceMi.Framework.Util.Http
{
    public class RequestHelper
    {
        private readonly HttpContext _context;
        private readonly DeviceDetector _deviceDetector;

        public RequestHelper(HttpContext context)
        {
            DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE);
            _context = context;
            _deviceDetector  = new DeviceDetector(_context.Request.Headers["User-Agent"]);
            //set DeviceDetector
            _deviceDetector.SetCache(new DictionaryCache());
            // OPTIONAL: If called, GetBot() will only return true if a bot was detected  (speeds up detection a bit)
            _deviceDetector.DiscardBotInformation();
            // OPTIONAL: If called, bot detection will completely be skipped (bots will be detected as regular devices then)
            _deviceDetector.SkipBotDetection();
            _deviceDetector.Parse();
        }

        /// <summary>
        /// 获取客户端操作系统版本
        /// </summary>
        /// <returns></returns>
        public string GetOSName()
        {
            var os = _deviceDetector.GetOs();
            if(os == null || os.Match == null || !os.Success)
            {
                return "未知";
            }
            return JsonUtil.SerializeToString(os.Match);
        }

        /// <summary>
        /// 获取浏览器名称
        /// </summary>
        /// <returns></returns>
        public string GetBrowser()
        {
            var browser = _deviceDetector.GetBrowserClient();
            if (browser == null || browser.Match == null || !browser.Success)
            {
                return "未知";
            }
            return JsonUtil.SerializeToString(browser.Match);
        }

        public string GetDevice()
        {
            var device = _deviceDetector.GetDeviceName();
            if (string.IsNullOrEmpty(device))
            {
                return "未知";
            }
            return JsonUtil.SerializeToString(new
            {
                Name = device
            });
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

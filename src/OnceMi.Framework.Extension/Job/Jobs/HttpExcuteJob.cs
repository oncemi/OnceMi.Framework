using FreeRedis;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using OnceMi.Framework.IService.Admin;
using OnceMi.Framework.Model;
using OnceMi.Framework.Model.Common;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Json;
using Quartz;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OnceMi.Framework.Extension.Job
{
    [DisallowConcurrentExecution]
    public class HttpExcuteJob : BaseJob
    {
        private readonly ILogger<HttpExcuteJob> _logger;
        private readonly IServer _server;
        private readonly RedisClient _redis;

        public HttpExcuteJob(ILogger<HttpExcuteJob> logger
            , IJobService jobsService
            , IServer server
            , RedisClient redis) : base(jobsService, logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._server = server ?? throw new ArgumentNullException(nameof(server));
            this._redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        public override async Task<object> Execute(IJobExecutionContext context, Entity.Admin.Job job)
        {
            string url;
            bool isInnerRequest = !job.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase);
            if (isInnerRequest)
                url = GetEndpoint(job.Url);
            else
                url = job.Url;
            if (!Enum.TryParse(job.RequestMethod, true, out Method method))
                throw new JobExcuteException($"Can not parse request method({job.RequestMethod})");
            //ResultObject<object> result;
            var result = await ExecuteHttpRequest(url, method, job.RequestHeader, job.RequestParam, isInnerRequest, context.CancellationToken);
            if (result == null)
            {
                throw new JobExcuteException($"Job request result is null, method:{job.RequestMethod}, url:{url}");
            }
            if (isInnerRequest)
            {
                if (string.IsNullOrEmpty(result.Content))
                {
                    throw new JobExcuteException($"Job request result is null, method:{job.RequestMethod}, url:{url}");
                }
                ResultObject<object> resultObj = JsonUtil.DeserializeStringToObject<ResultObject<object>>(result.Content);
                if (resultObj.Code != 0)
                {
                    throw new JobExcuteException($"Job request result code is {resultObj.Code}, message:{resultObj.Message},source data:{result.Content}")
                    {
                        Result = resultObj
                    };
                }
                return resultObj;
            }
            else
            {
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    throw new JobExcuteException($"Job request failed, HttpStatusCode is {result.StatusCode}.")
                    {
                        Result = result.Content ?? null,
                    };
                }
                return result.Content;
            }
        }

        private string GetEndpoint(string jobPath)
        {
            string endpoint = null;
            var address = _server.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (address == null || address.Length == 0)
            {
                throw new Exception("Can not get current app endpoint.");
            }
            if (address.Length > 1)
            {
                foreach (var item in address)
                {
                    if (item.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        endpoint = item;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(endpoint))
                {
                    endpoint = address[0];
                }
            }
            else
            {
                endpoint = address[0];
            }
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Can not get current app endpoint.");
            }
            var uri = Regex.Replace(endpoint, @"^(?<scheme>https?):\/\/((\+)|(\*)|\[::\]|(0.0.0.0))(?=[\:\/]|$)", "${scheme}://localhost");
            Uri httpEndpoint = new Uri(uri, UriKind.Absolute);
            return new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port, jobPath).ToString();
        }

        private Dictionary<string, string> DeserializeJsonToDictionary(string header)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(header))
            {
                return values;
            }
            Dictionary<string, JsonElement> headerValues = JsonUtil.DeserializeStringToObject<Dictionary<string, JsonElement>>(header);
            if (headerValues == null || headerValues.Count == 0)
            {
                return values;
            }
            JsonValueKind[] allAllowTypes = new JsonValueKind[]
            {
                JsonValueKind.String,
                JsonValueKind.False,
                JsonValueKind.True,
                JsonValueKind.Number
            };
            foreach (var item in headerValues)
            {
                if (allAllowTypes.Contains(item.Value.ValueKind))
                {
                    string value = item.Value.ToString();
                    if (!values.ContainsKey(item.Key) && !string.IsNullOrEmpty(value))
                    {
                        values.Add(item.Key, value);
                    }
                }
            }
            return values;
        }

        private async Task<IRestResponse> ExecuteHttpRequest(string url
            , Method method
            , string headers
            , string @params
            , bool isInnerRequest
            , CancellationToken cancellationToken)
        {
            var client = new RestClient()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };
            var request = new RestRequest(url, method)
            {
                Timeout = 360000, //一个小时
            };
            //处理header
            if (!string.IsNullOrEmpty(headers))
            {
                Dictionary<string, string> headersDic = DeserializeJsonToDictionary(headers);
                request.AddHeaders(headersDic);
            }
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.61 Safari/537.36");
            request.AddHeader("Accept", "text/html,application/json,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            if (isInnerRequest)
            {
                string jobApiKey = Guid.NewGuid().ToString("N");
                request.AddHeader("JobKey", jobApiKey);
                //cache set
                _redis.Set(CacheConstant.GetJobApiKey(jobApiKey), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), TimeSpan.FromSeconds((8 + new Random().Next(5))));
            }
            //处理参数
            if (!string.IsNullOrEmpty(@params))
            {
                switch (method)
                {
                    case Method.GET:
                        {
                            Dictionary<string, string> requestParams = DeserializeJsonToDictionary(@params);
                            foreach (var item in requestParams)
                            {
                                request.AddQueryParameter(item.Key, item.Value, true);
                            }
                        }
                        break;
                    case Method.POST:
                        {
                            request.AddJsonBody(@params, "application/json");
                        }
                        break;
                }
            }
            var result = await client.ExecuteAsync(request, cancellationToken);
            return result;
        }
    }
}

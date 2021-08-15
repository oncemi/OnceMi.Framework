using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Util.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnceMi.Framework.Api.Controllers.v2
{
    [ApiController]
    [ApiVersion(ApiVersions.V2)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly EndpointDataSource _endpoint;
        private readonly IServer _server;

        public TestController(EndpointDataSource endpoint
            , IServer server)
        {
            _endpoint = endpoint;
            _server = server;
        }

        [HttpGet]
        [Route("[action]")]
        [AllowAnonymous]
        public Task<string> Endpoint()
        {
            //string json = JsonUtil.SerializeToString(_endpoint);
            return Task.FromResult(GetListenAddress());
        }

        private string GetListenAddress()
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
            return endpoint.Replace("*", "localhost").Replace("[::]", "localhost").Replace("0.0.0.0", "localhost");
        }
    }
}

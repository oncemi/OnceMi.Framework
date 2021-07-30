using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnceMi.Framework.Model.Dto;
using OnceMi.Framework.Model.Enums;
using OnceMi.Framework.Model.Exception;
using OnceMi.Framework.Util.Date;
using OnceMi.Framework.Util.Json;
using OnceMi.Framework.Util.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OnceMi.Framework.Api.Controllers.v1.Admin
{
    /// <summary>
    /// 软件注册
    /// </summary>
    [ApiController]
    [ApiVersion(ApiVersions.V1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class GrantController : ControllerBase
    {
        const string _publicKey = "<RSAKeyValue><Modulus>3WtmgMBr04w8N3NVaktDFNRZNem0nZhyG3dy3+4IUCeHiIAswgXuIL/t4jeDtVrXwsOfvPDcjV+iF6Fqj3ewRSAU+M1Yxx75u1w/RgZBnOUMy6pl5famagYilxzF5+BuFv3OcXtvJoh4DMEm/lIQqqWmfXRjV/TW1O5XB1kj78aKp2bxyPf3wTSRUyPIfj0WBorlagu4RD7dWCCDYp8+sPqGAq/7210AnsO0R7nAijjsQq4ioGZ/UgwdBiJbV2wPtu0iSQHGHxMgex4VbXahT0a2RSnUr/tFim/7XvEV2ibLLAHxNRsrsS9Ohbg+YdDySl0PTuQfJrxUW/D23H3pOQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        /// <summary>
        /// 根据机器码获取注册码
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public SoftGrantResponse Get([FromQuery] SoftGrantRequest requestModel)
        {
            //验证token

            string code = requestModel.Code.Replace("-", "");
            StringBuilder codeSb = new StringBuilder();
            codeSb.Append(code);
            int less = code.Length % 4 == 0 ? 0 : 4 - code.Length % 4;
            if (less != 0)
            {
                for (int i = 0; i < less; i++)
                {
                    codeSb.Append('=');
                }
            }
            try
            {
                string decodeCode = Encrypt.Base64Decode(codeSb.ToString());
                if (string.IsNullOrEmpty(decodeCode))
                {
                    throw new BusException(50004, "机器码错误。");
                }
                string[] values = decodeCode.Split('|');
                if (values.Length < 2 || !values[^1].StartsWith('v'))
                {
                    throw new BusException(50005, "机器码错误。");
                }
                List<string> hdwInfos = values.ToList();
                hdwInfos.RemoveAt(values.Length - 1);
                object licenceObj = new
                {
                    ProduectName = "",
                    MachineCodes = hdwInfos,
                    SignTime = TimeUtil.Timestamp(),
                    EndTime = TimeUtil.DateTimeToUnixTimeStamp(DateTime.MaxValue),
                    Version = values[^1],
                    SignName = "oncemi.com",
                    SignFor = "",
                };
                string licence = new RsaXmlUtil(Encoding.UTF8, _publicKey).EncryptBigData(JsonUtil.SerializeToString(licenceObj), RSAEncryptionPadding.Pkcs1);
                return new SoftGrantResponse()
                {
                    Licence = licence,
                    EndTime = DateTime.MaxValue,
                    Version = values[^1]
                };
            }
            catch (Exception ex)
            {
                throw new BusException(50009, "错误的机器码。", ex);
            }
        }
    }
}

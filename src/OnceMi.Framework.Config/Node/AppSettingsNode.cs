using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    public class AppSettingsNode
    {
        public bool IsSeedDatabase { get; set; }

        public int AppId { get; set; }

        public string Host { get; set; }

        private string _developerRoleName = null;

        public string DeveloperRoleName
        {
            get
            {
                return _developerRoleName;
            }
            set
            {
                //当配置文件中开发者人员为空时，创建一个随机guid
                if (string.IsNullOrEmpty(value))
                {
                    _developerRoleName = Guid.NewGuid().ToString();
                }
                else
                {
                    _developerRoleName = value;
                }
            }
        }

        private string _aesSecretKey = null;

        public string AESSecretKey
        {
            get
            {
                return _aesSecretKey;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length < 16)
                {
                    throw new ArgumentNullException(nameof(AESSecretKey), "加密密钥不能为空，且至少需要16位");
                }
                _aesSecretKey = value;
            }
        }

        private string _aesVector = null;

        public string AESVector
        {
            get
            {
                return _aesVector;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length < 16)
                {
                    throw new ArgumentNullException(nameof(AESVector), "加密向量不能为空，且至少需要16位");
                }
                _aesVector = value;
            }
        }

        public bool IsEnabledRequestLog { get; set; }

        public HealthCheckNode HealthCheck { get; set; }
    }
}

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

        public string FileUploadPath { get; set; }

        public bool IsEnableRequestLog { get; set; }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Config
{
    public class ConfigManager
    {
        private static ConfigManager _manager = null;
        private static readonly object _locker = new object();

        [JsonIgnore]
        public IConfiguration Configuration { get; private set; }

        public static ConfigManager Instance
        {
            get
            {
                if (_manager == null)
                {
                    throw new Exception("Please load config at first.");
                }
                return _manager;
            }
        }

        public ConfigManager(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(IConfiguration));
            this.Configuration = configuration;
        }

        public AppSettingsNode AppSettings
        {
            get
            {
                return GetSection<AppSettingsNode>();
            }
        }

        public List<DbConnectionStringsNode> DbConnectionStrings
        {
            get
            {
                return GetSection<List<DbConnectionStringsNode>>();
            }
        }

        public RedisSettingNode RedisSetting
        {
            get
            {
                return GetSection<RedisSettingNode>();
            }
        }

        public FileUploadNode FileUpload
        {
            get
            {
                return GetSection<FileUploadNode>();
            }
        }

        public TokenManagementNode TokenManagement
        {
            get
            {
                return GetSection<TokenManagementNode>();
            }
        }

        public IdentityServerNode IdentityServer
        {
            get
            {
                return GetSection<IdentityServerNode>();
            }
        }

        #region Methods

        public void Load()
        {
            if (_manager == null)
            {
                lock (_locker)
                {
                    if (_manager == null)
                    {
                        _manager = this;
                    }
                }
            }
        }

        private T GetSection<T>(string sectionName = null)
        {
            string nameofT = typeof(T).Name;
            if (string.IsNullOrEmpty(nameofT) && string.IsNullOrEmpty(sectionName))
            {
                return default;
            }
            if (string.IsNullOrEmpty(sectionName))
            {
                int index = nameofT.LastIndexOf("Node");
                if (index > 0 && index == nameofT.Length - 4)
                {
                    nameofT = nameofT.Substring(0, index);
                }
            }
            else
            {
                nameofT = sectionName;
            }
            IConfigurationSection section = this.Configuration.GetSection(nameofT);
            if (section == null || !section.Exists())
            {
                return default;
            }
            return section.Get<T>();
        }

        /// <summary>
        /// 根据Key获取value
        /// Exp: AppSettings:OSS:AccessKey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(key);
            return Configuration.GetValue<string>(key);
        }

        /// <summary>
        /// 加载配置json文件
        /// </summary>
        /// <param name="hostingContext"></param>
        /// <param name="configuration"></param>
        /// <exception cref="Exception"></exception>
        public static void LoadAppsettings(HostBuilderContext hostingContext, IConfigurationBuilder configuration)
        {
            string baseConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Base.json");
            if (!File.Exists(baseConfigPath))
            {
                throw new Exception($"Base app config not exist. Please check file '{baseConfigPath}'");
            }
            configuration.AddJsonFile(baseConfigPath, optional: false, reloadOnChange: true);

            string normalConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(normalConfigPath))
            {
                configuration.AddJsonFile(normalConfigPath, optional: false, reloadOnChange: true);
            }

            string eventName = hostingContext.HostingEnvironment.EnvironmentName;
            if (!string.IsNullOrEmpty(eventName))
            {
                string eventAppConfigPath = Path.Combine(AppContext.BaseDirectory, $"appsettings.{eventName}.json");
                if (File.Exists(eventAppConfigPath))
                {
                    configuration.AddJsonFile(eventAppConfigPath, optional: false, reloadOnChange: true);
                }
            }
        }

        #endregion
    }
}

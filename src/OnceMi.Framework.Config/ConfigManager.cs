using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Config
{
    public class ConfigManager
    {
        private static ConfigManager _manager = null;

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

        public void Load()
        {
            try
            {
                _manager = this;
            }
            catch (Exception ex)
            {
                throw new Exception($"Load config failed. {ex.Message}");
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
            if(section == null || !section.Exists())
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
    }
}

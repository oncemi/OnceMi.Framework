using OnceMi.Framework.Entity.Admin;
using System;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    public class UploadFileInfo
    {
        public long? Id { get; set; }

        public string Name { get; set; }

        public string OldName { get; set; }

        public string Path { get; set; }

        public long? Size { get; set; }

        public string Url { get; set; }

        public long? Owner { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StorageType StorageType { get; set; }

        public string BucketName { get; set; }

        /// <summary>
        /// 访问模式
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileAccessMode AccessMode { get; set; }

        /// <summary>
        /// 到期时间
        /// </summary>
        public DateTime? ExpiredTime { get; set; }
    }
}

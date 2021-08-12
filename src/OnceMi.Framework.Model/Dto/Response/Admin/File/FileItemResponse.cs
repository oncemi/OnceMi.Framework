using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using OnceMi.Framework.Util.Extensions;
using System;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    [MapperFrom(typeof(UpLoadFiles))]
    public class FileItemResponse : IResponse
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string OldName { get; set; }

        public string Path { get; set; }

        /// <summary>
        /// Base64 存放
        /// </summary>
        public string Content { get; set; }

        public long? Size { get; set; }

        public string  StorageTypeName
        {
            get
            {
                return this.StorageType.GetDescription();
            }
        }

        /// <summary>
        /// 储存类型
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StorageType StorageType { get; set; }

        public string BucketName { get; set; }

        public string AccessModeName
        {
            get
            {
                return this.AccessMode.GetDescription();
            }
        }

        /// <summary>
        /// 是否公有
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileAccessMode AccessMode { get; set; }

        public int Version { get; set; } = 1;

        public string Remark { get; set; }

        /// <summary>
        /// 文件URL，在Service中查询
        /// </summary>
        public string Url { get; set; }

        public long CreatedUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        public string OwnerName
        {
            get
            {
                return !string.IsNullOrEmpty(Owner?.NickName) ? Owner?.NickName : Owner?.UserName;
            }
        }

        public UserItemResponse Owner { get; set; }
    }
}

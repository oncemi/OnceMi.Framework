using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Entity.Admin
{
    /// <summary>
    /// 上传文件表
    /// </summary>
    [Table(Name = nameof(UpLoadFiles))]
    public class UpLoadFiles : IBaseEntity<long>
    {
        [MaxLength(120)]
        [Column(IsNullable = false)]
        public string Name { get; set; }

        [MaxLength(120)]
        [Column(IsNullable = true)]
        public string OldName { get; set; }

        [MaxLength(300)]
        [Column(IsNullable = true)]
        public string Path { get; set; }

        /// <summary>
        /// Base64 存放
        /// </summary>
        [Column(DbType = "text", IsNullable = true)]
        public string Content { get; set; }

        [Column(IsNullable = true)]
        public long? Size { get; set; }

        public StorageType StorageType { get; set; }

        [MaxLength(60)]
        [Column(IsNullable = true)]
        public string BucketName { get; set; }

        /// <summary>
        /// 是否公有
        /// </summary>
        public FileAccessMode AccessMode { get; set; }

        public int Version { get; set; } = 1;

        [MaxLength(300)]
        public string Remark { get; set; }

        /// <summary>
        /// 文件URL，在Service中查询
        /// </summary>
        [Column(IsIgnore = true)]
        public string Url { get; set; }

        [Navigate(nameof(CreatedUserId))]
        public Users Owner { get; set; }

    }

    public enum StorageType
    {
        [Description("对象储存")]
        OSS = 1,

        [Description("本地")]
        Local = 2,

        [Description("Base64")]
        Base64 = 3,
    }

    public enum FileAccessMode
    {
        /// <summary>
        /// 公共读
        /// </summary>
        [Description("公共读")]
        PublicRead = 1,

        /// <summary>
        /// 公共读写
        /// </summary>
        [Description("公共读写（暂不支持）")]
        PublicReadAndWrite = 3,

        /// <summary>
        /// 公共写
        /// </summary>
        [Description("公共写（暂不支持）")]
        PublicWrite = 5,

        /// <summary>
        /// 私有
        /// </summary>
        [Description("私有")]
        Private = 7,

        /// <summary>
        /// 内部
        /// </summary>
        [Description("内部")]
        Inside = 9,
    }
}

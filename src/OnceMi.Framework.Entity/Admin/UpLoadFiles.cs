using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Entity.Admin
{
    public enum StorageType
    {
        OSS = 1,
        Local = 2,
        Base64 = 3,
    }

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
        public bool IsPublic { get; set; } = false;

        public int Version { get; set; } = 1;

        [MaxLength(300)]
        public string Remark { get; set; }

        /// <summary>
        /// 文件URL，在Service中查询
        /// </summary>
        [Column(IsIgnore = true)]
        public string Url { get; set; }

    }
}

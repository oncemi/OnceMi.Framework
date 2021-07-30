using OnceMi.Framework.Entity.Admin;
using OnceMi.Framework.Model.Attributes;
using System.ComponentModel.DataAnnotations;

namespace OnceMi.Framework.Model.Dto
{
    [MapperTo(typeof(UpLoadFiles))]
    public class UploadFileRequest : IRequest
    {
        [Required(ErrorMessage = "文件名称不能为空")]
        public string FileName { get; set; }

        [Required(ErrorMessage = "历史文件名称不能为空")]
        public string FileOldName { get; set; }

        public string UploadUid { get; set; }

        public string Path { get; set; }

        public long Size { get; set; }

        public string Url { get; set; }

        public long DbId { get; set; }
    }
}

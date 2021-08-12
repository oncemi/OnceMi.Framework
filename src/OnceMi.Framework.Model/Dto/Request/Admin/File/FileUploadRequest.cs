using OnceMi.Framework.Entity.Admin;
using System;
using System.Text.Json.Serialization;

namespace OnceMi.Framework.Model.Dto
{
    public class FileUploadRequest : IRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileAccessMode AccessMode { get; set; }

        public DateTime? ExpiredTime { get; set; }
    }
}

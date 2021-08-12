using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    public class FileUploadNode
    {
        public bool IsUploadToOSS { get; set; }

        public string BucketName { get; set; }

        public string FileUploadPath { get; set; }
    }
}

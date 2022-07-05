namespace OnceMi.Framework.Config
{
    public class FileUploadNode
    {
        public bool IsUploadToOSS { get; set; }

        public string BucketName { get; set; }

        public string FileUploadPath { get; set; }
    }
}

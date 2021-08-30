using System.Collections.Generic;

namespace OnceMi.Framework.Model.Dto
{
    public class ApiDocInfo
    {
        public string Controller { get; set; }

        public string ControllerName { get; set; }

        public string OperationId { get; set; }

        public string Version { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Method { get; set; }

        public List<string> Parameters { get; set; }
    }
}

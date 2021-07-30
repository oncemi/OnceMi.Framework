using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Common
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

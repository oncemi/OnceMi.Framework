using System;

namespace OnceMi.Framework.Model.Dto
{
    public class SoftGrantResponse : IResponse
    {
        public string Version { get; set; }

        public string Licence { get; set; }

        public DateTime EndTime { get; set; }
    }
}

using System;

namespace OnceMi.AspNetCore.MQ.Models
{
    class JobMember
    {
        public string Id { get; set; }

        public long Source { get; set; }

        public string Data { get; set; }

        public string Channel { get; set; }
    }
}

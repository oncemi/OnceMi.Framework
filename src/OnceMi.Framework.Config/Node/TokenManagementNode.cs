using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Config
{
    public class TokenManagementNode
    {
        public string Secret { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int AccessExpiration { get; set; }

        public int RefreshExpiration { get; set; }
    }
}

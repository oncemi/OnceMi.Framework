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

        private int _accessExpiration = 3600;

        public int AccessExpiration
        {
            get
            {
                return _accessExpiration;
            }
            set
            {
                if(value < 60)
                {
                    throw new Exception("Access expiration can not less than 60 seconds");
                }
                _accessExpiration = value;
            }
        }

        private int _refreshExpiration = 604800;

        public int RefreshExpiration
        {
            get
            {
                return _refreshExpiration;
            }
            set
            {
                if (value < 3600)
                {
                    throw new Exception("Refresh expiration can not less than 3600 seconds");
                }
                _refreshExpiration = value;
            }
        }
    }
}

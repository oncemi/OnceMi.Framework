﻿namespace OnceMi.Framework.Config
{
    public class IdentityServerNode
    {
        public bool IsEnabledIdentityServer { get; set; }

        public string Url { get; set; }

        public string Audience { get; set; }

        public bool RequireHttps { get; set; }
    }
}

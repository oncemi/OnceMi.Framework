using Microsoft.AspNetCore.Mvc;
using OnceMi.AspNetCore.AutoInjection;
using OnceMi.Framework.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnceMi.Framework.Extension.Helpers
{
    public static class ApiVersionHelper
    {
        public static List<ApiVersion> GetAllApiVersions()
        {
            List<ApiVersion> result = new List<ApiVersion>();
            List<Type> types = new AssemblyLoader(p => p.Name.StartsWith(GlobalConfigConstant.FirstNamespace, StringComparison.OrdinalIgnoreCase)).DomainAllTypes;
            if (types == null || types.Count == 0)
                return result;
            foreach (var item in types)
            {
                Attribute[] attributes = System.Attribute.GetCustomAttributes(item, true);
                if (attributes == null || attributes.Length == 0)
                    continue;
                foreach (Attribute attrItem in attributes)
                {
                    if (attrItem is not ApiVersionAttribute)
                        continue;
                    var attrVersions = ((ApiVersionAttribute)attrItem).Versions;
                    foreach (var attrVersion in attrVersions)
                    {
                        if (result.Count > 0 && result.FindIndex(p => p.ToString() == attrVersion.ToString()) >= 0)
                            continue;
                        result.Add(attrVersion);
                    }
                }
            }
            return result.OrderBy(p => p.MajorVersion).ToList();
        }
    }
}

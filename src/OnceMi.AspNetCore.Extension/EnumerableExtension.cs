using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnceMi.AspNetCore.Extension
{
    public static class EnumerableExtension
    {
        public static bool Any(this IEnumerable<string> source, string dist, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (source == null || source.Count() == 0)
                return false;
            if (dist == null)
                return false;
            foreach (var item in source)
            {
                if (item.Equals(dist, stringComparison)) 
                    return true;
            }
            return false;
        }
    }
}

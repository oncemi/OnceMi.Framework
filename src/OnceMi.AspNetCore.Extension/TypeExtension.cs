using System;
using System.Collections.Generic;
using System.Text;

namespace OnceMi.AspNetCore.Extension
{
    public static class TypeExtension
    {
        public static bool IsBoolean(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return t == typeof(bool);
        }

        public static bool IsTrueEnum(this Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;

            return t.IsEnum;
        }
    }
}

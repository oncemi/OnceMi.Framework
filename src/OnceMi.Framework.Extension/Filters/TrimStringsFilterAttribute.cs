using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnceMi.Framework.Extension.Filters
{
    /// <summary>
    /// 去除请求string中前后空格
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TrimStringsFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var changes = new Dictionary<string, object>();
            foreach (var arg in actionContext.ActionArguments.Where(a => a.Value != null))
            {
                var type = arg.Value.GetType();
                if (IsEnumerable(type))
                {
                    changes[arg.Key] = TrimEnumerable((IEnumerable)arg.Value);
                }
                else if (IsComplexObject(type))
                {
                    changes[arg.Key] = TrimObject(arg.Value);
                }
            }
            foreach (var change in changes)
            {
                actionContext.ActionArguments[change.Key] = change.Value;
            }
        }

        private static IEnumerable TrimEnumerable(IEnumerable value)
        {
            var enumerable = value as object[] ?? value.Cast<object>().ToArray();
            return enumerable.OfType<string>().Any() ?
                        enumerable.Cast<string>().Select(s => s == null
                                ? null
                                : s.Trim())
                        : enumerable.Select(TrimObject);
        }

        private static bool IsEnumerable(Type t)
        {
            return t.IsAssignableFrom(typeof(IEnumerable));
        }

        private static bool IsComplexObject(Type value)
        {
            return value.IsClass && !value.IsArray;
        }

        private static object TrimObject(object argValue)
        {
            if (argValue == null) return null;
            var argType = argValue.GetType();
            if (IsEnumerable(argType))
            {
                TrimEnumerable((IEnumerable)argValue);
            }
            var s = argValue as string;
            if (s != null)
            {
                return s.Trim();
            }
            if (!IsComplexObject(argType))
            {
                return argValue;
            }
            var props = argType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(prop => prop.PropertyType == typeof(string))
                    .Where(prop => prop.GetIndexParameters().Length == 0)
                    .Where(prop => prop.CanWrite && prop.CanRead);

            foreach (var prop in props)
            {
                var value = (string)prop.GetValue(argValue, null);
                if (value != null)
                {
                    value = value.Trim();
                    prop.SetValue(argValue, value, null);
                }
            }
            return argValue;
        }
    }
}

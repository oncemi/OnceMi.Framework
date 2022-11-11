using AutoMapper.Internal;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OnceMi.Framework.Util.Cache
{
    public static class MemoryCacheExtensions
    {
        private static T GetPrivateField<T>(object instance, string fieldname)
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException($"Missing {fieldname} field in {type.FullName}.");
            }
            var obj = field.GetValue(instance);
            if (obj != null && obj is T)
            {
                return (T)obj;
            }
            return default(T);
        }

        private static T GetPrivateProperty<T>(object instance, string fieldname)
        {
            Type type = instance.GetType();
            PropertyInfo propertyInfo = type.GetProperty(fieldname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propertyInfo == null)
            {
                throw new MissingFieldException($"Missing {fieldname} property in {type.FullName}.");
            }
            var obj = propertyInfo.GetValue(instance);
            if (obj != null && obj is T)
            {
                return (T)obj;
            }
            return default(T);
        }

        private static IDictionary GetEntriesCollection(MemoryCache memoryCache)
        {
            try
            {
                bool isDisposed = GetPrivateField<bool>(memoryCache, "_disposed");
                if (isDisposed)
                {
                    throw new ObjectDisposedException($"{nameof(memoryCache)} has disposed.");
                }
                object coherentState = GetPrivateField<object>(memoryCache, "_coherentState");
                if (coherentState == null)
                {
                    throw new ArgumentNullException("coherentState field is null");
                }
                IDictionary entries = GetPrivateProperty<IDictionary>(coherentState, "EntriesCollection");
                if(entries == null)
                {
                    throw new ArgumentNullException("EntriesCollection property is null");
                }
                return entries;
            }
            catch (Exception ex)
            {
                throw new Exception($"Getting EntriesCollection property info by reflection failed, {ex.Message}", ex);
            }
        }

        public static IEnumerable GetKeys(this IMemoryCache memoryCache) =>
            GetEntriesCollection((MemoryCache)memoryCache).Keys;

        public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) =>
            GetKeys(memoryCache).OfType<T>();
    }
}

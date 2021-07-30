using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace OnceMi.AspNetCore.Extension
{
    public static class EnumExtension
    {
        /// <summary>
        ///  获取枚举的中文描述
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum obj)
        {
            try
            {
                string objName = obj.ToString();
                Type t = obj.GetType();
                FieldInfo fi = t.GetField(objName);
                DescriptionAttribute[] arrDesc = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return arrDesc[0].Description;
            }
            catch
            {
                return null;
            }
        }
    }
}

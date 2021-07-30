using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace OnceMi.Framework.Util.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        ///  获取枚举的中文描述
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Enum obj)
        {
            try
            {
                string objName = obj.ToString();
                FieldInfo fi = obj.GetType().GetField(objName);
                if (fi == null) return null;
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

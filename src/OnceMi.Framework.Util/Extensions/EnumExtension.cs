using System.ComponentModel;
using System.Reflection;

namespace OnceMi.Framework.Util.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        ///  获取枚举的中文描述
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Enum value)
        {
            try
            {
                Type enumType = value.GetType();
                // 获取枚举常数名称。
                string name = System.Enum.GetName(enumType, value);
                if (name != null)
                {
                    // 获取枚举字段。
                    FieldInfo fieldInfo = enumType.GetField(name);
                    if (fieldInfo != null)
                    {
                        // 获取描述的属性。
                        DescriptionAttribute attr = Attribute.GetCustomAttribute(fieldInfo,
                            typeof(DescriptionAttribute), false) as DescriptionAttribute;
                        if (attr != null)
                        {
                            return attr.Description;
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

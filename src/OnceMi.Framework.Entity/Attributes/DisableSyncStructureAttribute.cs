
namespace OnceMi.Framework.Entity
{
    /// <summary>
    /// 禁用表结构同步
    /// 使用该Attribute的Entity不会被同步至数据库
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DisableSyncStructureAttribute : Attribute
    {

    }
}

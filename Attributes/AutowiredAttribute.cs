using System;

namespace EasyInject.Attributes
{
    /// <summary>
    /// author: spyn
    /// description: Autowired特性（用于标注这是个可以被依赖注入的字段或构造函数的参数上）
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AutowiredAttribute : Attribute
    {
        public string Name { get; }

        public AutowiredAttribute(string name)
        {
            Name = name;
        }

        public AutowiredAttribute()
        {
            Name = string.Empty;
        }
    }
}